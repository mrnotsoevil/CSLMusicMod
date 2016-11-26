using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ColossalFramework.Plugins;
using CSLMusicMod.IO;
using ColossalFramework;

namespace CSLMusicMod
{
    public class SettingsManager : MonoBehaviour
    {
        private SimpleIni SettingsFile = new SimpleIni("CSLMusicMod_Settings.ini");
        public Options ModOptions = new Options();

        public SettingsManager()
        {
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void SaveModSettings()
        {
            Debug.Log("[CSLMusic] Saving settings ...");

            //SettingsFile.Set("Music Library", "AutoAddMusicTypesForCustomMusic", ModOptions.AutoAddMusicTypesForCustomMusic);

            SettingsFile.Set("Tweaks", "MusicWhileLoading", ModOptions.MusicWhileLoading);
            SettingsFile.Set("Tweaks", "HeightDependentMusic_HeightThreshold", ModOptions.HeightDependentMusic_HeightThreshold);
            SettingsFile.Set("Tweaks", "MoodDependentMusic_MoodThreshold", ModOptions.MoodDependentMusic_MoodThreshold);

            //SettingsFile.Set("Chirper", "EnableChirper", ModOptions.EnableChirper);

            SettingsFile.Set("Playlist", "RandomTrackSelection", ModOptions.RandomTrackSelection);

            //Save Keybindings
            SettingsFile.Set("Keys", "NextTrack", ModOptions.Key_NextTrack);
            SettingsFile.Set("Keys", "ShowSettings", ModOptions.Key_Settings);

            //Update 3: Modpack music folders, additional music folders
            SettingsFile.Set("Music Library", "EnableMusicPacks", ModOptions.EnableMusicPacks);
            SettingsFile.Set("Music Library", "CustomMusicFolders", String.Join(";", ModOptions.AdditionalCustomMusicFolders.ToArray()));

            //Update 3.3
            //SettingsFile.Set("UI", "MusicListShortNames", ModOptions.MusicListShortNames);
            //SettingsFile.Set("UI", "MusicListEnableScrollbar", ModOptions.MusicListEnableScrollbar);

            //Update 4 settings
            SettingsFile.Set("Technical", "PlayWithoutConvert", ModOptions.PlayWithoutConvert);
            SettingsFile.Set("Music Selection", "MusicTagTypePriority", String.Join(";", ModOptions.MusicTagTypePriority.ToArray()));
            SettingsFile.Set("UI", "ShowToolbarButton", ModOptions.ShowToolbarButton);
            SettingsFile.Set("Technical", "MusicSelectionAlgorithm", ModOptions.MusicSelectionAlgorithm.ToString());
            SettingsFile.Set("UI", "LargePlayList", ModOptions.LargePlayList);

            SettingsFile.Set("UI", "ToolbarButtonX", (int)ModOptions.ToolbarButtonX);
            SettingsFile.Set("UI", "ToolbarButtonY", (int)ModOptions.ToolbarButtonY);
            SettingsFile.Set("UI", "FixateToolbarButton", ModOptions.FixateToolbarButton);

            SettingsFile.Set("Technical", "CrossfadeLimit", (int)ModOptions.CrossfadeLimit);
            SettingsFile.Set("Technical", "IgnoreCrossfadeLimit", ModOptions.IgnoreCrossfadeLimit);

            //Update 5.1 settings

            SettingsFile.Set("Performance", "CacheSongs", ModOptions.CacheSongs);
            SettingsFile.Set("Performance", "PrefetchSongs", ModOptions.PrefetchSongs);

			// Update 5.2 Settings
			SettingsFile.Set("Performance", "MusicUpdateTime", ModOptions.MusicUpdateTime);

            SettingsFile.Save();
        }

        public void LoadModSettings()
        {
            Debug.Log("[CSLMusic] Loading settings ...");

            if (!File.Exists(SettingsFile.Filename))
            {
                SaveModSettings();
            }

            SettingsFile.Load();

            var DefaultModOptions = new Options();

            //ModOptions.AutoAddMusicTypesForCustomMusic = SettingsFile.GetAsBool("Music Library", "AutoAddMusicTypesForCustomMusic", DefaultModOptions.AutoAddMusicTypesForCustomMusic);

            ModOptions.MusicWhileLoading = SettingsFile.GetAsBool("Tweaks", "MusicWhileLoading", DefaultModOptions.MusicWhileLoading);
            ModOptions.HeightDependentMusic_HeightThreshold = SettingsFile.GetAsFloat("Tweaks", "HeightDependentMusic_HeightThreshold", DefaultModOptions.HeightDependentMusic_HeightThreshold);
            ModOptions.MoodDependentMusic_MoodThreshold = SettingsFile.GetAsInt("Tweaks", "MoodDependentMusic_MoodThreshold", DefaultModOptions.MoodDependentMusic_MoodThreshold);

            //Check values if they are legal
            if (ModOptions.HeightDependentMusic_HeightThreshold < 0)
                ModOptions.HeightDependentMusic_HeightThreshold = DefaultModOptions.HeightDependentMusic_HeightThreshold;
            if (ModOptions.MoodDependentMusic_MoodThreshold < 0)
                ModOptions.MoodDependentMusic_MoodThreshold = DefaultModOptions.MoodDependentMusic_MoodThreshold;

            //ModOptions.EnableChirper = SettingsFile.GetAsBool("Chirper", "EnableChirper", DefaultModOptions.EnableChirper); //Default to false

            ModOptions.RandomTrackSelection = SettingsFile.GetAsBool("Playlist", "RandomTrackSelection", DefaultModOptions.RandomTrackSelection);

            //Load keybindings
            ModOptions.Key_NextTrack = SettingsFile.GetAsKeyCode("Keys", "NextTrack", DefaultModOptions.Key_NextTrack);
            ModOptions.Key_Settings = SettingsFile.GetAsKeyCode("Keys", "ShowSettings", DefaultModOptions.Key_Settings);

            //Load Update 3 settings
            ModOptions.EnableMusicPacks = SettingsFile.GetAsBool("Music library", "EnableMusicPacks", DefaultModOptions.EnableMusicPacks);

            ModOptions.AdditionalCustomMusicFolders.Clear();
            foreach (String folder in SettingsFile.Get("Music library", "CustomMusicFolders", "").Split(';'))
            {
                if (!String.IsNullOrEmpty(folder) && !ModOptions.CustomMusicFolders.Contains(folder))
                {
                    ModOptions.AdditionalCustomMusicFolders.Add(folder);
                }
            }

            //Update 3.3 settings
            //ModOptions.MusicListShortNames = SettingsFile.GetAsBool("UI", "MusicListShortNames", true);
            //ModOptions.MusicListEnableScrollbar = SettingsFile.GetAsBool("UI", "MusicListEnableScrollbar", true);

            //Update 4 settings
            ModOptions.PlayWithoutConvert = SettingsFile.GetAsBool("Technical", "PlayWithoutConvert", DefaultModOptions.PlayWithoutConvert);
            //ModOptions.MusicTagTypePriority = new List<string>(SettingsFile.Get("Music Selection", "MusicTagTypePriority", String.Join(";", DefaultModOptions.MusicTagTypePriority.ToArray())).Split(';'));
            ModOptions.ShowToolbarButton = SettingsFile.GetAsBool("UI", "ShowToolbarButton", DefaultModOptions.ShowToolbarButton);
            ModOptions.LargePlayList = SettingsFile.GetAsBool("UI", "LargePlayList", DefaultModOptions.LargePlayList);

            ModOptions.ToolbarButtonX = SettingsFile.GetAsInt("UI", "ToolbarButtonX", (int)DefaultModOptions.ToolbarButtonX);
            ModOptions.ToolbarButtonY = SettingsFile.GetAsInt("UI", "ToolbarButtonY", (int)DefaultModOptions.ToolbarButtonY);
            ModOptions.FixateToolbarButton = SettingsFile.GetAsBool("UI", "FixateToolbarButton", DefaultModOptions.FixateToolbarButton);

            ModOptions.CrossfadeLimit = SettingsFile.GetAsInt("Technical", "CrossfadeLimit", DefaultModOptions.CrossfadeLimit);
            ModOptions.IgnoreCrossfadeLimit = SettingsFile.GetAsBool("Technical", "IgnoreCrossfadeLimit", DefaultModOptions.IgnoreCrossfadeLimit);

			//Update 5 (Snowfall) - Catch additional tags. Discard config if there is something wrong with it
			var tag_type_priority = new List<string>(SettingsFile.Get("Music Selection", "MusicTagTypePriority", String.Join(";", DefaultModOptions.MusicTagTypePriority.ToArray())).Split(';'));
            bool tag_type_check_ok = true;

            tag_type_check_ok = (tag_type_priority.Count == DefaultModOptions.MusicTagTypePriority.Count); // Correct count			
            
            foreach (var t in tag_type_priority) //Correct content
            {
                if (!gameObject.GetComponent<MusicManager>().MusicTagTypes.ContainsKey(t))
                {
                    tag_type_check_ok = false;
                    break;
                }
            }

            if (!tag_type_check_ok)
            {
                Debug.Log("[CSLMusicMod] Loading tag priority failed. Resetting.");
                ModOptions.MusicTagTypePriority = DefaultModOptions.MusicTagTypePriority;
            }
            else
            {
                ModOptions.MusicTagTypePriority = tag_type_priority;
            }

            try
            {
                ModOptions.MusicSelectionAlgorithm = (Options.MusicSelectionType)Enum.Parse(typeof(Options.MusicSelectionType), SettingsFile.Get("Technical", "MusicSelectionAlgorithm", DefaultModOptions.MusicSelectionAlgorithm.ToString()));
            }
            catch (Exception)
            {
                ModOptions.MusicSelectionAlgorithm = DefaultModOptions.MusicSelectionAlgorithm;
            }

            // Update 5.1
            ModOptions.CacheSongs = SettingsFile.GetAsBool("Performance", "CacheSongs", DefaultModOptions.CacheSongs);
            ModOptions.PrefetchSongs = SettingsFile.GetAsBool("Performance", "PrefetchSongs", DefaultModOptions.PrefetchSongs);

			// Update 5.2
			ModOptions.MusicUpdateTime = SettingsFile.GetAsInt("Performance", "MusicUpdateTime", DefaultModOptions.MusicUpdateTime);

            //If there are non exisiting keys in the settings file, add them by saving the settings
            if (SettingsFile.FoundNonExistingKeys)
            {
                SaveModSettings();
            }

            //Add music folders from music packs
            AddModpackMusicFolders();
        }

        private void AddModpackMusicFolders()
        {
            ModOptions.ModdedMusicSourceFolders.Clear();

            //If music packs are disabled, just add no folders.
            if (!ModOptions.EnableMusicPacks)
                return;

            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (info.isEnabled)
                {
                    String path = Path.Combine(info.modPath, MusicManager.CustomMusicDefaultFolder);

                    if (Directory.Exists(path))
                    {
                        Debug.Log("[CSLMusic] Adding music pack @ " + path);

                        if (!ModOptions.ModdedMusicSourceFolders.Contains(path))
                        {
                            ModOptions.ModdedMusicSourceFolders.Add(path);
                        }
                    }
                }
            }
        }

