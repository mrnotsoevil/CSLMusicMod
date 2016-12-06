using System;
using CSLMusicMod.LitJson;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace CSLMusicMod
{
    public class MoodContext : RadioContext
    {    
        public int m_HappinessFrom = 0;

        public int m_HappinessTo = 100;

        public HashSet<String> m_Collections = new HashSet<string>();

        public MoodContext()
        {
        }

        public bool Applies()
        {
            int finalHappiness = (int)Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_finalHappiness;

            return finalHappiness >= m_HappinessFrom && finalHappiness <= m_HappinessTo;
        }

        public HashSet<String> GetCollections()
        {
            return m_Collections;
        }

        public static MoodContext LoadFromJson(JsonData json)
        {
            MoodContext context = new MoodContext();

            context.m_HappinessFrom = (int)json["from"];
            context.m_HappinessTo = (int)json["to"];

            foreach(JsonData e in json["collections"])
            {
                context.m_Collections.Add((String)e);
            }

            return context;
        }
    }
}

