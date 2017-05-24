using System;
using ColossalFramework;
using System.Collections;

namespace CSLMusicMod
{
    /// <summary>
    /// Helpers to inject operations into the game's loading routines.
    /// </summary>
    public class Loading
    {
        /// <summary>
        /// Queues an action during loading
        /// </summary>
        /// <param name="action">Action.</param>
        public static void QueueLoadingAction(Action action)
        {
            Singleton<LoadingManager>.instance.QueueLoadingAction(ActionWrapper(action));
        }

        /// <summary>
        /// Queues an action that returns a list of data etc. into loading
        /// </summary>
        /// <param name="action">Action.</param>
        public static void QueueLoadingAction(IEnumerator action)
        {
            Singleton<LoadingManager>.instance.QueueLoadingAction(action);
        }

        /// <summary>
        /// Needed if an action is queued.
        /// </summary>
        /// <returns>The wrapper.</returns>
        /// <param name="a">The alpha component.</param>
        private static IEnumerator ActionWrapper(Action a)
        {
            a.Invoke();
            yield break;
        }
    }
}

