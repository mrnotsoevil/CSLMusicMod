using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ColossalFramework.Plugins;

namespace CSLMusicMod
{
	public class SettingsManager : MonoBehaviour
	{
		private SimpleIni SettingsFile = new SimpleIni("CSLMusicMod_Settings.ini");
		public Options ModOptions = new Options ();

		public SettingsManager ()
		{
		}

		public void Awake()
		{
			DontDestroyOnLoad(this);
		}

		public void SaveModSettings()
		{
			Debug.Log("[CSLMusic] Saving settings ...");

			SettingsFile.Set("Music Selection", "HeightDependentMusic", ModOptions.HeightDependentMusic);
			SettingsFile.Set("Music Selection", "MoodDependentMusic", ModOptions.MoodDependentMusic);

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

			var DefaultModOptions = new Options ();

			ModOptions.HeightDependentMusic = SettingsFile.GetAsBool("Music Selection", "HeightDependentMusic", DefaultModOptions.HeightDependentMusic);
			ModOptions.MoodDependentMusic = SettingsFile.GetAsBool("Music Selection", "MoodDependentMusic", DefaultModOptions.MoodDependentMusic);

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
            ModOptions.PlayWithoutConvert = SettingsFile.GetAsBool("Technical", "PlayWithoutConvert", true);

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
			public bool HeightDependentMusic = true;
			public bool MoodDependentMusic = true;
			public bool MusicWhileLoading = true;			
			public KeyCode Key_NextTrack = KeyCode.N;
			public KeyCode Key_Settings = KeyCode.M;			
			public bool EnableMusicPacks = true;
			public int MusicStreamSwitchTime = 154350; //65536*2 // 65536*3
			public int MoodDependentMusic_MoodThreshold = 40;
			public float HeightDependentMusic_HeightThreshold = 1400f;
			public bool RandomTrackSelection = true;
			public List<String> AdditionalCustomMusicFolders = new List<string>();
			public List<String> ModdedMusicSourceFolders = new List<String>();

            public bool PlayWithoutConvert = true;

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
		}
	}
}

