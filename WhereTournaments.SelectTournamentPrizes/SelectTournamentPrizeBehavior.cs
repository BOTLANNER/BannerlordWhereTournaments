using System;
using System.Collections.Generic;
using System.Linq;

using SandBox.View.Map;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace WhereTournaments.SelectTournamentPrizes
{
    internal class SelectTournamentPrizeBehavior : CampaignBehaviorBase, ITournamentTrackingPlugin
    {

        public override void RegisterEvents()
        {
            //Set as first plugin to execute
            TournamentTrackingBehavior.Current?.TournamentTrackingPlugins.Insert(0, this);
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
            if (townTournament != null && townTournament.Tournament != null && townTournament.Tournament is FightTournamentGame tournamentGame)
            {
                var descriptionText = new TextObject("{=where_tournaments_select_prizes_str_select_cat}{TOWN}: Select the prize category or confirm the current prize.");
                descriptionText.SetTextVariable("TOWN", townTournament.Town!.Settlement.EncyclopediaLinkWithName);

                var regular = tournamentGame.GetPossibleRegularRewardItemObjectsCache();
                if (regular == null || regular.Count == 0)
                {
                    tournamentGame.CachePossibleRegularRewardItems();
                    regular = tournamentGame.GetPossibleRegularRewardItemObjectsCache();
                }

                var elite = tournamentGame.GetPossibleEliteRewardItemObjectsCache();
                if (elite == null || elite.Count == 0)
                {
                    tournamentGame.CachePossibleEliteRewardItems();
                    elite = tournamentGame.GetPossibleEliteRewardItemObjectsCache();
                }

                var banners = tournamentGame.GetPossibleBannerRewardItemObjectsCache();
                if (banners == null || banners.Count == 0)
                {
                    //tournamentGame.CachePossibleBannerItems(true);
                    banners = Campaign.Current.Models.BannerItemModel.GetPossibleRewardBannerItems().ToList();
                    if (banners != null && banners.Count > 0)
                    {
                        tournamentGame.SetPossibleBannerRewardItemObjectsCache(banners);
                    }
                }

                var currentText = new TextObject("{=where_tournaments_select_prizes_str_select_current_prize}Confirm the current prize.");

                var bannersText = new TextObject("{=where_tournaments_select_prizes_str_select_prizes}Select the prize from {CATEGORY} or confirm the current prize.");
                bannersText.SetTextVariable("CATEGORY", new TextObject("{=where_tournaments_select_prizes_str_banner_cat}available banners"));

                var regularText = new TextObject("{=where_tournaments_select_prizes_str_select_prizes}Select the prize from {CATEGORY} or confirm the current prize.");
                regularText.SetTextVariable("CATEGORY", new TextObject("{=where_tournaments_select_prizes_str_regular_cat}available regular items"));

                var eliteText = new TextObject("{=where_tournaments_select_prizes_str_select_prizes}Select the prize from {CATEGORY} or confirm the current prize.");
                eliteText.SetTextVariable("CATEGORY", new TextObject("{=where_tournaments_select_prizes_str_elite_cat}available elite items"));

                TextObject titleText = new TextObject("{=where_tournaments_select_prizes_str_select_prize_title}Select Prize for {TOWN}.");
                titleText.SetTextVariable("TOWN", townTournament.Town!.Settlement!.EncyclopediaLinkWithName);

                TextObject confirmText = new TextObject("{=where_tournaments_select_prizes_str_select_button}Confirm");

                List<InquiryElement> inquiryElements1 = new()
                {
                    new InquiryElement(PrizeCategorySelection.Current, tournamentGame.Prize.Name.ToString(), new ItemImageIdentifier(tournamentGame.Prize), true, currentText.ToString() + "\r\n\r\n" + tournamentGame.Prize.ToToolTipTextObject().ToString() ),
                };
                if (banners != null && banners.Count > 0)
                {
                    inquiryElements1.Add(new InquiryElement(PrizeCategorySelection.Banners, new TextObject("{=where_tournaments_select_prizes_str_banner_cat_short}Banners").ToString(), new ItemImageIdentifier(banners.FirstOrDefault()), true, bannersText.ToString()));
                }
                if (regular != null && regular.Count > 0)
                {
                    inquiryElements1.Add(new InquiryElement(PrizeCategorySelection.Regular, new TextObject("{=where_tournaments_select_prizes_str_regular_cat_short}Regular").ToString(), new ItemImageIdentifier(regular.FirstOrDefault()), true, regularText.ToString()));
                }
                if (elite != null && elite.Count > 0)
                {
                    inquiryElements1.Add(new InquiryElement(PrizeCategorySelection.Elite, new TextObject("{=where_tournaments_select_prizes_str_elite_cat_short}Elite").ToString(), new ItemImageIdentifier(elite.FirstOrDefault()), true, eliteText.ToString()));
                }

                Action queryCategories = () =>
                {
                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        titleText: titleText.ToString(),
                        descriptionText: descriptionText.ToString(),
                        inquiryElements: inquiryElements1,
                        isExitShown: true,
                        maxSelectableOptionCount: 1,
                        minSelectableOptionCount: 0,
                        affirmativeText: confirmText.ToString(),
                        negativeText: null,
                        affirmativeAction: (List<InquiryElement> args) =>
                        {
                            List<InquiryElement> source = args;
                            InformationManager.HideInquiry();

                            if (Enum.TryParse<PrizeCategorySelection>(args.FirstOrDefault()?.Identifier?.ToString(), out PrizeCategorySelection selection))
                            {
                                List<ItemObject>? selectionList = null;
                                TextObject? descriptionText = null;
                                List<InquiryElement> inquiryElementList = new List<InquiryElement>();
                                inquiryElementList.Add(new InquiryElement(tournamentGame.Prize, tournamentGame.Prize.Name.ToString(), new ItemImageIdentifier(tournamentGame.Prize), true, currentText.ToString() + "\r\n\r\n" + tournamentGame.Prize.ToToolTipTextObject().ToString()));
                                switch (selection)
                                {
                                    case PrizeCategorySelection.Banners:
                                        selectionList = banners;
                                        descriptionText = bannersText;
                                        break;
                                    case PrizeCategorySelection.Regular:
                                        selectionList = regular;
                                        descriptionText = regularText;
                                        break;
                                    case PrizeCategorySelection.Elite:
                                        selectionList = elite;
                                        descriptionText = eliteText;
                                        break;

                                    case PrizeCategorySelection.Current:
                                    default:
                                        deferredPluginInvoke.Invoke();
                                        return;
                                }

                                inquiryElementList.AddRange(selectionList.Select(s => new InquiryElement(s, s.Name.ToString(), new ItemImageIdentifier(s), true, s.ToToolTipTextObject().ToString())));
                                Action<float>? querySelection = null;
                                
                                querySelection = (f) =>
                                {
                                    Main.OnTick -= querySelection!;

                                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                                        titleText: titleText.ToString(),
                                        descriptionText: descriptionText.ToString(),
                                        inquiryElements: inquiryElementList,
                                        isExitShown: true,
                                        maxSelectableOptionCount: 1,
                                        minSelectableOptionCount: 0,
                                        affirmativeText: confirmText.ToString(),
                                        negativeText: null,
                                        affirmativeAction: (List<InquiryElement> args) =>
                                        {
                                            List<InquiryElement> source = args;
                                            InformationManager.HideInquiry();
                                            var selected = args.FirstOrDefault()?.Identifier as ItemObject;
                                            if (selected != null && selected.ToString() != tournamentGame.Prize?.ToString())
                                            {
                                                tournamentGame.UpdateTournamentPrize(true, false);
                                                tournamentGame.ForceSetPrize(selected);
                                            }
                                            deferredPluginInvoke.Invoke();
                                        },
                                        negativeAction: null,
                                        soundEventPath: "")
                                        ,
                                        false,
                                        false);
                                };
                                Main.OnTick += querySelection;


                                return;
                            }

                            deferredPluginInvoke.Invoke();
                        },
                        negativeAction: null,
                        soundEventPath: "")
                        ,
                        false,
                        false);
                };

                MapScreen.Instance.MapNotificationView.RegisterMapNotificationType(typeof(SelectTournamentPrizeMapNotification), typeof(SelectTournamentPrizeMapNotificationItemVM));

                MBInformationManager.AddNotice(new SelectTournamentPrizeMapNotification(townTournament, deferredPluginInvoke, queryCategories, descriptionText));

                breakPluginExecution = true;
                return true;
            }
            breakPluginExecution = false;
            return false;
        }

    }


    public class SelectTournamentPrizeMapNotification : InformationData
    {
        public SelectTournamentPrizeMapNotification(TownTournamentItem townTournament, Action deferredPluginInvoke, Action queryCategories, TextObject description) : base(description)
        {
            TownTournament = townTournament;
            DeferredPluginInvoke = deferredPluginInvoke;
            QueryCategories = queryCategories;
        }


        public override TextObject TitleText
        {
            get
            {
                TextObject titleText = new TextObject("{=where_tournaments_select_prizes_str_select_prize_title}Select Prize for {TOWN}.");
                titleText.SetTextVariable("TOWN", TownTournament.Town!.Settlement!.EncyclopediaLinkWithName);
                return titleText;
            }
        }

        public TownTournamentItem TownTournament { get; }
        public Action DeferredPluginInvoke { get; }
        public Action QueryCategories { get; }
        public override string? SoundEventPath { get; }

        public override bool IsValid()
        {
            var actual = Campaign.Current.TournamentManager.GetTournamentGame(TownTournament.Town);
            return !(actual == null || (actual != TownTournament.Tournament && actual.CreationTime != TownTournament.Tournament!.CreationTime));
        }
    }

    public class SelectTournamentPrizeMapNotificationItemVM : MapNotificationItemBaseVM
    {
        public SelectTournamentPrizeMapNotificationItemVM(SelectTournamentPrizeMapNotification data) : base(data)
        {
            base.NotificationIdentifier = "vote";
            this._onInspect = () =>
            {
                data.QueryCategories?.Invoke();
                base.ExecuteRemove();
            };
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
        }
    }
}