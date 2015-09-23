using System;
using UnityEngine;

namespace CSLMusicMod
{
    public class TagNight : MusicEntryTag
    {
        public TagNight() : base("night", "#night - Music at nighttime")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            //throw new NotImplementedException();
            Debug.Log("[CSLMusicMod] #night not implemented yet");

            return false;
        }
    }
}

