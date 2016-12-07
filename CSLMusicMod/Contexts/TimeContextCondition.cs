using System;
using CSLMusicMod.LitJson;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace CSLMusicMod
{
    public class TimeContextCondition : RadioContextCondition
    {
        public float m_TimeFrom = 0;

        public float m_TimeTo = 24;

        public HashSet<String> m_Collections = new HashSet<string>();

        public TimeContextCondition()
        {
        }

        public bool Applies()
        {
            float currenttime = Singleton<SimulationManager>.instance.m_currentDayTimeHour;

            if(m_TimeFrom < m_TimeTo)
            {
                return currenttime >= m_TimeFrom && currenttime <= m_TimeTo;
            }
            else if(m_TimeTo < m_TimeFrom)
            {
                return currenttime >= m_TimeFrom || currenttime <= m_TimeTo;
            }
            else
            {
                return true;
            }
        }

        public static TimeContextCondition LoadFromJson(JsonData json)
        {
            TimeContextCondition context = new TimeContextCondition();

            context.m_TimeFrom = (int)json["from"];
            context.m_TimeTo = (int)json["to"];


            return context;
        }
    }
}

