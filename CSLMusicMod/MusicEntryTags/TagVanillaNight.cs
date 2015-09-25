using System;
using UnityEngine;
using ColossalFramework;

namespace CSLMusicMod.MusicEntryTags
{
    public class TagVanillaNight : MusicEntryTag
    {
        public TagVanillaNight() : base("night", "#night - Music at nighttime")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {     
            return Singleton<SimulationManager>.instance.m_isNightTime;
        }
    }
}

