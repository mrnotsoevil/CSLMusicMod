using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ColossalFramework.Plugins;
using ColossalFramework;

namespace CSLMusicMod
{
	public class MusicManager : MonoBehaviour
	{
		public const String CustomMusicDefaultFolder = "CSLMusicMod_Music";
		public const String MusicSettingsFileName = "CSLMusicMod_MusicFiles.csv";

		public SettingsManager.Options ModOptions
		{
			get
			{
				return gameObject.GetComponent<SettingsManager> ().ModOptions;
			}
		}

		public List<CSLCustomMusicEntry> MusicEntries = new List<CSLCustomMusicEntry>();
		public List<CSLCustomMusicEntry> EnabledMusicEntries
		{
			get
			{
				List<CSLCustomMusicEntry> entries = new List<CSLCustomMusicEntry>();

				foreach (CSLCustomMusicEntry entry in MusicEntries)
				{
					if (entry.Enable)
					{
						entries.Add(entry);
					}
				}

				return entries;
			}
		}

		public MusicManager()
		{
		}

		private static bool AddUnknownCustomMusicFiles(ref bool mood_entries_not_found)
		{
			Debug.Log("[CSLMusic] Fetching unknown custom music files ...");

			bool foundsomething = false;

			foreach (String folder in ModOptions.CustomMusicFolders)
			{               
				if (Directory.Exists(folder))
				{ 
					foundsomething |= AddUnknownMusicFiles(folder, ref mood_entries_not_found);
				}
				else
				{
					Debug.LogError("ERROR: " + folder + " is not existing!");
				}
			}

			return foundsomething;
		}

		private static bool AddUnknownMusicPackMusicFiles(ref bool mood_entries_not_found)
		{
			Debug.Log("[CSLMusic] Fetching unknown music pack music files ...");

			bool foundsomething = false;

			List<String> searchfolders = new List<string>();
			searchfolders.AddRange(ModOptions.ModdedMusicSourceFolders);
			if (Directory.Exists(ConversionManager.ConvertedMusicPackMusicFolder))
			{
				searchfolders.AddRange(Directory.GetDirectories(ConversionManager.ConvertedMusicPackMusicFolder));               
			}

			/**
             * Add *.raw music from mod folders if somebody really wants to upload raw files
             * and music from converted
             * */
			foreach (String folder in searchfolders)
			{      
				if (Directory.Exists(folder))
				{ 
					//Does the plugin exist? Is it active
					PluginManager.PluginInfo info = ModHelper.GetSourceModFromId(Path.GetFileName(folder));

					if (info == null)
					{
						Debug.LogWarning("[CSLMusic] Unknown mod in folder @ " + folder);
						continue;
					}

					if (info.isEnabled)
					{
						Debug.Log("[CSLMusic] Adding mod conversion files from " + folder);
						foundsomething |= AddUnknownMusicFiles(folder, ref mood_entries_not_found);
					}
					else
					{
						Debug.Log("[CSLMusic] Not adding mod conversion files from " + folder + ": Not enabled");
						Debug.Log("-> Directory of this mod is " + info.modPath);
					}
				}
				else
				{
					Debug.LogError("ERROR: " + folder + " is not existing!");
				}
			}

			return foundsomething;
		}

