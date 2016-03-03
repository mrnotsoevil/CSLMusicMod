using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace CSLMusicMod
{
    /**
     * Contains the music, associates the songs to their tags
     * */
    public class MusicEntry
    {
        public GameObject GameObject { get; private set; }

        public String BaseName { get; private set; }

        /**
         * Dictionary of <Tag> to <List of songs>
         * */
        public Dictionary<String, List<String>> TagSongs { get; private set; }

        /**
         * Dictionary of <Song> to <List of tags>
         * */
        public Dictionary<String, List<String>> SongTags { get; private set; }

        private bool _enable = true;

        public bool Enable
        {
            get
            {
                return _enable;
            }
            set
            {
                _enable = value;
                GameObject.GetComponent<MusicManager>().SaveMusicFileSettings();
            }
        }

        public bool Empty
        {
            get
            {
                return SongTags.Count == 0;
            }
        }

        private Dictionary<String, bool> __applicableTags = new Dictionary<string, bool>();
        private Dictionary<String, int> __tagScore = new Dictionary<string, int>();

        public MusicEntry(bool enable, GameObject gameObject, String baseName)
        {
            _enable = enable;
            GameObject = gameObject;
            BaseName = baseName;
            TagSongs = new Dictionary<string, List<string>>();
            SongTags = new Dictionary<string, List<string>>();
        }

        /**
         * Adds a song.
         * */
        public void AddSong(String filename)
        {
            // Split tags from filename
            string[] cell = Path.GetFileNameWithoutExtension(filename).Split(MusicManager.TagIndicator);
            string[] tags = new string[cell.Length - 1];           

            for (int i = 1; i < cell.Length; i++)
            {
                tags[i - 1] = cell[i].Trim();
            }

            AddSong(filename, tags);
        }

        /**
         * Adds a song with manually tags
         * */
        public void AddSong(String filename, params String[] tags)
        {          
            //Add into dictionaries
            List<String> taglist = new List<string>(tags);

            SongTags.Add(filename, taglist);

            // Always add implicit default tag

            /*{
                if (!TagSongs.ContainsKey(""))
                    TagSongs.Add("", new List<string>());

                TagSongs[""].Add(filename);
            }

            foreach (String tag in tags)
            {
                if (!TagSongs.ContainsKey(tag))
                    TagSongs.Add(tag, new List<string>());

                TagSongs[tag].Add(filename);
            }*/

            // Switch for "default tag"
            if (taglist.Count == 0)
            {
                if (!TagSongs.ContainsKey(""))
                    TagSongs.Add("", new List<string>());

                TagSongs[""].Add(filename);
            }
            else
            {
                foreach (String tag in tags)
                {
                    if (!TagSongs.ContainsKey(tag))
                        TagSongs.Add(tag, new List<string>());

                    TagSongs[tag].Add(filename);
                }
            }

            //Sort all songs
            foreach (var songs in TagSongs.Values)
            {
                songs.Sort((s1, s2) => SongTags[s1].Count.CompareTo(SongTags[s2].Count));
            }
        }

        public void RemoveSong(String filename)
        {
            SongTags.Remove(filename);

            foreach (var list in TagSongs.Values)
            {
                list.Remove(filename);
            }

            //Sort all songs
            foreach (var songs in TagSongs.Values)
            {
                songs.Sort((s1, s2) => SongTags[s1].Count.CompareTo(SongTags[s2].Count));
            }
        }

      

        /**
         * Gets the best matching music according to selection algorithm
         * */
        public String GetMatchingMusic(AudioManager.ListenerInfo info)
        {
            String song = null;
            bool sel_and = GameObject.GetComponent<SettingsManager>().ModOptions.MusicSelectionAlgorithm == SettingsManager.Options.MusicSelectionType.AND;

            {
                var all_tags = GameObject.GetComponent<MusicManager>().MusicTagTypes.Values;

                //Determine which tags are now applicable
                foreach (var tag in all_tags)
                {
                    if (tag.TagApplies(GameObject, info))
                    {
                        __applicableTags[tag.Name] = true;
                    }
                    else
                    {
                        __applicableTags[tag.Name] = false;
                    }
                }

                //Determine the score of each tag
                var tag_priority = GameObject.GetComponent<SettingsManager>().ModOptions.MusicTagTypePriority;

                for (int i = 0; i < tag_priority.Count; i++)
                {
                    var tagname = tag_priority[i];
                    __tagScore[tagname] = tag_priority.Count - i + 1;
                }
      

                //Find all songs that apply according to selection type
                // Find with most priority score

                int best_score = 0;

                foreach (var kv in SongTags)
                {
                    var applicable = true;
                    var currentsong = kv.Key;

                    foreach (var tagname in kv.Value)
                    {            
                        if (__applicableTags.ContainsKey(tagname))
                        {
                            if (sel_and)
                                applicable &= __applicableTags[tagname];
                            else
                                applicable |= __applicableTags[tagname];
                        }
                    }

                    if (applicable)
                    {                       
                        // Determine the score; if better -> set to new song
                        int score = 0;

                        foreach (var tag in kv.Value)
                        {
                            if(__tagScore.ContainsKey(tag))
                                score += __tagScore[tag];
                        }

                        if (score > best_score)
                        {
                            song = currentsong;
                            best_score = score;
                        }
                    }
                }
            }

            if (song != null)
                return song;

            //Select the "default" tag song if available
            if (TagSongs.ContainsKey("") && TagSongs[""].Count != 0)
                return TagSongs[""][0];

            return null;
        }

       

        public bool Contains(String song)
        {
            return SongTags.ContainsKey(song);
        }
    }
}

