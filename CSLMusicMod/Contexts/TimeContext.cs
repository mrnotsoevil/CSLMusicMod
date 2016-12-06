using System;
using CSLMusicMod.LitJson;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace CSLMusicMod
{
    public class TimeContext : RadioContext
    {
        public float m_TimeFrom = 0;

        public float m_TimeTo = 24;

        public HashSet<String> m_Collections = new HashSet<string>();

        public TimeContext()
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

        public HashSet<String> GetCollections()
        {
            return m_Collections;
        }

        public static TimeContext LoadFromJson(JsonData json)
        {
            TimeContext context = new TimeContext();

            context.m_TimeFrom = (int)json["from"];
            context.m_TimeTo = (int)json["to"];

            foreach(JsonData e in json["collections"])
            {
                context.m_Collections.Add((String)e);
            }

            return context;
        }
    }
}

