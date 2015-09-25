using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ColossalFramework.Plugins;
using ColossalFramework;
using CSLMusicMod.MusicEntryTags;
using CSLMusicMod.Helpers;

namespace CSLMusicMod
{
    public class MusicManager : MonoBehaviour
    {
        public const String CustomMusicDefaultFolder = "CSLMusicMod_Music";
        public const String MusicSettingsFileName = "CSLMusicMod_MusicFiles.csv";

        public static readonly HashSet<String> Supported_Formats_Conversion = new HashSet<string>(new string[]{ ".ogg" });
        public static readonly HashSet<String> Supported_Formats_Playback = new HashSet<string>(new string[] { ".ogg", ".wav", ".aiff", ".aif", ".mod", ".it", ".s3m", ".xm" });

        public SettingsManager.Options ModOptions
        {
            get
            {
                return gameObject.GetComponent<SettingsManager>().ModOptions; 
            }
        }

        public List<MusicEntry> MusicEntries = new List<MusicEntry>();

        public List<MusicEntry> EnabledMusicEntries
        {
            get
            {
                List<MusicEntry> entries = new List<MusicEntry>();

                foreach (MusicEntry entry in MusicEntries)
                {
                    if (entry.Enable)
                    {
                        entries.Add(entry);
                    }
                }

                return entries;
            }
        }

        public Dictionary<String, MusicEntryTag> MusicTagTypes = new Dictionary<String, MusicEntryTag>();

        public MusicManager()
        {
            InitializeTags();
        }

        /**
         * Add the tag types
         * */
        private void InitializeTags()
        {
            AddTagType(new TagVanillaSky());
            AddTagType(new TagVanillaMood());
            AddTagType(new TagVanillaNight());
            AddTagType(new TagDefault());
            AddTagType(new TagFalse());
        }

        /**
         * Adds a music entry tag type
         * */
        public void AddTagType(MusicEntryTag tag)
        {
            Debug.Log("[CSLMusicMod] Adding tag type #" + tag.Name);
            MusicTagTypes.Add(tag.Name, tag);
        }

        private bool AddUnknownCustomMusicFiles(ref bool mood_entries_not_found)
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

        private bool AddUnknownMusicPackMusicFiles(ref bool mood_entries_not_found)
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
                    var modid = Directory.GetParent(folder).Name;
                    Debug.Log("[CSLMusic] Looking for MusicPack songs in " + folder + ", mod-id " + modid);

                    //Does the plugin exist? Is it active
                    PluginManager.PluginInfo info = ModHelper.GetSourceModFromId(modid);

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

        private bool AddUnknownMusicFiles(String folder, ref bool mood_entries_not_found)
        {
            bool foundsomething = false;

            /**
             * 
             * Note: Must convert before running!
             * 
             * update 4: added option to directly play ogg vorbis
             * 
             * */

            Debug.Log("[CSLMusic] Fetching unknown music files from " + folder + " ...");

            foreach (String file in Directory.GetFiles(folder))
            {
                if (!ModOptions.PlayWithoutConvert && Path.GetExtension(file) != ".raw")
                    continue;
                if (Path.GetExtension(file) != ".raw" && !ModOptions.SupportedNonRawFileFormats.Contains(Path.GetExtension(file)))
                    continue;
                if (MusicFileKnown(file))
                    continue;

                String baseName = GetCustomMusicBaseName(file);

                // If necessary create the entry
                MusicEntry entry = GetEntryByName(baseName);

                if (entry == null)
                {
                    entry = new MusicEntry(true, gameObject, baseName);
                    MusicEntries.Add(entry);

                    foundsomething = true;
                }

                entry.AddSong(file);
            }

            Debug.Log("... done");

            return foundsomething;
        }

        private bool AddUnknownVanillaMusicFiles(ref bool mood_entries_not_found)
        {
            bool foundsomething = false;
            mood_entries_not_found = false;

            String audioFileLocation = ReflectionHelper.GetPrivateField<String>(
                                           Singleton<AudioManager>.instance, "m_audioLocation");

            Debug.Log("[CSLMusic] Fetching unknown vanilla music files ...");

            //Get good music
            foreach (String file in Directory.GetFiles(audioFileLocation))
            { 
                if (Path.GetExtension(file) != ".raw") //Vanilla music is always *.raw
                    continue;
                if (MusicFileKnown(file))
                    continue;

                String baseName = GetVanillaMusicBaseName(file);

                Debug.Log("[CSLMusicMod] Vanilla music base name: \"" + baseName + "\" <- " + file);

                //After Dark Music
                bool after_dark = false;
                if (baseName.StartsWith("After Dark") && baseName != "After Dark Menu")
                {
                    after_dark = true;
                    baseName = baseName.Replace("After Dark", "Colossal Style");

                    Debug.Log("[CSLMusicMod] AfterDark override: " + baseName + " to Colossal Style#night");
                    baseName = baseName.Replace("After Dark", "Colossal Style");
                }

                // Translate names to OST original names
                switch (baseName)
                {
                    case "Colossal Menu":
                        baseName = "Cities: Skylines - Main Theme";
                        break;
                    case "After Dark Menu":
                        baseName = "Cities: Skylines - Main Theme (After Dark)";
                        break;
                    case "Colossal Style 1":
                        baseName = "Cities: Skylines - Stern Berger";
                        break;
                    case "Colossal Style 2":
                        baseName = "Cities: Skylines - Burned Bean Coffee";
                        break;
                    case "Colossal Style 3":
                        baseName = "Cities: Skylines - AUKIO";
                        break;
                    case "Colossal Style 4":
                        baseName = "Cities: Skylines - Itsy Bitsy Critter";
                        break;
                    case "Colossal Style 5":
                        baseName = "Cities: Skylines - Dino Oil";
                        break;
                }

                MusicEntry entry = GetEntryByName(baseName);

                if (entry == null)
                {
                    if (baseName == "Cities: Skylines - Main Theme" || baseName == "Cities: Skylines - Main Theme (After Dark)")
                        entry = new MusicEntry(false, gameObject, baseName);
                    else
                        entry = new MusicEntry(true, gameObject, baseName);

                    MusicEntries.Add(entry);
                }

                //Add the vanilla music according to the vanilla annotation
                String file_noext = Path.GetFileNameWithoutExtension(file);

                if (after_dark)
                {
                    //This is afterdark music: add it as #night
                    if (file_noext.EndsWith("b"))
                        entry.AddSong(file, "bad", "night");
                    else if (file_noext.EndsWith("s"))
                        entry.AddSong(file, "sky", "night");
                    else
                        entry.AddSong(file, "night");
                }
                else
                {
                    if (file_noext.EndsWith("b"))
                        entry.AddSong(file, "bad");
                    else if (file_noext.EndsWith("s"))
                        entry.AddSong(file, "sky");
                    else
                        entry.AddSong(file, "");
                }

                foundsomething = true;
            }

            Debug.Log("... done");

            return foundsomething;
        }

