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
        public GameObject GameObject { get; private set;}
        public String BaseName { get; private set;}

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
            string[] cell = Path.GetFileNameWithoutExtension(filename).Split('#');
            string[] tags = new string[cell.Length - 1];

            for (int i = 1; i < cell.Length; i++)
            {
                tags[i - 1] = cell[i];
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
         * If there is a matching music provided by a tag, return the tag music with the highest priority
         * If no tag is matching and there is a default music, return the default
         * If no tag is matching and there is no default music, return null
         * 
         * Always choose the song with the lowest tag count
         * */
        public String GetMatchingMusic(AudioManager.ListenerInfo info)
        {
            var tagtypes = GameObject.GetComponent<MusicManager>().MusicTagTypes;
            var tagpriority = GameObject.GetComponent<SettingsManager>().ModOptions.MusicTagTypePriority;

            foreach (String tagname in tagpriority)
            {
                var tag = tagtypes[tagname];

                if (tag.TagApplies(GameObject, info))
                {
                    if (TagSongs.ContainsKey(tag.Name) && TagSongs[tag.Name].Count != 0)
                    {
                        //Select the song with the least tag count
                        List<String> songs = TagSongs[tag.Name];

                        return songs.Last();
                    }
                }
            }

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

