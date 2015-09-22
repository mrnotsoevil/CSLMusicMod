using System;
using ColossalFramework;
using UnityEngine;

namespace CSLMusicMod
{
    /**
     * Vanilla sky music tag implementation
     * */
    public class TagVanillaSky : MusicEntry.MusicEntryTag
    {       
        public TagVanillaSky() : base("sky", 1000)
        {

        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            SettingsManager.Options ModOptions = gameObject.GetComponent<SettingsManager>().ModOptions;

            return (ModOptions.HeightDependentMusic
            && GetListenerHeight(info) > ModOptions.HeightDependentMusic_HeightThreshold);
        }

        private float GetListenerHeight(AudioManager.ListenerInfo listenerInfo)
        {
            if (listenerInfo == null)
                return  0f;

            float listenerHeight;
            if (Singleton<AudioManager>.instance.m_properties != null && Singleton<LoadingManager>.instance.m_loadingComplete)
            {
                listenerHeight = Mathf.Max(0f, listenerInfo.m_position.y - Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(listenerInfo.m_position, true, 0f));
            }
            else
            {
                listenerHeight = 0f;
            }

            return listenerHeight;
        }
    }
}