		private static bool AddUnknownMusicFiles(String folder, ref bool mood_entries_not_found)
		{
			bool foundsomething = false;

			/**
             * 
             * Note: Must convert before running!
             * 
             * */

			Debug.Log("[CSLMusic] Fetching unknown music files from " + folder + " ...");

			//Get other music
			foreach (String file in Directory.GetFiles(folder))
			{              
				if (Path.GetExtension(file) != ".raw")
					continue;
				if (MusicFileKnown(file))
					continue;
				if (ModOptions.AutoAddMusicTypesForCustomMusic)
				{
					if (file.Contains("#bad") || file.Contains("#sky"))
						continue;
				}

				CSLCustomMusicEntry entry = new CSLCustomMusicEntry(GetCustomMusicBaseName(file), file, "", "");

				Debug.Log("Adding as 'Good' Music file: " + file);              
				MusicEntries.Add(entry);

				foundsomething = true;
			}

			//Find bad/sky music if enabled
			if (ModOptions.AutoAddMusicTypesForCustomMusic)
			{
				//Add remaining music
				foreach (String file in Directory.GetFiles(folder))
				{ 
					if (Path.GetExtension(file) != ".raw")
						continue;
					if (MusicFileKnown(file))
						continue;

					String baseName = GetCustomMusicBaseName(file);
					CSLCustomMusicEntry entry = GetEntryByName(baseName);

					if (entry == null)
					{
						Debug.Log("[CSLMusic] Could not find music entry for " + file + ". Ignoring that file.");
						mood_entries_not_found = true;
						continue;
					}

					if (file.Contains("#bad"))
					{
						entry.BadMusic = file;
						entry.EnableBadMusic = true;
						Debug.Log("Adding as 'Bad' Music file: " + file);
						foundsomething = true;
					}
					else if (file.Contains("#sky"))
					{
						entry.SkyMusic = file;
						entry.EnableSkyMusic = true;
						Debug.Log("Adding as 'Sky' Music file: " + file);
						foundsomething = true;
					}
				}
			}

			Debug.Log("... done");



			return foundsomething;
		}

		private static bool AddUnknownVanillaMusicFiles(ref bool mood_entries_not_found)
		{
			bool foundsomething = false;
			mood_entries_not_found = false;

			String audioFileLocation = ReflectionHelper.GetPrivateField<String>(
				Singleton<AudioManager>.instance, "m_audioLocation");

			Debug.Log("[CSLMusic] Fetching unknown vanilla music files ...");

			//Get good music
			foreach (String file in Directory.GetFiles(audioFileLocation))
			{ 
				if (Path.GetExtension(file) != ".raw")
					continue;
				if (MusicFileKnown(file))
					continue;

				String baseName = Path.GetFileNameWithoutExtension(file);

				if (!baseName.EndsWith("s") && !baseName.EndsWith("b"))
				{
					Debug.Log("'Good' Music file: " + file);

					CSLCustomMusicEntry entry = new CSLCustomMusicEntry(
						GetVanillaMusicBaseName(file), file, "", "");                   
					MusicEntries.Add(entry);

					foundsomething = true;
				}
			}

			//Get other music
			foreach (String file in Directory.GetFiles(audioFileLocation))
			{ 
				if (Path.GetExtension(file) != ".raw")
					continue;
				if (MusicFileKnown(file))
					continue;

				String baseName = Path.GetFileNameWithoutExtension(file);
				String mainName = GetVanillaMusicBaseName(file);

				if (baseName.EndsWith("s") || baseName.EndsWith("b"))
				{
					CSLCustomMusicEntry entry = GetEntryByName(mainName);

					if (entry != null)
					{
						if (baseName.EndsWith("s"))
						{
							Debug.Log("'Sky' Music file: " + file);
							entry.SkyMusic = file;
							entry.EnableSkyMusic = true;

							foundsomething = true;
						}
						else if (baseName.EndsWith("b"))
						{
							Debug.Log("'Bad' Music file: " + file);
							entry.BadMusic = file;
							entry.EnableBadMusic = true;

							foundsomething = true;
						}
					}
					else
					{
						Debug.Log("[CSLMusic] Could not add vanilla music: " + entry + ". Music entry not found.");
						mood_entries_not_found = true;
					}
				}
			}

			Debug.Log("... done");

			return foundsomething;
		}

