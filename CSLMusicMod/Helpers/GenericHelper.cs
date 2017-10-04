using System.Collections.Generic;

namespace CSLMusicMod.Helpers
{
    /// <summary>
    /// Helper methods for generics
    /// </summary>
    public static class GenericHelper
    {
        /// <summary>
        /// Creates a list with the contents of <c>source</c>.
        /// If source is null, return an empty list.
        /// </summary>
        /// <returns>List with contents of source, otherwise an empty list</returns>
        /// <param name="source">Source list</param>
        /// <typeparam name="T">Any type</typeparam>
        public static List<T> CopyOrCreateList<T>(IEnumerable<T> source)
        {
            if (source == null)
                return new List<T>();
            else
                return new List<T>(source);
        }
    }
}

