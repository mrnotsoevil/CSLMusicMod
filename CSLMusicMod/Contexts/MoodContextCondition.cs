using System;
using CSLMusicMod.LitJson;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// This condition handles the mood of a city (the happiness)
    /// </summary>
    public class MoodContextCondition : RadioContextCondition
    {    
        public int m_HappinessFrom = 0;

        public int m_HappinessTo = 100;

        public bool m_Invert = false;

        public HashSet<String> m_Collections = new HashSet<string>();

        public MoodContextCondition()
        {
        }

        public override bool Applies()
        {
            return m_Invert ? !_Applies() : _Applies();
        }

        private bool _Applies()
        {
            int finalHappiness = (int)Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_finalHappiness;

            return finalHappiness >= m_HappinessFrom && finalHappiness <= m_HappinessTo;
        }

        public static MoodContextCondition LoadFromJson(JsonData json)
        {
            MoodContextCondition context = new MoodContextCondition();

            context.m_HappinessFrom = (int)json["from"];
            context.m_HappinessTo = (int)json["to"];

            if(json.Keys.Contains("not"))
            {
                context.m_Invert = (bool)json["not"];
            }

            return context;
        }
    }
}

