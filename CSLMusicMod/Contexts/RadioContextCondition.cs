using System;
using System.Collections.Generic;
using CSLMusicMod.Contexts;
using CSLMusicMod.LitJson;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// General interface for a context condition.
    /// </summary>
    public abstract class RadioContextCondition
    {
        public abstract bool Applies();

        public static RadioContextCondition LoadFromJsonUsingType(JsonData json)
        {
            RadioContextCondition context = null;

            switch((String)json["type"])
            {
                case "time":
                    context = TimeContextCondition.LoadFromJson(json);
                    break;
                case "weather":
                    context = WeatherContextCondition.LoadFromJson(json);
                    break;
                case "mood":
                    context = MoodContextCondition.LoadFromJson(json);
                    break;
                case "disaster":
                    context = DisasterContextCondition.LoadFromJson(json);
                    break;
                default:
                    Debug.LogError("[CSLMusic] Error: Unknown context type!");
                    break;
            }

            return context;
        }
    }
}

