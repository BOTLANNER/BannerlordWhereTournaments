using System;
using System.Collections.Generic;
using System.Linq;

using Helpers;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace WhereTournaments
{
    public class TournamentTrackingBehavior : CampaignBehaviorBase
    {
        public List<ITournamentTrackingPlugin> TournamentTrackingPlugins = new List<ITournamentTrackingPlugin>();

        public static TournamentTrackingBehavior? Current { get; private set; }

        public TournamentTrackingBehavior()
        {
            Current = this;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnAfterNewGameCreated));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public void OnAfterNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            this.AddGameMenus(campaignGameStarter);
        }

        protected void AddGameMenus(CampaignGameStarter campaignGameSystemStarter)
        {
            campaignGameSystemStarter.AddGameMenuOption("town_arena", "where_tournaments_find_all", "{=where_tournaments_str_tournament_find_all}Find all tournaments.", new GameMenuOption.OnConditionDelegate(game_menu_town_arena_find_tournaments_on_condition), new GameMenuOption.OnConsequenceDelegate(a => game_menu_town_arena_find_tournaments_on_consequence(a, false)), false, 0, false);

        }

        private void game_menu_town_arena_find_tournaments_on_consequence(MenuCallbackArgs args, bool nearby)
        {
            var list = Town.AllTowns.Select(t =>
            {
                return new TownTournamentItem
                {
                    Town= t,
                    Tournament = Campaign.Current.TournamentManager.GetTournamentGame(t)
                };
            }).Where(item =>
            {
                if (item.Tournament == null)
                {
                    return false;
                }

                return item.Town != Settlement.CurrentSettlement.Town;
            }).ToList();

            var ordered = (
                    from x in list
                    orderby x.Town.Settlement.Position2D.DistanceSquared(Settlement.CurrentSettlement.Position2D)
                    select x);

            if (nearby)
            {
                list = ordered.Take(4).ToList();
            }
            else
            {
                list = ordered.ToList();
            }

            TournamentGame? currentTournament = null;
            bool currentTownHasTournament = Settlement.CurrentSettlement.IsTown && (currentTournament = Campaign.Current.TournamentManager.GetTournamentGame(Settlement.CurrentSettlement.Town)) != null;
            
            string tournamentInCurrentText = new TextObject("{=where_tournaments_str_tournament_here}There is currently a tournament in this town's arena.").ToString();
            List<InquiryElement> inquiryElementList = new List<InquiryElement>();
            if (currentTownHasTournament)
            {
                currentTournament!.UpdateTournamentPrize(true, false);
                var townDays = new TextObject("{=where_tournaments_str_tournament_days_left}{TOWN} - {DAYS} days left");
                townDays.SetTextVariable("TOWN", Settlement.CurrentSettlement!.EncyclopediaLinkWithName.ToString());
                townDays.SetTextVariable("DAYS", ((int)(currentTournament!.RemoveTournamentAfterDays - (CampaignTime.Now - currentTournament!.CreationTime).ToDays)).ToString());
                inquiryElementList.Add(new InquiryElement(Settlement.CurrentSettlement.Town, townDays.ToString(), new ImageIdentifier(currentTournament!.Prize, Settlement.CurrentSettlement!.Owner?.ClanBanner?.Serialize()), false, $"{tournamentInCurrentText} \r\n\r\n{currentTournament.GetMenuText().ToString()}"));
            }
            foreach (var item in list.Where(item => item != null && item.Town.Settlement != null))
            {
                try
                {
                    item!.Town!.Settlement.AddMobileParty(MobileParty.MainParty);
                    item!.Tournament!.UpdateTournamentPrize(true, false);
                    item!.Town!.Settlement.RemoveMobileParty(MobileParty.MainParty);
                }
                catch (Exception e)
                {
                    Debug.PrintError(e.ToString(), e.StackTrace);
                }
                var townDays = new TextObject("{=where_tournaments_str_tournament_days_left}{TOWN} - {DAYS} days left");
                townDays.SetTextVariable("TOWN", item.Town!.Settlement!.EncyclopediaLinkWithName.ToString());
                townDays.SetTextVariable("DAYS", ((int) (item.Tournament.RemoveTournamentAfterDays - (CampaignTime.Now - item.Tournament!.CreationTime).ToDays)).ToString());

                bool isTracked = Campaign.Current.VisualTrackerManager.CheckTracked(item.Town.Settlement);
                TextObject trackedHint = new TextObject(isTracked ? "{=where_tournaments_str_tournaments_town_already_tracked}{TOWN} is already being tracked" : "{=where_tournaments_str_tournaments_track_town}Track {TOWN} on the map");
                trackedHint.SetTextVariable("TOWN", item.Town!.Settlement!.EncyclopediaLinkWithName);

                DeferFoundTournament(item, Current!.TournamentTrackingPlugins).Invoke();

                inquiryElementList.Add(new InquiryElement(item, townDays.ToString(), new ImageIdentifier(item.Tournament!.Prize, item.Town!.Settlement!.Owner?.ClanBanner?.Serialize()), !isTracked,$"{trackedHint.ToString()} \r\n\r\n{item.Tournament.GetMenuText().ToString()}"));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                titleText: new TextObject("{=where_tournaments_str_tournaments_found}Found Tournaments.").ToString(),
                descriptionText: currentTownHasTournament ? tournamentInCurrentText : string.Empty,
                inquiryElements: inquiryElementList,
                isExitShown: true,
                maxSelectableOptionCount: -1,
                minSelectableOptionCount: 0,
                affirmativeText: new TextObject("{=where_tournaments_str_tournaments_track_selected}Track Selected").ToString(),
                negativeText: null,
                affirmativeAction: (List<InquiryElement> args) =>
                {
                    List<InquiryElement> source = args;
                    InformationManager.HideInquiry();
                    foreach (var item in source)
                    {
                        var townTournament = (item.Identifier as TownTournamentItem);

                        DeferTrackTournamentTown(townTournament, Current!.TournamentTrackingPlugins).Invoke();

                        if (townTournament != null && townTournament.Town != null && townTournament.Town.Settlement != null && !Campaign.Current.VisualTrackerManager.CheckTracked(townTournament.Town.Settlement))
                        {
                            Campaign.Current.VisualTrackerManager.RegisterObject(townTournament.Town.Settlement); 
                        }
                    }
                },
                negativeAction: null,
                soundEventPath: "")
                ,
                false,
                false);
        }

        private Action DeferFoundTournament(TownTournamentItem? item, List<ITournamentTrackingPlugin>? tournamentTrackingPlugins)
        {
            return () =>
            {
                if (tournamentTrackingPlugins != null && item != null)
                {
                    for (int i = 0; i < tournamentTrackingPlugins.Count; i++)
                    {
                        ITournamentTrackingPlugin? plugin = tournamentTrackingPlugins[i];
                        if (plugin.FoundTournamentTown(item, DeferFoundTournament(item,tournamentTrackingPlugins.Count > i + 1 ? tournamentTrackingPlugins.GetRange(i + 1, tournamentTrackingPlugins.Count - (i + 1)) : null), out bool stop) && stop)
                        {
                            break;
                        }
                    }
                }

            };
        }

        private Action DeferTrackTournamentTown(TownTournamentItem? item, List<ITournamentTrackingPlugin>? tournamentTrackingPlugins)
        {
            return () =>
            {
                if (tournamentTrackingPlugins != null && item != null)
                {
                    for (int i = 0; i < tournamentTrackingPlugins.Count; i++)
                    {
                        ITournamentTrackingPlugin? plugin = tournamentTrackingPlugins[i];
                        if (plugin.TrackTournamentTown(item, DeferTrackTournamentTown(item, tournamentTrackingPlugins.Count > i + 1 ? tournamentTrackingPlugins.GetRange(i + 1, tournamentTrackingPlugins.Count - (i + 1)) : null), out bool stop) && stop)
                        {
                            break;
                        }
                    }
                }

            };
        }

        private bool game_menu_town_arena_find_tournaments_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
            bool shouldBeDisabled = !Campaign.Current.IsDay;
            TextObject? disabledText = null;
            if (shouldBeDisabled)
            {
                disabledText = new TextObject("{=where_tournaments_str_return_during_day}Arena is closed, come back during the day.");
            }

            return MenuHelper.SetOptionProperties(args, true, shouldBeDisabled, disabledText);
        }

        
    }
}