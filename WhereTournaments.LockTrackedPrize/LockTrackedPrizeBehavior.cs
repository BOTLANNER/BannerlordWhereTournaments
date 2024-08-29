using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

namespace WhereTournaments.LockTrackedPrize
{
    public class LockTrackedPrizeBehavior : CampaignBehaviorBase, ITournamentTrackingPlugin
    {
        private List<TownTournamentItemPrize> trackedTournaments = new List<TownTournamentItemPrize>();

        public override void RegisterEvents()
        {
            TournamentTrackingBehavior.Current?.TournamentTrackingPlugins.Add(this);

            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, SettlementEntered);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, SettlementTick);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnAfterNewGameCreated));
            CampaignEvents.TournamentFinished.AddNonSerializedListener(this, TournamentFinished);
            CampaignEvents.TournamentCancelled.AddNonSerializedListener(this, TournamentCancelled);
        }

        private void TournamentCancelled(Town town)
        {
            var trackedItem = trackedTournaments.FirstOrDefault(t => t.TownTournamentItem!.Town == town);
            if (trackedItem != null)
            {
                trackedTournaments.Remove(trackedItem);
            }
        }

        public void TournamentFinished(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town, ItemObject prize)
        {
            var trackedItem = trackedTournaments.FirstOrDefault(t => t.TownTournamentItem!.Town == town);
            if (trackedItem != null)
            {
                trackedTournaments.Remove(trackedItem);
            }
        }

        public void OnAfterNewGameCreated(CampaignGameStarter campaignGameSystemStarter)
        {
            try
            {
                campaignGameSystemStarter.AddGameMenuOption("town_arena", "where_tournaments_lock_track", string.Empty, new GameMenuOption.OnConditionDelegate(game_menu_town_arena_lock_track_on_condition),null, false, 99999, false);

            }
            catch (Exception e)
            {
                TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }

        private bool game_menu_town_arena_lock_track_on_condition(MenuCallbackArgs args)
        {
            try
            {
                Settlement currentSettlement = Settlement.CurrentSettlement;
                if (currentSettlement != null)
                {
                    EnsurePrize(currentSettlement);
                }
            }
            catch (Exception e)
            {
                TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }

            //Never actually show, this is just to execute code on arena menu
            return false;
        }

        private void DailyTick()
        {
            try
            {
                TownTournamentItem[] array = trackedTournaments.Select(t => t!.TownTournamentItem!).ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    TownTournamentItem? tracked = array[i];
                    if (tracked != null &&
                       tracked.Town != null &&
                       tracked.Tournament != null &&
                       tracked.Tournament.Prize != null)
                    {
                        var actual = Campaign.Current.TournamentManager.GetTournamentGame(tracked.Town);
                        if (actual == null || (actual != tracked.Tournament && actual.CreationTime != tracked.Tournament.CreationTime))
                        {
                            //Tournament is over. Removing...
                            trackedTournaments.RemoveAt(i);
                            continue;
                        }

                        if (!Campaign.Current.VisualTrackerManager.CheckTracked(tracked.Town.Settlement))
                        {
                            //Town was untracked. Removing...
                            trackedTournaments.RemoveAt(i);
                            continue;
                        }
                    }
                    else
                    {
                        //No such item. Removing...
                        trackedTournaments.RemoveAt(i);
                        continue;
                    }
                }
            }
            catch (System.Exception e)
            {
                TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }

        private void SettlementTick(Settlement settlement)
        {
            if (settlement.IsTown && Settlement.CurrentSettlement == settlement)
            {
                EnsurePrize(settlement);
            }
        }

        private void SettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party == MobileParty.MainParty && settlement.IsTown)
            {
                EnsurePrize(settlement);
            }
        }

        private void EnsurePrize(Settlement settlement)
        {
            try
            {
                var trackedItem = trackedTournaments.FirstOrDefault(t => t.TownTournamentItem!.Town == settlement.Town);
                if (trackedItem != null)
                {
                    TownTournamentItem? townTournament = trackedItem.TownTournamentItem;
                    if (townTournament != null &&
                       townTournament.Town != null &&
                       townTournament.Tournament != null &&
                       townTournament.Tournament.Prize != null)
                    {
                        var tournament = Campaign.Current.TournamentManager.GetTournamentGame(townTournament.Town);
                        if (tournament == null || (tournament != townTournament.Tournament && tournament.CreationTime != townTournament.Tournament.CreationTime))
                        {
                            //Tournament is over. Removing...
                            trackedTournaments.Remove(trackedItem);
                            return;
                        }
                    }
                    else
                    {
                        //No such item. Removing...
                        trackedTournaments.Remove(trackedItem);
                        return;
                    }
                    var actual = Campaign.Current.TournamentManager.GetTournamentGame(townTournament.Town);

                    trackedItem.TownTournamentItem!.Tournament!.UpdateTournamentPrize(true, false);
                    if (trackedItem.Prize?.ToString() != trackedItem.TownTournamentItem!.Tournament!.Prize?.ToString() && actual is FightTournamentGame tournamentGame)
                    {
                        trackedItem.TownTournamentItem!.Tournament!.ForceSetPrize(trackedItem.Prize);
                    }
                }
            }
            catch (System.Exception e)
            {
                TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public bool FoundTournamentTown(TownTournamentItem townTournament, Action deferredPluginInvoke, out bool breakPluginExecution)
        {
            breakPluginExecution = false;
            return false;
        }

        public bool TrackTournamentTown(TownTournamentItem townTournament, Action deferredPluginInvoke, out bool breakPluginExecution)
        {
            breakPluginExecution = false;

            try
            {
                if (townTournament != null && 
                    townTournament.Town != null && 
                    townTournament.Tournament != null && 
                    townTournament.Tournament.Prize != null &&
                    townTournament.Tournament is FightTournamentGame tournamentGame)
                {

                    var found = trackedTournaments.FirstOrDefault(t => t.TownTournamentItem!.Tournament == townTournament.Tournament || (t.TownTournamentItem.Tournament!.CreationTime == townTournament.Tournament.CreationTime && t.TownTournamentItem.Town == townTournament.Town));
                    if (found != null)
                    {
                        found.TownTournamentItem = townTournament;
                        found.Prize = townTournament.Tournament.Prize;
                    }
                    else
                    {
                        trackedTournaments.Add(new TownTournamentItemPrize
                        {
                            TownTournamentItem = townTournament,
                            Prize = townTournament.Tournament.Prize
                        });
                    }
                }
                return true;
            }
            catch (System.Exception e)
            {
                TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
                return false;
            }
        }
    }

    public class TownTournamentItemPrize
    {
        public TownTournamentItem? TownTournamentItem;
        public ItemObject? Prize;
    }
}
