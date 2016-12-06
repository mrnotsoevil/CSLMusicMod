using System;
using CSLMusicMod.LitJson;
using System.Collections.Generic;
using UnityEngine;

namespace CSLMusicMod
{
    public class RadioContext
    {
        public HashSet<string> m_Collections = new HashSet<string>();

        public List<RadioContextCondition> m_Conditions = new List<RadioContextCondition>();

        public RadioContext()
        {
        }

        public bool Applies()
        {
            foreach(RadioContextCondition cond in m_Conditions)
            {
                if(!cond.Applies())
                {
                    return false;
                }
            }

            return true;
        }

        public static RadioContext LoadFromJson(JsonData json)
        {
            RadioContext radiocontext = new RadioContext();

            foreach(JsonData entry in json["conditions"])
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
                    default:
                        Debug.Log("[CSLMusic] Error: Unknown context type!");
                        break;
                }

                if(context != null)
                {
                    radiocontext.m_Conditions.Add(context);
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