		private static bool RemoveDeactivatedMusicPackSongs()
		{
			Debug.Log("[CSLMusic] Removing deactivated modded files ...");

			//Let a list of all to-be-removed
			bool changed_sth = false;
			List<String> unwanted = new List<string>();

			foreach (CSLCustomMusicEntry entry in MusicEntries)
			{
				if (MusicFileBelongsToInactiveMod(entry.GoodMusic))
					unwanted.Add(entry.GoodMusic);
				if (MusicFileBelongsToInactiveMod(entry.BadMusic))
					unwanted.Add(entry.BadMusic);
				if (MusicFileBelongsToInactiveMod(entry.SkyMusic))
					unwanted.Add(entry.SkyMusic);
			}

			//Clean them away
			List<CSLCustomMusicEntry> toremove = new List<CSLCustomMusicEntry>();
			foreach (String uw in unwanted)
			{
				Debug.Log("[CSLMusic] ... " + uw);

				foreach (CSLCustomMusicEntry entry in MusicEntries)
				{
					if (entry.GoodMusic == uw)
					{
						toremove.Add(entry);
						continue;
					}

					if (entry.BadMusic == uw)
					{
						entry.BadMusic = "";
						entry.EnableBadMusic = false;
						changed_sth = true;
					}
					if (entry.SkyMusic == uw)
					{
						changed_sth = true;
						entry.SkyMusic = "";
						entry.EnableSkyMusic = false;
					}
				}
			}

			foreach (CSLCustomMusicEntry tr in toremove)
			{
				MusicEntries.Remove(tr);
				changed_sth = true;
			}

			return changed_sth;
		}

		private static bool MusicFileBelongsToInactiveMod(String file)
		{
			if (file.StartsWith(ConversionManager.ConvertedMusicPackMusicFolder))
			{
				String modid = Path.GetFileName(Path.GetDirectoryName(file));

				PluginManager.PluginInfo info = ModHelper.GetSourceModFromId(modid);

				if (info != null)
				{
					return !info.isEnabled;
				}
			}
			else
			{
				PluginManager.PluginInfo info = ModHelper.GetSourceModFromFolder(file);

				if (info != null)
				{
					return !info.isEnabled;
				}
			}

			return false;
		}

		public static void LoadMusicFiles()
		{
			Debug.Log("[CSLMusic] Loading music files from configuration ...");

			//Clear data
			MusicEntries.Clear();

			//Go through the settings file and insert all music from there
			if (File.Exists(MusicSettingsFileName))
			{
				using (StreamReader w = new StreamReader(MusicSettingsFileName))
				{
					String line;

					while ((line = w.ReadLine()) != null)
					{
						//# are comments
						if (line.StartsWith("#"))
						{
							continue;
						}

						String[] cell = line.Trim().Split('\t');

						/**
                         * 
                         * File format:
                         * 
                         * AUTO/MANUAL ENABLED GOOD BAD BAD_ENABLED SKY SKY_ENABLED
                         * 
                         * */

						if (cell.Length == 6)
						{
							bool enabled = cell[0].ToLower() == "true";
							String good = cell[1];
							String bad = cell[2];
							bool bad_enable = cell[3].ToLower() == "true";
							String sky = cell[4];
							bool sky_enable = cell[5].ToLower() == "true";

							if (!File.Exists(good))
							{
								Debug.Log("... Cannot find 'good' music " + good + ". Skipping whole entry!");
								continue;
							}

							if (!File.Exists(bad))
							{
								Debug.Log("... Cannot find 'bad' music " + bad + ". Ignoring this file.");
								bad = "";
								bad_enable = false;
							}

							if (!File.Exists(sky))
							{
								Debug.Log("... Cannot find 'sky' music " + sky + ". Ignoring this file.");
								sky = "";
								sky_enable = false;
							}

							String name = Path.GetFileNameWithoutExtension(good);                           

							//Create the entry
							MusicEntries.Add(new CSLCustomMusicEntry(enabled, name, good, bad, bad_enable, sky, sky_enable));
						}                       
					}
				}
			}

			Debug.Log("[CSLMusic] ... done");

			//Add unknown music files
			bool mood_entries_not_found = false;
			bool changed = AddUnknownVanillaMusicFiles(ref mood_entries_not_found) 
				| AddUnknownCustomMusicFiles(ref mood_entries_not_found) 
				| AddUnknownMusicPackMusicFiles(ref mood_entries_not_found) 
				| RemoveDeactivatedMusicPackSongs();

			//Update 3.3 if something was not assigned - retry now
			if (mood_entries_not_found)
			{
				Debug.Log("[CSLMusic] Reported: Not all items were assigned. Trying to fetch skipped ones, too ...");

				changed |= AddUnknownVanillaMusicFiles(ref mood_entries_not_found)
					| AddUnknownCustomMusicFiles(ref mood_entries_not_found)
					| AddUnknownMusicPackMusicFiles(ref mood_entries_not_found);
			}

			if (changed)
				SaveMusicFileSettings();
		}

