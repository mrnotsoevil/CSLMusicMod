using System;
using ColossalFramework;
using UnityEngine;

namespace CSLMusicMod
{
    public class CSLCustomMusicEntry
    {
        public String Name
        {
            get;
            set;
        }

        private bool _enable = true;
        public bool Enable
        {
            get
            {
                return _enable;
            }
            set
            {
                _enable = value;
                CSLMusicModSettings.SaveMusicFileSettings();
            }
        }

        public String GoodMusic { get; private set;}

        public String BadMusic { get; set;}

        private bool _enableBadMusic = true;
        public bool EnableBadMusic
        {
            get
            {
                return _enableBadMusic;
            }
            set
            {
                _enableBadMusic = value;
                CSLMusicModSettings.SaveMusicFileSettings();
            }
        }

        public String SkyMusic { get; set;}

        private bool _enableSkyMusic = true;
        public bool EnableSkyMusic
        {
            get
            {
                return _enableSkyMusic;
            }
            set
            {
                _enableSkyMusic = value;
                CSLMusicModSettings.SaveMusicFileSettings();
            }
        }

        public CSLCustomMusicEntry(bool enable, String name, String good, String bad, bool enable_bad, String sky, bool enable_sky)
        {
            Name = name;
            _enable = enable;
            GoodMusic = good;
            BadMusic = bad;
            _enableBadMusic = enable_bad;
            SkyMusic = sky;
            _enableSkyMusic = enable_sky;
        }

        public CSLCustomMusicEntry(String name, String good, String bad, String sky) 
            : this(true, name, good, bad, true, sky, true)
        {
           
        }

        public String GetMusicFromMood(AudioManager.ListenerInfo info)
        {
            if (EnableSkyMusic && CSLMusicModSettings.HeightDependentMusic && !String.IsNullOrEmpty(SkyMusic) && GetListenerHeight(info) > 1400f)
            {
                return SkyMusic;
            }

            int finalHappiness = (int)Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_finalHappiness;

            if ( EnableBadMusic && CSLMusicModSettings.MoodDependentMusic && !String.IsNullOrEmpty(BadMusic) && finalHappiness < 40)
            {
                return BadMusic;
            }

            return GoodMusic;
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

        public bool Contains(String filename)
        {
            return filename == GoodMusic || filename == BadMusic || filename == SkyMusic;
        }

        public override string ToString()
        {
            return string.Format("[CSLCustomMusicEntry] g:" + GoodMusic  + ", b:" + BadMusic + ", s:" + SkyMusic);
        }
    }
}

