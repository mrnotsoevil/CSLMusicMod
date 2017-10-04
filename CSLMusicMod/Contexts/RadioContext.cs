using System;
using System.Collections.Generic;
using System.Linq;
using CSLMusicMod.LitJson;

namespace CSLMusicMod.Contexts
{
    /// <summary>
    /// Allows context-sensitive music to be played.
    /// This is specific to a radio station.
    /// </summary>
    public class RadioContext
    {
        public UserRadioChannel m_RadioChannel;
        
        public HashSet<string> m_Collections = new HashSet<string>();
        
        public HashSet<string> m_Songs = new HashSet<string>();

        /// <summary>
        /// The songs that are associated to this context
        /// </summary>
        private HashSet<UserRadioContent> m_AttachedSongs = null;

        /// <summary>
        /// Stores the literals (radio context conditions) in disjunctive normal form.
        /// </summary>
        public List<List<RadioContextCondition>> m_Conditions = new List<List<RadioContextCondition>>();

        public RadioContext()
        {
        }

        /// <summary>
        /// Returns if there is one of the station defined conditions that apply.
        /// </summary>
        /// <returns>true if one set of conditions apply</returns>
        public bool Applies()
        {
            foreach(var conj in m_Conditions)
            {
                bool applies = true;

                foreach(RadioContextCondition cond in conj)
                {
                    applies &= cond.Applies();

                    if(!applies)
                    {
                        break;
                    }
                }

                if (applies)
                    return true;
            }


            return false;
        }
        
        /// <summary>
        /// Loads the context definition from JSON formatted data
        /// </summary>
        /// <returns>The RadioContext loaded from JSON</returns>
        /// <param name="json">JSON data</param>
        public static RadioContext LoadFromJson(JsonData json, Dictionary<string, RadioContextCondition> namedConditions)
        {
            RadioContext radiocontext = new RadioContext();

            foreach(JsonData conj in json["conditions"])
            {
                radiocontext.m_Conditions.Add(new List<RadioContextCondition>());

                foreach(JsonData entry in conj)
                {
                    if (entry.IsObject)
                    {
                        RadioContextCondition context = RadioContextCondition.LoadFromJsonUsingType(entry);

                        if(context != null)
                        {
                            radiocontext.m_Conditions.Last().Add(context);
                        }
                    }
                    else if (entry.IsString)
                    {
                        RadioContextCondition context;

                        if (namedConditions.TryGetValue(entry.ToString(), out context))
                        {
                            radiocontext.m_Conditions.Last().Add(context);
                        }
                        else
                        {
                            CSLMusicMod.Log("Could not find named condition " + entry + "!");
                        }
                    }
                    
                }
            }

            foreach(JsonData e in json["collections"])
            {
                radiocontext.m_Collections.Add((String)e);
            }

            if (json.Keys.Contains("songs"))
            {
                foreach (JsonData e in json["songs"])
                {
                    radiocontext.m_Songs.Add((String) e);
                }
            }
            

            return radiocontext;
        }

        public HashSet<UserRadioContent> GetAttachedSongs()
        {
            if (m_AttachedSongs == null)
            {
                m_AttachedSongs = new HashSet<UserRadioContent>();
                foreach (var song in m_RadioChannel.m_Content)
                {
                    if (m_Collections.Contains(song.m_Collection))
                    {
                        if (m_Songs.Count == 0 || m_Songs.Contains(song.m_DisplayName))
                        {
                            m_AttachedSongs.Add(song);
                        }
                    }
                }
            }

            return m_AttachedSongs;
        }
    }
}

