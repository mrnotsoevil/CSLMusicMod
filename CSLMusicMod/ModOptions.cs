using System;
using System.Linq;
using System.IO;
using UnityEngine;
using CSLMusicMod.LitJson;
using ColossalFramework.IO;
using System.Text;

namespace CSLMusicMod
{
    public class ModOptions : MonoBehaviour
    {
        public static ModOptions Instance 
        {
            get
            {
                var found = Resources.FindObjectsOfTypeAll<ModOptions>();

                if(found.Length == 0)
                {
                    return new GameObject("CSLMusicMod Settings").AddComponent<ModOptions>();
                }

                return found.First();
            }
        }

        private Options m_Options = new Options();

        public bool CreateMixChannels
        {
            get
            {
                return m_Options.CreateMixChannels;
            }
            set
            {
                m_Options.CreateMixChannels = value;
                SaveSettings();
            }
        }

        public bool CreateChannelsFromLegacyPacks
        {
            get
            {
                return m_Options.CreateChannelsFromLegacyPacks;
            }
            set
            {
                m_Options.CreateChannelsFromLegacyPacks = value;
                SaveSettings();
            }
        }

        public bool EnableMusicPacks
        {
            get
            {
                return m_Options.EnableMusicPacks;
            }
            set
            {
                m_Options.EnableMusicPacks = value;
                SaveSettings();
            }
        }

        public bool AllowContentMusic
        {
            get
            {
                return m_Options.AllowContentMusic;
            }
            set
            {
                m_Options.AllowContentMusic = value;
                SaveSettings();
            }
        }

        public bool AllowContentTalk
        {
            get
            {
                return m_Options.AllowContentTalk;
            }
            set
            {
                m_Options.AllowContentTalk = value;
                SaveSettings();
            }
        }

        public bool AllowContentBlurb
        {
            get
            {
                return m_Options.AllowContentBlurb;
            }
            set
            {
                m_Options.AllowContentBlurb = value;
                SaveSettings();
            }
        }

        public bool AllowContentBroadcast
        {
            get
            {
                return m_Options.AllowContentBroadcast;
            }
            set
            {
                m_Options.AllowContentBroadcast = value;
                SaveSettings();
            }
        }

        public bool AllowContentCommercial
        {
            get
            {
                return m_Options.AllowContentCommercial;
            }
            set
            {
                m_Options.AllowContentCommercial = value;
                SaveSettings();
            }
        }

        public bool MixContentMusic
        {
            get
            {
                return m_Options.MixContentMusic;
            }
            set
            {
                m_Options.MixContentMusic = value;
                SaveSettings();
            }
        }

        public bool MixContentTalk
        {
            get
            {
                return m_Options.MixContentTalk;
            }
            set
            {
                m_Options.MixContentTalk = value;
                SaveSettings();
            }
        }

        public bool MixContentBlurb
        {
            get
            {
                return m_Options.MixContentBlurb;
            }
            set
            {
                m_Options.MixContentBlurb = value;
                SaveSettings();
            }
        }

        public bool MixContentBroadcast
        {
            get
            {
                return m_Options.MixContentBroadcast;
            }
            set
            {
                m_Options.MixContentBroadcast = value;
                SaveSettings();
            }
        }

        public bool MixContentCommercial
        {
            get
            {
                return m_Options.MixContentCommercial;
            }
            set
            {
                m_Options.MixContentCommercial = value;
                SaveSettings();
            }
        }

        public bool EnableCustomUI
        {
            get
            {
                return m_Options.EnableCustomUI;
            }
            set
            {
                m_Options.EnableCustomUI = value;
                SaveSettings();
            }
        }

        public String SettingsFilename
        {
            get
            {
                return Path.Combine(DataLocation.applicationBase, "CSLMusicMod.json");
            }
        }

        public ModOptions()
        {
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
            LoadSettings();
        }

        public void SaveSettings()
        {
            try
            {
                StringBuilder json = new StringBuilder();
                JsonWriter f = new JsonWriter(json);
                f.PrettyPrint = true;

                JsonMapper.ToJson(m_Options, f);
                File.WriteAllText(SettingsFilename, json.ToString());

            }
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public void LoadSettings()
        {
            if(File.Exists(SettingsFilename))
            {
                try
                {
                    String data = File.ReadAllText("CSLMusicMod.json");
                    m_Options = JsonMapper.ToObject<Options>(data);
                }
                catch(Exception ex)
                {
                    Debug.Log(ex);
                }
            }
            else
            {
                SaveSettings();
            }
        }

        public class Options
        {
            public bool CreateMixChannels { get; set; }
            public bool CreateChannelsFromLegacyPacks { get; set; }
            public bool EnableMusicPacks { get; set; }

            public bool AllowContentMusic { get; set; }
            public bool AllowContentBlurb { get; set; }
            public bool AllowContentTalk { get; set; }
            public bool AllowContentCommercial { get; set; }
            public bool AllowContentBroadcast { get; set; }

            public bool EnableCustomUI { get; set; }

            public bool MixContentMusic { get; set; }
            public bool MixContentBlurb { get; set; }
            public bool MixContentTalk { get; set; }
            public bool MixContentCommercial { get; set; }
            public bool MixContentBroadcast { get; set; }

            public Options()
            {
                CreateMixChannels = true;
                MixContentBlurb = false;
                MixContentBroadcast = false;
                MixContentCommercial = false;
                MixContentMusic = true;
                MixContentTalk = false;

                CreateChannelsFromLegacyPacks = true;
                EnableMusicPacks = true;
                AllowContentMusic = true;
                AllowContentBlurb = true;
                AllowContentTalk = true;
                AllowContentCommercial = true;
                AllowContentBroadcast = true;
                EnableCustomUI = true;
            }
        }
    }
}