		public static void SaveMusicFileSettings()
		{
			using (StreamWriter w = new StreamWriter(MusicSettingsFileName))
			{
				w.WriteLine("# " + CSLMusicMod.VersionName);
				w.WriteLine("# CSL Music Mod Configuration File");
				w.WriteLine("# Uncomment or add custom entries here");
				w.WriteLine("# Enabled (true/false)\t'Good' music\t'Bad' music\tEnable 'Bad' music\t'Sky' music\tEnable 'Sky' music");

				foreach (CSLCustomMusicEntry entry in MusicEntries)
				{
					String data = (String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", 
						entry.Enable,
						entry.GoodMusic,
						entry.BadMusic,
						entry.EnableBadMusic,
						entry.SkyMusic,
						entry.EnableSkyMusic));

					w.WriteLine(data);
				}
			}
		}

		private static void CreateMusicFolder()
		{
			//Create the music folder
			Directory.CreateDirectory(CustomMusicDefaultFolder);

			//Create the readme file into the directory
			using (StreamWriter w = File.CreateText(Path.Combine(CustomMusicDefaultFolder,"README.txt")))
			{
				w.WriteLine("Cities: Skylines Music Mod");
				w.WriteLine("--------------------------");
				w.WriteLine();
				w.WriteLine("Supported Formats:");
				w.WriteLine("*.raw Format");
				w.WriteLine("*.ogg Vorbis");
				w.WriteLine();
				w.WriteLine("--- RAW Format");
				w.WriteLine("Convert your audio file to:");
				w.WriteLine("Signed 16 bit PCM");
				w.WriteLine("Little Endian byte order");
				w.WriteLine("2 Channels (Stereo)");
				w.WriteLine("Frequency 44100Hz");
				w.WriteLine();
				w.WriteLine("--- Ogg Vorbis");
				w.WriteLine("The mod will convert the files into *.raw audio files.");
				w.WriteLine("NVorbis (https://nvorbis.codeplex.com) is used for converting");
				w.WriteLine("the audio files. If NVorbis could not convert your file,");
				w.WriteLine("check for extremly large ID3 tags (e.g. Cover images).");
				w.WriteLine("NVorbis usually fails if ID3 tags are too large.");

			}
		}

		private static bool MusicFileKnown(String filename)
		{
			foreach (CSLCustomMusicEntry entry in MusicEntries)
			{              
				if (entry.Contains(filename))
					return true;
			}

			return false;
		}

		private static String GetCustomMusicBaseName(String filename)
		{
			//Get filename without extension without #bad #sky
			return Path.GetFileNameWithoutExtension(filename).Replace("#bad", "").Replace("#sky", "").Trim();
		}

		private static String GetVanillaMusicBaseName(String filename)
		{
			filename = Path.GetFileNameWithoutExtension(filename);

			if (filename.EndsWith("s"))
				return filename.Substring(0, filename.Length - 1);
			if (filename.EndsWith("b"))
				return filename.Substring(0, filename.Length - 1);

			return filename;
		}

		public static CSLCustomMusicEntry GetEntryByName(String name)
		{
			foreach (CSLCustomMusicEntry entry in MusicEntries)
			{
				if (entry.Name == name)
					return entry;
			}

			return null;
		}
	}
}

