using System;
using System.Collections.Generic;

namespace CSLMusicMod
{
    public interface RadioContext
    {
        bool Applies();

        HashSet<string> GetCollections();
    }
}

