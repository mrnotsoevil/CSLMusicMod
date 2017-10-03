using System;
using System.Linq;
using System.IO;
using UnityEngine;
using CSLMusicMod.LitJson;
using ColossalFramework.IO;
using System.Text;
using System.Collections.Generic;

namespace CSLMusicMod
{
    /// <summary>
    /// Class that wraps the options of the mod.
    /// </summary>
    public class ModOptions
    {
        private static ModOptions _Instance = null;
        public static ModOptions Instance 
        {
            get
            {
                if(_Instance == null)
                {
                    //_Instance = new GameObject("CSLMusicMod Settings").AddComponent<ModOptions>();
                    _Instance = new ModOptions();
                    _Instance.LoadSettings();

                }

                return _Instance;
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

        public bool EnableShortcuts
        {
            get
            {
                return m_Options.EnableShortcuts;
            }
            set
            {
                m_Options.EnableShortcuts = value;
                SaveSettings();
            }
        }

        public Shortcut ShortcutNextTrack
        {
            get
            {
                return m_Options.ShortcutNextTrack;
            }
            set
            {
                m_Options.ShortcutNextTrack = value;
                SaveSettings();
            }
        }

        public Shortcut ShortcutNextStation
        {
            get
            {
                return m_Options.ShortcutNextStation;
            }
            set
            {
                m_Options.ShortcutNextStation = value;
                SaveSettings();
            }
        }

        public Shortcut ShortcutOpenRadioPanel
        {
            get
            {
                return m_Options.ShortcutOpenRadioPanel;
            }
            set
            {
                m_Options.ShortcutOpenRadioPanel = value;
                SaveSettings();
            }
        }

        public bool EnableDisabledContent
        {
            get
            {
                return m_Options.EnableDisabledContent;
            }
            set
            {
                m_Options.EnableDisabledContent = value;
                SaveSettings();
            }
        }

        public List<String> DisabledContent
        {
            get
            {
                return m_Options.DisabledContent;
            }
            set
            {
                m_Options.DisabledContent = value;
                SaveSettings();
            }
        }

        public bool EnableContextSensitivity
        {
            get
            {
                return m_Options.EnableContextSensitivity;
            }
            set
            {
                m_Options.EnableContextSensitivity = value;
                SaveSettings();
            }
        }	

        public bool EnableAddingContentToVanillaStations
        {
            get
            {
                return m_Options.EnableAddingContentToVanillaStations;
            }
            set
            {
                m_Options.EnableAddingContentToVanillaStations = value;
                SaveSettings();
            }
        }

        public bool EnableSmoothTransitions
        {
            get
            {
                return m_Options.EnableSmoothTransitions;
            }
            set
            {
                m_Options.EnableSmoothTransitions = value;
                SaveSettings();
            }
        }

        public List<String> DisabledRadioStations
        {
            get
            {
                return m_Options.DisabledRadioStations;
            }
            set
            {
                m_Options.DisabledRadioStations = value;
                SaveSettings();
            }
        }

        public bool EnableDebugInfo
		{
			get
			{
				return m_Options.EnableDebugInfo;
			}
			set
			{
				m_Options.EnableDebugInfo = value;
				SaveSettings();
			}
		}
        
        public bool AddVanillaSongsToMusicMix
        {
            get
            {
                return m_Options.AddVanillaSongsToMusicMix;
            }
            set
            {
                m_Options.AddVanillaSongsToMusicMix = value;
                SaveSettings();
            }
        }
        
        public bool EnableImprovedRadioStationList
        {
            get
            {
                return m_Options.EnableImprovedRadioStationList;
            }
            set
            {
                m_Options.EnableImprovedRadioStationList = value;
                SaveSettings();
            }
        }
        
        public bool EnableOpenStationDirButton
        {
            get
            {
                return m_Options.EnableOpenStationDirButton;
            }
            set
            {
                m_Options.EnableOpenStationDirButton = value;
                SaveSettings();
            }
        }

        public static String SettingsFilename
        {
            get
            {
                return Path.Combine(DataLocation.applicationBase, "CSLMusicMod.json");
            }
        }

        public ModOptions()
        {
        }

        //public void Awake()
        //{
        //    DontDestroyOnLoad(this);
        //    LoadSettings();
        //}

        public void SaveSettings()
        {
            try
            {
                StringWriter json = new StringWriter();
                JsonWriter f = new JsonWriter(json);
                f.PrettyPrint = true;

                JsonMapper.ToJson(m_Options, f);
                File.WriteAllText(SettingsFilename, json.ToString());

            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                CSLMusicMod.Log("Settings saved.");
            }
        }

        public void LoadSettings()
        {
            if(File.Exists(SettingsFilename))
            {
                try
                {
                    String data = File.ReadAllText(SettingsFilename);
                    m_Options = JsonMapper.ToObject<Options>(data);
                }
                catch(Exception ex)
                {
                    Debug.LogError(ex);
                }
                finally
                {
                    CSLMusicMod.Log("Settings loaded.");
                }
            }
            else
            {
                SaveSettings();
            }
        }

        public class Shortcut
        {
            public KeyCode Key { get; set; }
            public bool ModifierControl { get; set; }
            public bool ModifierAlt { get; set; }
            public bool ModifierShift { get; set; }

            public Shortcut()
            {
                
            }
           
            public Shortcut(KeyCode key, bool ctrl, bool alt, bool shift)
            {
                Key = key;
                ModifierAlt = alt;
                ModifierControl = ctrl;
                ModifierShift = shift;
            }

            public override string ToString()
            {
                return string.Format("[Shortcut: Key={0}, ModifierControl={1}, ModifierAlt={2}, ModifierShift={3}]", Key, ModifierControl, ModifierAlt, ModifierShift);
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

            public bool EnableShortcuts { get; set; }
            public Shortcut ShortcutNextTrack { get; set; }
            public Shortcut ShortcutNextStation { get; set; }
            public Shortcut ShortcutOpenRadioPanel { get; set; }

            public List<String> DisabledContent { get; set; }
            public bool EnableDisabledContent { get; set; }

            public bool EnableContextSensitivity { get; set; }
            public double ContentWatcherInterval { get; set; }

            public bool EnableAddingContentToVanillaStations { get; set; }

            public bool EnableSmoothTransitions { get; set; }

            public List<String> DisabledRadioStations { get; set; }

            public bool EnableDebugInfo { get; set; }
            
            public bool AddVanillaSongsToMusicMix { get; set; }
            public bool EnableImprovedRadioStationList { get; set; }
            public bool EnableOpenStationDirButton { get; set; }

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

                EnableShortcuts = true;
                ShortcutNextTrack = new Shortcut(KeyCode.N, false, false, false);
                ShortcutNextStation = new Shortcut(KeyCode.N, true, false, false);
                ShortcutOpenRadioPanel = new Shortcut(KeyCode.M, false, false, false);

                DisabledContent = new List<string>();
                EnableDisabledContent = true;

                EnableContextSensitivity = true;

                EnableAddingContentToVanillaStations = true;

                EnableSmoothTransitions = true;

                DisabledRadioStations = new List<string>();

                EnableDebugInfo = false;

                AddVanillaSongsToMusicMix = true;
                EnableImprovedRadioStationList = true;
                EnableOpenStationDirButton = false;
            }
        }
    }
}

