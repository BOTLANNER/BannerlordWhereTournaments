using System.Collections.Generic;
using System.Reflection;

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
        
        private static MethodInfo cachePossibleBannerItemsMethod = typeof(FightTournamentGame).GetMethod(nameof(CachePossibleBannerItems), BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo cachePossibleEliteRewardItemsMethod = typeof(FightTournamentGame).GetMethod(nameof(CachePossibleEliteRewardItems), BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo cachePossibleRegularRewardItemsMethod = typeof(FightTournamentGame).GetMethod(nameof(CachePossibleRegularRewardItems), BindingFlags.Instance | BindingFlags.NonPublic);


        public static void CachePossibleBannerItems(this FightTournamentGame fightTournamentGame,bool isElite)
        {
            cachePossibleBannerItemsMethod.Invoke(fightTournamentGame, new object[] { isElite });
        }

        public static void CachePossibleEliteRewardItems(this FightTournamentGame fightTournamentGame)
        {
            cachePossibleEliteRewardItemsMethod.Invoke(fightTournamentGame, new object[0]);
        }

        public static void CachePossibleRegularRewardItems(this FightTournamentGame fightTournamentGame)
        {
            cachePossibleRegularRewardItemsMethod.Invoke(fightTournamentGame, new object[0]);
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