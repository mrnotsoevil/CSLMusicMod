using System;
using System.Linq;
using UnityEngine;

namespace CSLMusicMod
{
    public class CustomRadioChannelInfo : RadioChannelInfo
    {
        public CustomRadioChannelInfo()
        {
        }

        public string CustomGetLocalizedTitle()
        {
            UserRadioCollection collection = Resources.FindObjectsOfTypeAll<UserRadioCollection>().FirstOrDefault();

            if (collection != null && collection.m_Stations.ContainsKey(name))
            {    
                return name;
            }
            else
            {
                return ColossalFramework.Globalization.Locale.Get("RADIO_CHANNEL_TITLE", base.gameObject.name);
            }

        }
    }
}

