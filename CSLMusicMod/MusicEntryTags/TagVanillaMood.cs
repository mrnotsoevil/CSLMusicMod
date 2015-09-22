using System;
using UnityEngine;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagVanillaMood : MusicEntry.MusicEntryTag
    {      

        public TagVanillaMood() : base("bad", 2000)
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            SettingsManager.Options ModOptions = gameObject.GetComponent<SettingsManager>().ModOptions;
            int finalHappiness = (int)Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_finalHappiness;

            return (ModOptions.MoodDependentMusic
                && finalHappiness < ModOptions.MoodDependentMusic_MoodThreshold);
        }
    }
}