        public class Options
        {
            public bool MusicWhileLoading = true;
            public KeyCode Key_NextTrack = KeyCode.N;
            public KeyCode Key_Settings = KeyCode.M;
            public bool EnableMusicPacks = true;
            public int MusicStreamSwitchTime = 154350;
            //65536*2 // 65536*3
            public int MoodDependentMusic_MoodThreshold = 40;
            public float HeightDependentMusic_HeightThreshold = 1400f;
            public bool RandomTrackSelection = true;
            public List<String> AdditionalCustomMusicFolders = new List<string>();
            public List<String> ModdedMusicSourceFolders = new List<String>();
            public List<String> MusicTagTypePriority = new List<string>();

            public bool PlayWithoutConvert = true;
            public bool ShowToolbarButton = true;
            public MusicSelectionType MusicSelectionAlgorithm = MusicSelectionType.AND;
            public bool LargePlayList = false;

            public float ToolbarButtonX = -1;
            public float ToolbarButtonY = -1;
            public bool FixateToolbarButton = true;

            public int CrossfadeLimit = 1024;
            public bool IgnoreCrossfadeLimit = false;

            public bool CacheSongs = true;
            public bool PrefetchSongs = false;

			public int MusicUpdateTime = 1000;

            public List<String> CustomMusicFolders
            {
                get
                {
                    List<string> folders = new List<string>();

                    folders.Add(MusicManager.CustomMusicDefaultFolder);
                    folders.AddRange(AdditionalCustomMusicFolders);

                    return folders;
                }
            }

