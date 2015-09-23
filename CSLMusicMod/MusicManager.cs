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
            AddTagType(new TagNight());
            AddTagType(new TagDefault());
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
                if (Path.GetExtension(file) != ".raw" && Path.GetExtension(file) != ".ogg")
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

                MusicEntry entry = GetEntryByName(baseName);

                if (entry == null)
                {
                    entry = new MusicEntry(true, gameObject, GetVanillaMusicBaseName(file));
                    MusicEntries.Add(entry);
                }

                //Add the vanilla music according to the vanilla annotation
                if (file.EndsWith("b"))
                    entry.AddSong(file, "bad");
                else if (file.EndsWith("s"))
                    entry.AddSong(file, "sky");
                else
                    entry.AddSong(file, "");

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
                            bool enabled = (cell[0].ToLower()) == "true";

                            if(GetEntryByName(baseName) != null)
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
                return filename.Substring(0, filename.Length - 1);
            if (filename.EndsWith("b"))
                return filename.Substring(0, filename.Length - 1);

            return filename;
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

