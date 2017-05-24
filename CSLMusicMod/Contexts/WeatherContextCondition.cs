using System;
using CSLMusicMod.LitJson;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// Allows conditions based on the current weather
    /// </summary>
    public class WeatherContextCondition : RadioContextCondition
    { 
        public float m_TempFrom = -100;

        public float m_TempTo = 100;

        public float m_RainFrom = 0;

        public float m_RainTo = 1;

        public float m_CloudFrom = 0;

        public float m_CloudTo = 1;

        public float m_FogFrom = 0;

        public float m_FogTo = 1;

        public float m_RainbowFrom = 0;

        public float m_RainbowTo = 1;

        public float m_NorthernLightsFrom = 0;

        public float m_NorthernLightsTo = 1;

        public bool m_Invert = false;

        public WeatherContextCondition()
        {
        }

        public bool Applies()
        {
            return m_Invert ? !_Applies() : _Applies();
        }

        private bool _Applies()
        {
            float temp = Singleton<WeatherManager>.instance.m_currentTemperature;

            if (temp < m_TempFrom || temp > m_TempTo)
                return false;

            float rain = Singleton<WeatherManager>.instance.m_currentRain;

            if (rain < m_RainFrom || rain > m_RainTo)
                return false;

            float cloud = Singleton<WeatherManager>.instance.m_currentCloud;

            if (cloud < m_CloudFrom || cloud > m_CloudTo)
                return false;
            
            float fog = Singleton<WeatherManager>.instance.m_currentFog;

            if (fog < m_FogFrom || fog > m_FogTo)
                return false;
            
            float rainbow = Singleton<WeatherManager>.instance.m_targetRainbow;

            if (rainbow < m_RainbowFrom || rainbow > m_RainbowTo)
                return false;

            float northernlights = Singleton<WeatherManager>.instance.m_currentNorthernLights;

            if (northernlights < m_NorthernLightsFrom || northernlights > m_NorthernLightsTo)
                return false;

            return true;
        }


        public static WeatherContextCondition LoadFromJson(JsonData json)
        {
            WeatherContextCondition context = new WeatherContextCondition();

            if(json.Keys.Contains("temperature"))
            {
                context.m_TempFrom = (float)((int)json["temperature"][0]);
                context.m_TempTo = (float)((int)json["temperature"][1]);
            }
            if(json.Keys.Contains("rain"))
            {
                context.m_RainFrom = (float)((int)json["rain"][0]) / 10f;
                context.m_RainTo = (float)((int)json["rain"][1]) / 10f;
            }
            if(json.Keys.Contains("cloudy"))
            {
                context.m_CloudFrom = (float)((int)json["cloudy"][0]) / 10f;
                context.m_CloudTo = (float)((int)json["cloudy"][1]) / 10f;
            }
            if(json.Keys.Contains("foggy"))
            {
                context.m_FogFrom = (float)((int)json["foggy"][0]) / 10f;
                context.m_FogTo = (float)((int)json["foggy"][1]) / 10f;
            }
            if(json.Keys.Contains("rainbow"))
            {
                context.m_RainbowFrom = (float)((int)json["rainbow"][0]) / 10f;
                context.m_RainbowTo = (float)((int)json["rainbow"][1]) / 10f;
            }
            if(json.Keys.Contains("northernlights"))
            {
                context.m_NorthernLightsFrom = (float)((int)json["northernlights"][0]) / 10f;
                context.m_NorthernLightsTo = (float)((int)json["northernlights"][1]) / 10f;
            }
            if(json.Keys.Contains("not"))
            {
                context.m_Invert = (bool)json["not"];
            }

            return context;
        }


    }
}

