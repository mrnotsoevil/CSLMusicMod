using System;
using UnityEngine;

namespace CSLMusicMod.MusicEntryTags
{
    public class TagVanillaNight : MusicEntryTag
    {
        public TagVanillaNight() : base("night", "#night - Music at nighttime")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            //throw new NotImplementedException();
            //Debug.Log("[CSLMusicMod] #night not implemented yet");

            return false;
        }
    }
}

