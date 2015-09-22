using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

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
           

           
        }

        /**
         * Adds a song with manually tags
         * */
        public void AddSong(String filename, params String[] tags)
        {
            // Is the basename correct?

            //Add into dictionaries

            //Sort all songs
            foreach (var songs in TagSongs.Values)
            {
                songs.Sort((s1, s2) => SongTags[s1].Count.CompareTo(SongTags[s2].Count));
            }
        }

        public void RemoveSong(String filename)
        {
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
            foreach (MusicEntryTag tag in GameObject.GetComponent<MusicManager>().MusicTagTypes)
            {
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

        public abstract class MusicEntryTag :IComparable
        {
            /**
             * The name of the tag; a song's tag is determined by #<Name>
             * */
            public String Name{get; private set;}

            /*
             * Priority: The lower this value the more priority this tag has
             * */
            public int Priority{ get; private set;}

            public MusicEntryTag(String name, int priority)
            {
                Name = name.ToLower();
                Priority = priority;
            }

            /**
             * Returns if this tag applies to the current game situation
             * */
            public abstract bool TagApplies(GameObject gameObject, AudioManager.ListenerInfo info);

            #region IComparable implementation

            public int CompareTo(object obj)
            {
                if (obj == null || !(obj is MusicEntryTag))
                    return 0;
                return -Priority.CompareTo(((MusicEntryTag)obj).Priority); //Order inverted
            }

            #endregion
        }
    }
}

