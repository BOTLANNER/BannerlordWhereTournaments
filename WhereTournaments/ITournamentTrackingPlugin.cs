using System;

namespace WhereTournaments
{
    public interface ITournamentTrackingPlugin
    {
        /// <summary>
        /// Executed per town found with tournament. 
        /// Use <paramref name="breakPluginExecution"/> to stop more pugins from executing this method after current.
        /// Use <paramref name="deferredPluginInvoke"/> to resume plugin execution from within plugin.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="deferredPluginInvoke"></param>
        /// <param name="breakPluginExecution"></param>
        /// <returns>Plugin execution success or failure</returns>
        public bool FoundTournamentTown(TownTournamentItem townTournament, Action deferredPluginInvoke, out bool breakPluginExecution);

        /// <summary>
        /// Executed per town with tournament selected for tracking. 
        /// Use <paramref name="breakPluginExecution"/> to stop more pugins from executing this method after current.
        /// Use <paramref name="deferredPluginInvoke"/> to resume plugin execution from within plugin.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="deferredPluginInvoke"></param>
        /// <param name="breakPluginExecution"></param>
        /// <returns>Plugin execution success or failure</returns>
        bool TrackTournamentTown(TownTournamentItem townTournament, Action deferredPluginInvoke, out bool breakPluginExecution);
    }

}