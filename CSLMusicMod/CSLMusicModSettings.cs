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
        #region Constants
        public const String VersionName = "Update 3.3";
        public static SimpleIni SettingsFile = new SimpleIni("CSLMusicMod_Settings.ini");
        public const String MusicSettingsFileName = "CSLMusicMod_MusicFiles.csv";
        public const String CustomMusicDefaultFolder = "CSLMusicMod_Music";
        public const String ConvertedMusicPackMusicFolder = "CSLMusicMod_Musicpacks_Converted";
        #endregion
        public static bool HeightDependentMusic = true;
        public static bool MoodDependentMusic = true;
        public static bool MusicWhileLoading = true;
        public static bool AutoAddMusicTypesForCustomMusic = true;
        public static KeyCode Key_NextTrack = KeyCode.N;
        public static KeyCode Key_Settings = KeyCode.M;
        public static bool EnableChirper = false; //Disable by default to prevent infection of savegame
        public static List<CSLCustomMusicEntry> MusicEntries = new List<CSLCustomMusicEntry>();
        #region Update 1 Settings & Variables
        public static int MusicStreamSwitchTime = 154350;
        //65536*2 // 65536*3
        public static int MoodDependentMusic_MoodThreshold = 40;
        public static float HeightDependentMusic_HeightThreshold = 1400f;
        #endregion
        #region Update 2 Settings & Variables
        /**
         * If enabled, switch to a random track
         * */
        public static bool RandomTrackSelection = true;
        /**
         * Contains all *.ogg files which could not be converted
         * */
        public static FastList<String> Info_NonConvertedFiles = new FastList<string>();
        #endregion
        #region Update 3 Settings & Variables
        public static List<String> AdditionalCustomMusicFolders = new List<string>();
        public static List<String> ModdedMusicSourceFolders = new List<String>();

        public static List<String> CustomMusicFolders
        {
            get
            {
                List<string> folders = new List<string>();

                folders.Add(CustomMusicDefaultFolder);
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
        #endregion
        #region Update 3.3 Settings
        /**
         * If music list should short the names
         * */
        public static bool MusicListShortNames = true;

        public static bool MusicListEnableScrollbar = true;
        #endregion
        static CSLMusicModSettings()
        {

        }
        #region Music conversion
        public static IEnumerator ConvertCustomMusic()
        {
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

                if (Path.GetExtension(srcfile) == ".ogg")
                {
                    if (!File.Exists(dstfile))
                    {
                        Debug.Log("[CSLMusic] To convert: " + srcfile + " -> " + dstfile);
                        AudioFormatHelper.ConvertOggToRAW(srcfile, dstfile);

                        yield return null;
                    }
                    else
                    {
                        Debug.Log("[CSLMusic] Not converting " + srcfile + " to " + dstfile);
                    }
                }
            }

            //Set/unset
            RemoveUnsubscribedConvertedModpackMusic();
        }

        private static void ConvertCustomMusic_AddConversionTasks(String folder, Dictionary<String, String> conversiontasks, bool mod)
        {
            Debug.Log("[CSLMusic] Conversion looking for files @ " + folder);

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

                    if (!modification.isEnabled)
                    {
                        //Don't convert if mod is not active
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
        #endregion
        #region Helpers
        /**
         * Creates and returns the folder containing the converted files for given mod
         * */
        public static String CreateModConvertedMusicFolder(PluginManager.PluginInfo info)
        {
            String destinationpath = Path.Combine(ConvertedMusicPackMusicFolder, info.publishedFileID.AsUInt64.ToString());
        
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
            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
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
        #endregion
        #region Adding unknown music
        private static bool AddUnknownCustomMusicFiles(ref bool mood_entries_not_found)
        {
            Debug.Log("[CSLMusic] Fetching unknown custom music files ...");

            bool foundsomething = false;

            foreach (String folder in CustomMusicFolders)
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
            searchfolders.AddRange(ModdedMusicSourceFolders);
            if (Directory.Exists(ConvertedMusicPackMusicFolder))
            {
                searchfolders.AddRange(Directory.GetDirectories(ConvertedMusicPackMusicFolder));               
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
                    PluginManager.PluginInfo info = GetSourceModFromId(Path.GetFileName(folder));

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
                if (AutoAddMusicTypesForCustomMusic)
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
            if (file.StartsWith(ConvertedMusicPackMusicFolder))
            {
                String modid = Path.GetFileName(Path.GetDirectoryName(file));

                PluginManager.PluginInfo info = GetSourceModFromId(modid);

                if (info != null)
                {
                    return !info.isEnabled;
                }
            }
            else
            {
                PluginManager.PluginInfo info = GetSourceModFromFolder(file);

                if (info != null)
                {
                    return !info.isEnabled;
                }
            }

            return false;
        }
        #endregion
        #region Settings Files
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
                w.WriteLine("# " + VersionName);
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
            SettingsFile.Set("Music Library", "CustomMusicFolders", String.Join(";", AdditionalCustomMusicFolders.ToArray()));

            //Update 3.3
            SettingsFile.Set("UI", "MusicListShortNames", MusicListShortNames);
            SettingsFile.Set("UI", "MusicListEnableScrollbar", MusicListEnableScrollbar);

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

            EnableChirper = SettingsFile.GetAsBool("Chirper", "EnableChirper", false); //Default to false

            RandomTrackSelection = SettingsFile.GetAsBool("Playlist", "RandomTrackSelection", true);

            //Load keybindings
            Key_NextTrack = SettingsFile.GetAsKeyCode("Keys", "NextTrack", KeyCode.N);
            Key_Settings = SettingsFile.GetAsKeyCode("Keys", "ShowSettings", KeyCode.M);

            //Load Update 3 settings
            EnableMusicPacks = SettingsFile.GetAsBool("Music library", "EnableMusicPacks", true);

            AdditionalCustomMusicFolders.Clear();
            foreach (String folder in SettingsFile.Get("Music library", "CustomMusicFolders", "").Split(';'))
            {
                if (!String.IsNullOrEmpty(folder) && !CustomMusicFolders.Contains(folder))
                {
                    AdditionalCustomMusicFolders.Add(folder);
                }
            }

            //Update 3.3 settings
            MusicListShortNames = SettingsFile.GetAsBool("UI", "MusicListShortNames", true);
            MusicListEnableScrollbar = SettingsFile.GetAsBool("UI", "MusicListEnableScrollbar", true);

            //If there are non exisiting keys in the settings file, add them by saving the settings
            if (SettingsFile.FoundNonExistingKeys)
            {
                SaveModSettings();
            }

            //Add music folders from music packs
            AddModpackMusicFolders();
        }
        #endregion
        #region Music Packs    
        private static void RemoveUnsubscribedConvertedModpackMusic()
        {
            Debug.Log("[CSLMusic] Removing unsubscribed converted music files ...");

            List<String> dirstoremove = new List<string>();

            if (Directory.Exists(ConvertedMusicPackMusicFolder))
            {
                dirstoremove.AddRange(Directory.GetDirectories(ConvertedMusicPackMusicFolder));
            }

            //Look through folders and look if pluginid exists
            foreach (String folder in dirstoremove.ToArray())
            {
                String foldername = Path.GetFileName(folder);
                String modid = foldername.TrimStart('_');

                if (PluginIdExists(modid))
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
            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (info.publishedFileID.AsUInt64.ToString() == id)
                    return true;
            }

            return false;
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
                    String path = Path.Combine(info.modPath, CustomMusicDefaultFolder);

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

        /**
         * 
         * Gets the mod by the specified id string
         * Only returns mod with CSLMusicMod_Music folder
         * 
         * */
        public static PluginManager.PluginInfo GetSourceModFromId(String id)
        {
            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (info.publishedFileID.AsUInt64.ToString() == id)
                {
                    if (Directory.Exists(Path.Combine(info.modPath, CustomMusicDefaultFolder)))
                        return info;
                }
            }

            return null;
        }
        #endregion
        #region Folders
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

        public static void CreateFolders()
        {
            CreateMusicFolder();
        }
        #endregion
    }
}

