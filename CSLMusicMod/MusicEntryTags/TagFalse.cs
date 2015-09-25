using System;

namespace CSLMusicMod
{
    /**
     * A "false" tag that never applies
     * */
    public class TagFalse : MusicEntryTag
    {
        public TagFalse() : base("/", "-")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            return false;
        }
    }
}

