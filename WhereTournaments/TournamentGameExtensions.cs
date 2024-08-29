using System.Reflection;

using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;

namespace WhereTournaments
{
    public static class TournamentGameExtensions
    {
        static MethodInfo setPrize = typeof(TournamentGame).GetMethod($"set_{nameof(TournamentGame.Prize)}", BindingFlags.Instance | BindingFlags.NonPublic);
        public static void ForceSetPrize(this TournamentGame tournamentGame, ItemObject? prize)
        {
            if (prize != null)
            {
                prize.DetermineItemCategoryForItem();
            }

            setPrize.Invoke(tournamentGame, new object[] { new ItemObject(prize) });

            // For some reason category can get cleared when updating the prize on tournament. Recalculate to avoid crashing on inventory.
            if (tournamentGame.Prize != null)
            {
                tournamentGame.Prize.DetermineItemCategoryForItem();
            }
        }
    }
}