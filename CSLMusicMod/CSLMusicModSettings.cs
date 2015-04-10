using System;
using System.Collections.Generic;
using System.IO;
using ColossalFramework;
using UnityEngine;
using System.Collections;

namespace CSLMusicMod
{
    public static class CSLMusicModSettings
    {
        public static SimpleIni SettingsFile = new SimpleIni("CSLMusicMod_Settings.ini");
        public const String MusicSettingsFileName = "CSLMusicMod_MusicFiles.csv";
        public static bool HeightDependentMusic = true;
        public static bool MoodDependentMusic = true;
        public static bool MusicWhileLoading = true;
        public static bool AutoAddMusicTypesForCustomMusic = true;
        public static KeyCode Key_NextTrack = KeyCode.N;
        public static KeyCode Key_Settings = KeyCode.M;
        public static bool EnableChirper = true;
        public static FastList<CSLCustomMusicEntry> MusicEntries = new FastList<CSLCustomMusicEntry>();

        public static FastList<CSLCustomMusicEntry> EnabledMusicEntries
        {
            get
            {
                FastList<CSLCustomMusicEntry> entries = new FastList<CSLCustomMusicEntry>();

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

        static CSLMusicModSettings()
        {

        }

        public static IEnumerator ConvertCustomMusic()
        {
            Debug.Log("[CSLMusic] Converting custom music files ...");

            //Get other music
            foreach (String file in Directory.GetFiles("CSLMusicMod_Music"))
            { 
                String oggfile = file;
                String rawfile = Path.ChangeExtension(file, ".raw");

                if (Path.GetExtension(oggfile) == ".ogg" && !File.Exists(rawfile))
                {
                    Debug.Log("[CSLMusic] To convert: " + oggfile);
                    CSLMusicMod.ConvertOggToRAW(oggfile, rawfile);

                    yield return null;
                }
            }

            Debug.Log("... done");
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

        private static bool AddUnknownCustomMusicFiles()
        {
            bool foundsomething = false;

            /**
             * 
             * Note: Must convert before running!
             * 
             * */

            Debug.Log("[CSLMusic] Fetching unknown custom music files ...");

            Dictionary<String, CSLCustomMusicEntry> result = new Dictionary<String, CSLCustomMusicEntry>();

            //Get other music
            foreach (String file in Directory.GetFiles("CSLMusicMod_Music"))
            {              
                if (Path.GetExtension(file) != ".raw")
                    continue;
                if (MusicFileKnown(file))
                    continue;
                if (AutoAddMusicTypesForCustomMusic)
                {
                    if (file.Contains("#bad") || file.Contains("#sky"))
                        continue;
                }

                CSLCustomMusicEntry entry = new CSLCustomMusicEntry(file, "", "", false);

                Debug.Log("Adding as 'Good' Music file: " + file);

                if (AutoAddMusicTypesForCustomMusic)
                {
                    //Add as result entry for later search 
                    result[GetCustomMusicBaseName(file)] = entry;
                }
                else
                {
                    MusicEntries.Add(entry);
                }

                foundsomething = true;
            }

            //Find bad/sky music if enabled
            if (AutoAddMusicTypesForCustomMusic)
            {
                //Add remaining music
                foreach (String file in Directory.GetFiles("CSLMusicMod_Music"))
                { 
                    if (Path.GetExtension(file) != ".raw")
                        continue;
                    if (MusicFileKnown(file))
                        continue;

                    String baseName = GetCustomMusicBaseName(file);
                    CSLCustomMusicEntry entry;

                    if (result.ContainsKey(baseName))
                    {
                        entry = result[baseName];
                    }
                    else
                    {
                        Debug.Log("[CSLMusic] Could not find music entry for " + file + ". Ignoring that file.");
                        continue;
                    }

                    if (file.Contains("#bad"))
                    {
                        entry.BadMusic = file;
                        Debug.Log("Adding as 'Bad' Music file: " + file);
                        foundsomething = true;
                    }
                    else if (file.Contains("#sky"))
                    {
                        entry.SkyMusic = file;
                        Debug.Log("Adding as 'Sky' Music file: " + file);
                        foundsomething = true;
                    }
                }

                //Put into list
                foreach (CSLCustomMusicEntry entry in result.Values)
                {
                    MusicEntries.Add(entry);
                }
            }

            Debug.Log("... done");

            return foundsomething;
        }

        private static bool AddUnknownVanillaMusicFiles()
        {
            bool foundsomething = false;

            Dictionary<String, CSLCustomMusicEntry> result = new Dictionary<String, CSLCustomMusicEntry>();

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

                    CSLCustomMusicEntry entry = new CSLCustomMusicEntry(file, "", "", true);                   
                    result.Add(baseName, entry);

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
                String mainName = baseName.Substring(0, baseName.Length - 1);

                if (baseName.EndsWith("s"))
                {
                    Debug.Log("'Sky' Music file: " + file);

                    if (result.ContainsKey(mainName))
                    {   
                        CSLCustomMusicEntry entry = result[mainName];
                        entry.SkyMusic = file;

                        foundsomething = true;
                    }
                }
                else if (baseName.EndsWith("b"))
                {
                    Debug.Log("'Bad' Music file: " + file);

                    if (result.ContainsKey(mainName))
                    {   
                        CSLCustomMusicEntry entry = result[mainName];
                        entry.BadMusic = file;

                        foundsomething = true;
                    }
                }
            }

            //Put into list
            foreach (CSLCustomMusicEntry entry in result.Values)
            {
                MusicEntries.Add(entry);
            }

            Debug.Log("... done");

            return foundsomething;
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
                            continue;

                        String[] cell = line.Trim().Split('\t');

                        /**
                         * 
                         * File format:
                         * 
                         * ENABLED GOOD BAD BAD_ENABLED SKY SKY_ENABLED
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
                                Debug.Log("... Cannot find 'bad' music " + sky + ". Ignoring this file.");
                                sky = "";
                                sky_enable = false;
                            }

                            //Create the entry
                            MusicEntries.Add(new CSLCustomMusicEntry(enabled, good, bad, bad_enable, sky, sky_enable, false));
                        }
                    }
                }
            }

            Debug.Log("[CSLMusic] ... done");

            //Add unknown music files
            bool added = AddUnknownVanillaMusicFiles() | AddUnknownCustomMusicFiles();
       
            if (added)
                SaveMusicFileSettings();
        }

        public static void SaveMusicFileSettings()
        {
            using (StreamWriter w = new StreamWriter(MusicSettingsFileName))
            {
                w.WriteLine("# CSL Music Mod Configuration File");
                w.WriteLine("# Enabled (true/false)\t'Good' music\t'Bad' music\tEnable 'Bad' music\t'Sky' music\tEnable 'Sky' music");
            
                foreach (CSLCustomMusicEntry entry in MusicEntries)
                {
                    w.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", 
                                              entry.Enable,
                                              entry.GoodMusic,
                                              entry.BadMusic,
                                              entry.EnableBadMusic,
                                              entry.SkyMusic,
                                              entry.EnableSkyMusic));
                }
            }
        }

        public static void SaveModSettings()
        {
            Debug.Log("[CSLMusic] Saving settings ...");

            SettingsFile.Set("Music Selection", "HeightDependentMusic", HeightDependentMusic);
            SettingsFile.Set("Music Selection", "MoodDependentMusic", MoodDependentMusic);

            SettingsFile.Set("Music Library", "AutoAddMusicTypesForCustomMusic", AutoAddMusicTypesForCustomMusic);

            SettingsFile.Set("Tweaks", "MusicWhileLoading", MusicWhileLoading);

            SettingsFile.Set("Chirper", "EnableChirper", EnableChirper);

            //Save Keybindings
            SettingsFile.Set("Keys", "NextTrack", Key_NextTrack);
            SettingsFile.Set("Keys", "ShowSettings", Key_Settings);

            SettingsFile.Save();
        }

        public static void LoadModSettings()
        {
            Debug.Log("[CSLMusic] Loading settings ...");

            if (!File.Exists(SettingsFile.Filename))
            {
                SaveModSettings();
            }

            SettingsFile.Load();

            HeightDependentMusic = SettingsFile.GetAsBool("Music Selection", "HeightDependentMusic", true);
            MoodDependentMusic = SettingsFile.GetAsBool("Music Selection", "MoodDependentMusic", true);

            AutoAddMusicTypesForCustomMusic = SettingsFile.GetAsBool("Music Library", "AutoAddMusicTypesForCustomMusic", true);

            MusicWhileLoading = SettingsFile.GetAsBool("Tweaks", "MusicWhileLoading", true);

            EnableChirper = SettingsFile.GetAsBool("Chirper", "EnableChirper", true);

            //Load keybindings
            Key_NextTrack = SettingsFile.GetAsKeyCode("Keys", "NextTrack", KeyCode.N);
            Key_Settings = SettingsFile.GetAsKeyCode("Keys", "ShowSettings", KeyCode.M);
        }

        private static void CreateMusicFolder()
        {
            //Create the music folder
            Directory.CreateDirectory("CSLMusicMod_Music");

            //Create the readme file into the directory
            using (StreamWriter w = File.CreateText("CSLMusicMod_Music/README.txt"))
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
                w.WriteLine("NVorbis usually fails when ID3 tags are too large.");

            }
        }

        public static void CreateFolders()
        {
            CreateMusicFolder();
        }
    }
}

