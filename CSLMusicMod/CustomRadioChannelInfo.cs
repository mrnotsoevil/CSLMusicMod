using System;
using System.Linq;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// Used for detours of RadioChannelInfo. See Detours class for the detour code.
    /// </summary>
    public class CustomRadioChannelInfo : RadioChannelInfo
    {
        public CustomRadioChannelInfo()
        {
        }

        /// <summary>
        /// The game usually translates all radio channel names with a translation table.
        /// If a name is not in this table, an error occurs. This detour looks if the station is custom
        /// and returns the correct title.
        /// </summary>
        /// <returns>Title of a radio station wrt custom stations</returns>
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

