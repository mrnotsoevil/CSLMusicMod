using System;
using CSLMusicMod.LitJson;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace CSLMusicMod
{
    public class MoodContextCondition : RadioContextCondition
    {    
        public int m_HappinessFrom = 0;

        public int m_HappinessTo = 100;

        public HashSet<String> m_Collections = new HashSet<string>();

        public MoodContextCondition()
        {
        }

        public bool Applies()
        {
            int finalHappiness = (int)Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_finalHappiness;

            return finalHappiness >= m_HappinessFrom && finalHappiness <= m_HappinessTo;
        }

        public static MoodContextCondition LoadFromJson(JsonData json)
        {
            MoodContextCondition context = new MoodContextCondition();

            context.m_HappinessFrom = (int)json["from"];
            context.m_HappinessTo = (int)json["to"];

            return context;
        }
    }
}

