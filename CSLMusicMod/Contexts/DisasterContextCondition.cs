﻿using System;
using CSLMusicMod.LitJson;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace CSLMusicMod
{
    public class DisasterContextCondition : RadioContextCondition
    {    
        public int m_DisasterCountFrom = 0;

        public int m_DisasterCountTo = DisasterManager.MAX_DISASTER_COUNT;

        public HashSet<string> m_DisasterFilter = new HashSet<string>();

        public HashSet<String> m_Collections = new HashSet<string>();

        public DisasterContextCondition()
        {
        }

        public bool Applies()
        {
            int disasterCount = Singleton<DisasterManager>.instance.m_disasterCount;

            if(m_DisasterFilter.Count == 0)
            {
                return disasterCount >= m_DisasterCountFrom && disasterCount <= m_DisasterCountTo;
            }
            else
            {
                int count = 0;

                for(int i = 0; i < disasterCount; ++i)
                {
                    DisasterInfo info = Singleton<DisasterManager>.instance.m_disasters[i].Info;

                    if(info != null)
                    {
                        if(m_DisasterFilter.Contains(info.name))
                        {
                            ++count;
                        }
                    }
                }

                return count >= m_DisasterCountFrom && count <= m_DisasterCountTo;
            }
        }

        public static DisasterContextCondition LoadFromJson(JsonData json)
        {
            DisasterContextCondition context = new DisasterContextCondition();

            context.m_DisasterCountFrom = (int)json["from"];
            context.m_DisasterCountTo = (int)json["to"];

            if (json.Keys.Contains("of"))
            {
                foreach (JsonData e in json["of"])
                {
                    context.m_DisasterFilter.Add((String)e);
                }
            }

            return context;
        }
    }
}