        private bool RemoveDeactivatedMusicPackSongs()
        {
            Debug.Log("[CSLMusic] Removing deactivated modded files ...");

            //Let a list of all to-be-removed
            bool changed_sth = false;


            foreach (MusicEntry entry in MusicEntries)
            {
                List<String> to_remove = new List<string>();

                foreach (String song in entry.SongTags.Keys)
                {
                    if (MusicFileBelongsToInactiveMod(song))
                    {
                        to_remove.Add(song);
                    }
                }

                //Remove the unwanted songs
                foreach (String song in to_remove)
                {
                    entry.RemoveSong(song);
                    changed_sth = true;
                }

            }

            //Remove all empty entries
            changed_sth |= MusicEntries.RemoveAll((e) => e.Empty) != 0;           

            return changed_sth;
        }

        private bool MusicFileBelongsToInactiveMod(String file)
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

        public void LoadMusicFiles()
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
                         * BASENAME ENABLED
                         * 
                         * */

                        if (cell.Length == 2)
                        {
                            String baseName = cell[0];
                            bool enabled = (cell[1].ToLower()) == "true";

                            if (GetEntryByName(baseName) == null)
                                MusicEntries.Add(new MusicEntry(enabled, gameObject, baseName));
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

            //Write report
            WriteReport();
        }

        public void WriteReport()
        {
            using (StreamWriter w = new StreamWriter("CSLMusicMod_MusicEntryReport.log"))
            {
                w.WriteLine("CSL Music Mod version " + CSLMusicMod.VersionName);

                foreach (var entry in MusicEntries)
                {
                    w.WriteLine(entry.BaseName + ", enabled: " + entry.Enable);
                    w.WriteLine();

                    foreach (var tagsong in entry.TagSongs)
                    {
                        w.WriteLine("#" + tagsong.Key + "->");

                        foreach (var song in tagsong.Value)
                        {
                            w.WriteLine("\t" + song);
                        }
                    }

                    w.WriteLine();
                    w.WriteLine();
                    w.WriteLine();
                }
            }
        }

        public void SaveMusicFileSettings()
        {
            using (StreamWriter w = new StreamWriter(MusicSettingsFileName))
            {
                w.WriteLine("# " + CSLMusicMod.VersionName);
                w.WriteLine("# CSL Music Mod Configuration File");
                w.WriteLine("# Uncomment or add custom entries here");
                w.WriteLine("# Base name\tEnabled (true/false)");

                foreach (MusicEntry entry in MusicEntries)
                {
                    String data = (String.Format("{0}\t{1}", 
                                      entry.BaseName,
                                      entry.Enable));

                    w.WriteLine(data);
                }
            }
        }

        public void CreateMusicFolder()
        {
            //Create the music folder
            Directory.CreateDirectory(CustomMusicDefaultFolder);

            //Create the readme file into the directory
            using (StreamWriter w = File.CreateText(Path.Combine(CustomMusicDefaultFolder, "README.txt")))
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

        private bool MusicFileKnown(String filename)
        {
            foreach (MusicEntry entry in MusicEntries)
            {              
                if (entry.Contains(filename))
                    return true;
            }

            return false;
        }

        /**
         * The filename is structured <Basename><Tag1><Tag2> ...
         * 
         * return <Basename>
         * */
        private String GetCustomMusicBaseName(String filename)
        {			
            return Path.GetFileNameWithoutExtension(filename).Split('#')[0].Trim();
        }

        /**
         * Returns the basename for vanilla C:S music files
         * */
        private String GetVanillaMusicBaseName(String filename)
        {
            filename = Path.GetFileNameWithoutExtension(filename);

            if (filename.EndsWith("s"))
                return filename.Substring(0, filename.Length - 1).Trim();
            if (filename.EndsWith("b"))
                return filename.Substring(0, filename.Length - 1).Trim();

            return filename.Trim();
        }

        public MusicEntry GetEntryByName(String name)
        {
            foreach (MusicEntry entry in MusicEntries)
            {
                if (entry.BaseName == name)
                    return entry;
            }

            return null;
        }
    }
}

