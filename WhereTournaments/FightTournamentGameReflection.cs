using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace WhereTournaments
{
    public static class FightTournamentGameReflection
    {
        private static FieldInfo possibleBannerRewardItemObjectsCacheField = typeof(FightTournamentGame).GetField("_possibleBannerRewardItemObjectsCache", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo possibleRegularRewardItemObjectsCacheField = typeof(FightTournamentGame).GetField("_possibleRegularRewardItemObjectsCache", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo possibleEliteRewardItemObjectsCacheField = typeof(FightTournamentGame).GetField("_possibleEliteRewardItemObjectsCache", BindingFlags.Instance | BindingFlags.NonPublic);
        
        //private static MethodInfo cachePossibleBannerItemsMethod = typeof(FightTournamentGame).GetMethod(nameof(CachePossibleBannerItems), BindingFlags.Instance | BindingFlags.NonPublic);
        //private static MethodInfo cachePossibleEliteRewardItemsMethod = typeof(FightTournamentGame).GetMethod(nameof(CachePossibleEliteRewardItems), BindingFlags.Instance | BindingFlags.NonPublic);
        //private static MethodInfo cachePossibleRegularRewardItemsMethod = typeof(FightTournamentGame).GetMethod(nameof(CachePossibleRegularRewardItems), BindingFlags.Instance | BindingFlags.NonPublic);


        public static void CachePossibleBannerItems(this FightTournamentGame fightTournamentGame,bool isElite)
        {
            //cachePossibleBannerItemsMethod.Invoke(fightTournamentGame, new object[] { isElite });


            MBList<ItemObject> regularRewardItems = Campaign.Current.Models.TournamentModel.GetRegularRewardItems((fightTournamentGame as TournamentGame).Town, 1600, 5000);
            var _possibleBannerRewardItemObjectsCache = new MBList<ItemObject>();
            foreach (ItemObject regularRewardItem in regularRewardItems)
            {
                if (!regularRewardItem.IsBannerItem)
                {
                    continue;
                }
                _possibleBannerRewardItemObjectsCache.Add(regularRewardItem);
            }

            possibleBannerRewardItemObjectsCacheField.SetValue(fightTournamentGame, _possibleBannerRewardItemObjectsCache);
        }

        public static void CachePossibleEliteRewardItems(this FightTournamentGame fightTournamentGame)
        {
            //cachePossibleEliteRewardItemsMethod.Invoke(fightTournamentGame, new object[0]);

            var _possibleEliteRewardItemObjectsCache = Campaign.Current.Models.TournamentModel.GetEliteRewardItems((fightTournamentGame as TournamentGame).Town, 1600, 5000);
            _possibleEliteRewardItemObjectsCache.Sort((ItemObject x, ItemObject y) => x.Value.CompareTo(y.Value));

            possibleEliteRewardItemObjectsCacheField.SetValue(fightTournamentGame, _possibleEliteRewardItemObjectsCache);
        }

        public static void CachePossibleRegularRewardItems(this FightTournamentGame fightTournamentGame)
        {
            //cachePossibleRegularRewardItemsMethod.Invoke(fightTournamentGame, new object[0]);

            var _possibleRegularRewardItemObjectsCache = Campaign.Current.Models.TournamentModel.GetRegularRewardItems((fightTournamentGame as TournamentGame).Town, 1600, 5000);
            _possibleRegularRewardItemObjectsCache.Sort((ItemObject x, ItemObject y) => x.Value.CompareTo(y.Value));

            possibleRegularRewardItemObjectsCacheField.SetValue(fightTournamentGame, _possibleRegularRewardItemObjectsCache);
        }



        public static List<ItemObject> GetPossibleBannerRewardItemObjectsCache(this FightTournamentGame fightTournamentGame)
        {
            return (List<ItemObject>) possibleBannerRewardItemObjectsCacheField.GetValue(fightTournamentGame);
        }

        public static bool SetPossibleBannerRewardItemObjectsCache(this FightTournamentGame fightTournamentGame, List<ItemObject> cache)
        {
            try
            {
                possibleBannerRewardItemObjectsCacheField.SetValue(fightTournamentGame, cache);
                return true;
            }
            catch (System.Exception e) { TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString()); Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); }
        
            return false;
        }

        public static List<ItemObject> GetPossibleRegularRewardItemObjectsCache(this FightTournamentGame fightTournamentGame)
        {
            return (List<ItemObject>) possibleRegularRewardItemObjectsCacheField.GetValue(fightTournamentGame);
        }

        public static List<ItemObject> GetPossibleEliteRewardItemObjectsCache(this FightTournamentGame fightTournamentGame)
        {
            return (List<ItemObject>) possibleEliteRewardItemObjectsCacheField.GetValue(fightTournamentGame);
        }
    }
}