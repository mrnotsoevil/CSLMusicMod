using System;
using UnityEngine;

namespace CSLMusicMod
{
    public abstract class MusicEntryTag
    {
        /**
             * The name of the tag; a song's tag is determined by #<Name>
             * */
        public String Name{ get; private set; }

        /***
         * The description of the tag
         * */
        public string Description { get; private set;}

        public MusicEntryTag(String name, String description)
        {
            Name = name.ToLower();
            Description = description;
        }

        /**
             * Returns if this tag applies to the current game situation
             * */
        public abstract bool TagApplies(GameObject gameObject, AudioManager.ListenerInfo info);
    }
}