            public HashSet<string> SupportedNonRawFileFormats
            {
                get
                {
                    if (PlayWithoutConvert)
                        return MusicManager.Supported_Formats_Playback;
                    else
                        return MusicManager.Supported_Formats_Conversion;
                }
            }

            public Options()
            {
                //Add default music tag type priorities
				MusicTagTypePriority.Add("snow");
                MusicTagTypePriority.Add("sky");
                MusicTagTypePriority.Add("night");
                MusicTagTypePriority.Add("bad");
                MusicTagTypePriority.Add("milestone13");
                MusicTagTypePriority.Add("milestone12");
                MusicTagTypePriority.Add("milestone11");
                MusicTagTypePriority.Add("milestone10");
                MusicTagTypePriority.Add("milestone09");
                MusicTagTypePriority.Add("milestone08");
                MusicTagTypePriority.Add("milestone07");
                MusicTagTypePriority.Add("milestone06");
                MusicTagTypePriority.Add("milestone05");
                MusicTagTypePriority.Add("milestone04");
                MusicTagTypePriority.Add("milestone03");
                MusicTagTypePriority.Add("milestone02");
                MusicTagTypePriority.Add("milestone01");
                MusicTagTypePriority.Add("");
            }

            public enum MusicSelectionType
            {
                AND,
                OR
            }
        }
    }
}

