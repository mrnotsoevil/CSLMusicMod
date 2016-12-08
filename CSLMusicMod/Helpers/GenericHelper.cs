using System;
using System.Collections.Generic;

namespace CSLMusicMod
{
    public static class GenericHelper
    {
        public static List<T> CopyOrCreateList<T>(IEnumerable<T> source)
        {
            if (source == null)
                return new List<T>();
            else
                return new List<T>(source);
        }
    }
}

