using System;
using System.Linq;
using CSLMusicMod.LitJson;
using System.Collections.Generic;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// Allows context-sensitive music to be played.
    /// This is specific to a radio station.
    /// </summary>
    public class RadioContext
    {
        public HashSet<string> m_Collections = new HashSet<string>();

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
        public static RadioContext LoadFromJson(JsonData json)
        {
            RadioContext radiocontext = new RadioContext();

            foreach(JsonData conj in json["conditions"])
            {
                radiocontext.m_Conditions.Add(new List<RadioContextCondition>());

                foreach(JsonData entry in conj)
                {
                    RadioContextCondition context = null;

                    switch((String)entry["type"])
                    {
                        case "time":
                            context = TimeContextCondition.LoadFromJson(entry);
                            break;
                        case "weather":
                            context = WeatherContextCondition.LoadFromJson(entry);
                            break;
                        case "mood":
                            context = MoodContextCondition.LoadFromJson(entry);
                            break;
                        case "disaster":
                            context = DisasterContextCondition.LoadFromJson(entry);
                            break;
                        default:
                            Debug.LogError("[CSLMusic] Error: Unknown context type!");
                            break;
                    }

                    if(context != null)
                    {
                        radiocontext.m_Conditions.Last().Add(context);
                    }
                }
            }

            foreach(JsonData e in json["collections"])
            {
                radiocontext.m_Collections.Add((String)e);
            }

            return radiocontext;
        }
    }
}

