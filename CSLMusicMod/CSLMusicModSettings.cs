using System;
using System.Collections.Generic;
using System.IO;
using ColossalFramework;
using UnityEngine;
using System.Collections;
using ColossalFramework.Plugins;

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
        public static List<CSLCustomMusicEntry> MusicEntries = new List<CSLCustomMusicEntry>();
        //Additional settings
        public static int MusicStreamSwitchTime = 154350;
        //65536*2 // 65536*3
        public static int MoodDependentMusic_MoodThreshold = 40;
        public static float HeightDependentMusic_HeightThreshold = 1400f;
        //Update 2 Settings
        /**
         * If enabled, switch to a random track
         * */
        public static bool RandomTrackSelection = true;
        /**
         * Contains all *.ogg files which could not be converted
         * */
        public static FastList<String> Info_NonConvertedFiles = new FastList<string>();
        /**
         * Update 3 settings
         * */
        public static List<String> AdditionalCustomMusicFolders = new List<string>();
        public static List<String> ModdedMusicSourceFolders = new List<String>();

        public static List<String> CustomMusicFolders
        {
            get
            {
                List<string> folders = new List<string>();

                folders.Add("CSLMusicMod_Music");
                folders.AddRange(AdditionalCustomMusicFolders);

                return folders;
            }
        }

        public static bool EnableMusicPacks = true;

        public static List<CSLCustomMusicEntry> EnabledMusicEntries
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

        static CSLMusicModSettings()
        {

        }

        public static IEnumerator ConvertCustomMusic()
        {
            //Do it here
            RemoveUnsubscribedConvertedModpackMusic();

            Debug.Log("[CSLMusic] Converting custom and music pack music ...");
            Info_NonConvertedFiles.Clear();

            //Collect all to convert
            Dictionary<String, String> conversiontasks = new Dictionary<string, string>();

            foreach (String folder in CustomMusicFolders)
            {
                ConvertCustomMusic_AddConversionTasks(folder, conversiontasks, false);
            }
            foreach (String folder in ModdedMusicSourceFolders)
            {
                ConvertCustomMusic_AddConversionTasks(folder, conversiontasks, true);
            }

            //Convert
            foreach (KeyValuePair<String,String> task in conversiontasks)
            {
                String srcfile = task.Key;
                String dstfile = task.Value;               

                if (Path.GetExtension(srcfile) == ".ogg" && !File.Exists(dstfile))
                {
                    Debug.Log("[CSLMusic] To convert: " + srcfile);
                    AudioFormatHelper.ConvertOggToRAW(srcfile, dstfile);

                    yield return null;
                }
            }
        }

        private static void ConvertCustomMusic_AddConversionTasks(String folder, Dictionary<String, String> conversiontasks, bool mod)
        {
            if (Directory.Exists(folder))
            { 
                PluginManager.PluginInfo modification = null;

                if (mod)
                {
                    modification = GetSourceModFromFolder(folder);

                    if (modification == null)
                    {
                        Debug.LogError("[CSLMusic] Cannot add folder " + folder + " as mod! Mod could not be identified");
                        return;
                    }
                }

                //Get music in pack folder and look if the file has a corresponding *.raw file
                //If not, convert the file to raw
                foreach (String file in Directory.GetFiles(folder))
                { 
                    if (Path.GetExtension(file) == ".ogg")
                    {
                        String srcfile = file;
                        String dstfile;

                        if (mod)
                        {
                            //We need to change the folder!
                            dstfile = Path.Combine(CreateModConvertedMusicFolder(modification), Path.GetFileNameWithoutExtension(srcfile) + ".raw");
                        }
                        else
                        {
                            dstfile = Path.ChangeExtension(file, ".raw"); //We can work in out own folder
                        }

                        if (!File.Exists(dstfile))
                        {
                            //Add task
                            conversiontasks[srcfile] = dstfile;
                        }
                    }
                }        
            }
            else
            {
                Debug.LogError("ERROR: " + folder + " is not existing!");
            }
        }

        /**
         * Creates and returns the folder containing the converted files for given mod
         * */
        public static String CreateModConvertedMusicFolder(PluginManager.PluginInfo info)
        {
            String destinationpath = Path.Combine("CSLMusicMod_Musicpacks_Converted", info.publishedFileID.AsUInt64.ToString());
        
            if (!Directory.Exists(destinationpath))
                Directory.CreateDirectory(destinationpath);

            return destinationpath;
        }

        /**
         * 
         * Gets the mod by the specified folder
         * 
         * */
        public static PluginManager.PluginInfo GetSourceModFromFolder(String folder)
        {
            foreach(PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (folder.StartsWith(info.modPath))
                    return info;
            }

            return null;
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

        private static bool AddUnknownCustomMusicFiles()
        {
            Debug.Log("[CSLMusic] Fetching unknown custom music files ...");

            bool foundsomething = false;

            foreach (String folder in CustomMusicFolders)
            {               
                if (Directory.Exists(folder))
                { 
                    foundsomething |= AddUnknownMusicFiles(folder, CSLCustomMusicEntry.SourceType.Custom);
                }
                else
                {
                    Debug.LogError("ERROR: " + folder + " is not existing!");
                }
            }

            return foundsomething;
        }

        private static bool AddUnknownMusicPackMusicFiles()
        {
            Debug.Log("[CSLMusic] Fetching unknown music pack music files ...");

            bool foundsomething = false;

            List<String> searchfolders = new List<string>();
            searchfolders.AddRange(ModdedMusicSourceFolders);
            if (Directory.Exists("CSLMusicMod_Musicpacks_Converted"))
            {
                searchfolders.AddRange(Directory.GetDirectories("CSLMusicMod_Musicpacks_Converted"));
            }

            /**
             * Add *.raw music from mod folders if somebody really wants to upload raw files
             * and music from converted
             * */
            foreach (String folder in searchfolders)
            {               
                if (Directory.Exists(folder))
                { 
                    foundsomething |= AddUnknownMusicFiles(folder, CSLCustomMusicEntry.SourceType.Mod);
                }
                else
                {
                    Debug.LogError("ERROR: " + folder + " is not existing!");
                }
            }

            return foundsomething;
        }

        private static bool AddUnknownMusicFiles(String folder, CSLCustomMusicEntry.SourceType type)
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
                if (AutoAddMusicTypesForCustomMusic)
                {
                    if (file.Contains("#bad") || file.Contains("#sky"))
                        continue;
                }

                CSLCustomMusicEntry entry = new CSLCustomMusicEntry(type, GetCustomMusicBaseName(file), file, "", "");

                Debug.Log("Adding as 'Good' Music file: " + file);              
                MusicEntries.Add(entry);

                foundsomething = true;
            }

            //Find bad/sky music if enabled
            if (AutoAddMusicTypesForCustomMusic)
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
            }

            Debug.Log("... done");

            return foundsomething;
        }

        private static bool AddUnknownVanillaMusicFiles()
        {
            bool foundsomething = false;

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
                        CSLCustomMusicEntry.SourceType.Vanilla, GetVanillaMusicBaseName(file), file, "", "");                   
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

                            foundsomething = true;
                        }
                        else if (baseName.EndsWith("b"))
                        {
                            Debug.Log("'Bad' Music file: " + file);
                            entry.BadMusic = file;

                            foundsomething = true;
                        }
                    }
                    else
                    {
                        Debug.Log("[CSLMusic] Could not add vanilla music: " + entry + ". Music entry not found.");
                    }
                }
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
                    bool found_current_version = false; //look for "# Update 3"

                    while ((line = w.ReadLine()) != null)
                    {
                        //# are comments
                        if (line.StartsWith("#"))
                        {
                            if (line.StartsWith("# Update 3"))
                                found_current_version = true;

                            continue;
                        }

                        //If file is not current version, ignore all entries
                        if (!found_current_version)
                        {
                            Debug.Log("[CSLMusic] Sorry, because music list now works different, ignoring the definitions in this file :(");
                            break;
                        }

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
                                Debug.Log("... Cannot find 'sky' music " + sky + ". Ignoring this file.");
                                sky = "";
                                sky_enable = false;
                            }

                            String name = Path.GetFileNameWithoutExtension(good);

                            //Create the entry
                            MusicEntries.Add(new CSLCustomMusicEntry(CSLCustomMusicEntry.SourceType.Manual, enabled, name, good, bad, bad_enable, sky, sky_enable));
                        }
                    }
                }
            }

            Debug.Log("[CSLMusic] ... done");

            //Add unknown music files
            bool changed = AddUnknownVanillaMusicFiles() | AddUnknownCustomMusicFiles();
       
            if (changed)
                SaveMusicFileSettings();
        }

        public static void SaveMusicFileSettings()
        {
            using (StreamWriter w = new StreamWriter(MusicSettingsFileName))
            {
                w.WriteLine("# Update 3");
                w.WriteLine("# CSL Music Mod Configuration File");
                w.WriteLine("# Uncomment or add custom entries here");
                w.WriteLine("# Enabled (true/false)\t'Good' music\t'Bad' music\tEnable 'Bad' music\t'Sky' music\tEnable 'Sky' music");
            
                foreach (CSLCustomMusicEntry entry in MusicEntries)
                {
                    String data = (String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", 
                                                 entry.Enable,
                                                 entry.GoodMusic,
                                                 entry.BadMusic,
                                                 entry.EnableBadMusic,
                                                 entry.SkyMusic,
                                                 entry.EnableSkyMusic));

                    if (entry.Source == CSLCustomMusicEntry.SourceType.Manual)
                    {
                        w.WriteLine(data);
                    }
                    else
                    {
                        w.WriteLine("# " + data); //Write our data commented so user can modify easily
                    }
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
            SettingsFile.Set("Tweaks", "HeightDependentMusic_HeightThreshold", HeightDependentMusic_HeightThreshold);
            SettingsFile.Set("Tweaks", "MoodDependentMusic_MoodThreshold", MoodDependentMusic_MoodThreshold);

            SettingsFile.Set("Chirper", "EnableChirper", EnableChirper);

            SettingsFile.Set("Playlist", "RandomTrackSelection", RandomTrackSelection);

            //Save Keybindings
            SettingsFile.Set("Keys", "NextTrack", Key_NextTrack);
            SettingsFile.Set("Keys", "ShowSettings", Key_Settings);

            //Update 3: Modpack music folders, additional music folders
            SettingsFile.Set("Music Library", "EnableMusicPacks", EnableMusicPacks);
            SettingsFile.Set("Music Library", "CustomMusicFolders", String.Join(";", CustomMusicFolders.ToArray()));

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
            HeightDependentMusic_HeightThreshold = SettingsFile.GetAsFloat("Tweaks", "HeightDependentMusic_HeightThreshold", 1400f);
            MoodDependentMusic_MoodThreshold = SettingsFile.GetAsInt("Tweaks", "MoodDependentMusic_MoodThreshold", 40);

            //Check values if they are legal
            if (HeightDependentMusic_HeightThreshold < 0)
                HeightDependentMusic_HeightThreshold = 0;
            if (MoodDependentMusic_MoodThreshold < 0)
                MoodDependentMusic_MoodThreshold = 0;

            EnableChirper = SettingsFile.GetAsBool("Chirper", "EnableChirper", true);

            RandomTrackSelection = SettingsFile.GetAsBool("Playlist", "RandomTrackSelection", true);

            //Load keybindings
            Key_NextTrack = SettingsFile.GetAsKeyCode("Keys", "NextTrack", KeyCode.N);
            Key_Settings = SettingsFile.GetAsKeyCode("Keys", "ShowSettings", KeyCode.M);

            //Load Update 3 settings
            EnableMusicPacks = SettingsFile.GetAsBool("Music library", "EnableMusicPacks", true);

            CustomMusicFolders.Clear();
            foreach (String folder in SettingsFile.Get("Music library", "CustomMusicFolders", "").Split(';'))
            {
                if (!String.IsNullOrEmpty(folder) && !CustomMusicFolders.Contains(folder))
                {
                    CustomMusicFolders.Add(folder);
                }
            }

            //If there are non exisiting keys in the settings file, add them by saving the settings
            if (SettingsFile.FoundNonExistingKeys)
            {
                SaveModSettings();
            }

            //Add music folders from music packs
            AddModpackMusicFolders();
        }

        private static void AddModpackMusicFolders()
        {
            ModdedMusicSourceFolders.Clear();

            //If music packs are disabled, just add no folders.
            if (!EnableMusicPacks)
                return;

            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (info.isEnabled)
                {
                    String path = Path.Combine(info.modPath, "CSLMusicMod_Music");

                    if (Directory.Exists(path))
                    {
                        Debug.Log("[CSLMusic] Adding music pack @ " + path);

                        if (!ModdedMusicSourceFolders.Contains(path))
                        {
                            ModdedMusicSourceFolders.Add(path);
                        }
                    }
                }
            }
        }

        private static void RemoveUnsubscribedConvertedModpackMusic()
        {
            Debug.Log("[CSLMusic] Removing unsubscribed converted music files ...");

            List<String> dirstoremove = new List<string>();

            if(Directory.Exists("CSLMusicMod_Musicpacks_Converted"))
            {
                dirstoremove.AddRange(Directory.GetDirectories("CSLMusicMod_Musicpacks_Converted"));
            }

            //Look through folders and look if pluginid exists
            foreach(String folder in dirstoremove.ToArray())
            {
                if (PluginIdExists(Path.GetFileName(folder)))
                {
                    dirstoremove.Remove(folder);
                }
            }

            //Delete all which are left
            foreach (String folder in dirstoremove)
            {
                Debug.Log("[CSLMusic] ... deleting " + folder);
                Directory.Delete(folder, true);
            }
        }

        private static bool PluginIdExists(String id)
        {
            foreach(PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (info.publishedFileID.AsUInt64.ToString() == id)
                    return true;
            }

            return false;
        }

        /**
         * EDIT: Obsolete through new system
         * 
         * Because modpack music is stored in the same file as "normal" music,
         * it must be cleaned away if the user deactivates the mod
         * 
         * There are no real nice alternatives to this
         * */
        /*private static bool RemoveInactiveModpackMusic()
        {
            Debug.Log("[CSLMusic] Removing inactive mod music files from entry list ...");

            bool changes_made = false;

            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (!info.isEnabled)
                {
                    String path = Path.Combine(info.modPath, "CSLMusicMod_Music");

                    if (Directory.Exists(path))
                    {
                        Debug.Log("... mod: " + info.name);

                        foreach (String file in Directory.GetFiles(path))
                        {
                            if (Path.GetExtension(file) == ".raw")
                            {
                                changes_made |= CleanMusicListFrom(file);
                            }
                        }
                    }
                }
            }

            return changes_made;
        }*/

        /**
         * Removes file from currently loaded music entries
         * Must save manually!
         * */
        /*private static bool CleanMusicListFrom(String file)
        {
            Debug.Log("[CSLMusic] Removing music file: " + file);

            List<CSLCustomMusicEntry> todelete = new List<CSLCustomMusicEntry>();

            bool changes_made = false;

            foreach (CSLCustomMusicEntry entry in MusicEntries)
            {
                if (entry.GoodMusic == file)
                {
                    todelete.Add(entry);
                    continue;
                }
                if (entry.BadMusic == file)
                {
                    entry.BadMusic = "";
                    entry.EnableBadMusic = false;
                    changes_made = true;
                }
                if (entry.SkyMusic == file)
                {
                    entry.SkyMusic = "";
                    entry.EnableSkyMusic = false;
                    changes_made = true;
                }
            }

            foreach (CSLCustomMusicEntry entry in todelete)
            {
                MusicEntries.Remove(entry);
            }

            return changes_made;
        }*/

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
                w.WriteLine("NVorbis usually fails if ID3 tags are too large.");

            }
        }

        public static void CreateFolders()
        {
            CreateMusicFolder();
        }
    }
}

