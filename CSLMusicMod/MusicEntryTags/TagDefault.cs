using System;

namespace CSLMusicMod.MusicEntryTags
{
    /**
     * The "default music" tag
     * */
    public class TagDefault : MusicEntryTag
    {
        public TagDefault() : base("", "Default music")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            return true;
        }
    }
}

