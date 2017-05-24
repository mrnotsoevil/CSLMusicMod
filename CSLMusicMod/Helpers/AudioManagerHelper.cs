using System;
using System.Reflection;
using System.Linq;
using ColossalFramework;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// Contains some helper functions that work the AudioManager class of the game
    /// </summary>
    public static class AudioManagerHelper
    {
        /// <summary>
        /// Switches to the next station
        /// </summary>
        /// <returns><c>true</c>, if it was possible to switch to the next station, <c>false</c> otherwise.</returns>
        public static bool NextStation()
        {
            var radiopanel = Resources.FindObjectsOfTypeAll<RadioPanel>().FirstOrDefault();

            if(radiopanel != null)
            {
                RadioChannelInfo current = ReflectionHelper.GetPrivateField<RadioChannelInfo>(radiopanel, "m_selectedStation");
                RadioChannelInfo[] stations = ReflectionHelper.GetPrivateField<RadioChannelInfo[]>(radiopanel, "m_stations");

                if(stations != null && stations.Length != 0)
                {
                    int index = current != null ? Array.IndexOf(stations, current) : 0;
                    if (index == -1)
                        index = 0;
                    else
                        index = (index + 1) % stations.Length;

                    RadioChannelInfo next = stations[index];

                    if(next != null)
                    {
                        ReflectionHelper.InvokePrivateVoidMethod(radiopanel, "SelectStation", next);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Switches to the next track.
        /// </summary>
        /// <returns><c>true</c>, if it was possible to switch to the next track, <c>false</c> otherwise.</returns>
        public static bool NextTrack()
        {
            AudioManager mgr = Singleton<AudioManager>.instance;         

            if(ReflectionHelper.GetPrivateField<bool>(mgr, "m_musicFileIsRadio"))
            {
                Debug.Log("[CSLMusic] Radio switches to next track");

                var player = ReflectionHelper.GetPrivateField<AudioManager.AudioPlayer>(mgr, "m_currentRadioPlayer");

                if(player != null)
                    player.m_source.Stop();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Switches to a specific radio content (music)
        /// </summary>
        /// <returns><c>true</c>, if the switch was successful, <c>false</c> otherwise.</returns>
        /// <param name="info">Info.</param>
        public static bool SwitchToContent(RadioContentInfo info)
        {
            AudioManager mgr = Singleton<AudioManager>.instance;

            if(ReflectionHelper.GetPrivateField<bool>(mgr, "m_musicFileIsRadio"))
            {
                ushort contentindex = 0;
                bool found = false;

                for(int i = 0; i < mgr.m_radioContentCount; ++i)
                {
                    RadioContentData data = mgr.m_radioContents[i];

                    //Debug.Log("CC: " + data + " + " + data.Info + " == " + info);

                    if(data.Info == info)
                    {
                        contentindex = (ushort)i;
                        //Debug.Log("Found content index for " + info);
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    Debug.Log("[CSLMusic] Switching to unloaded music " + info);

                    if(!mgr.CreateRadioContent(out contentindex, info))
                    {
                        Debug.Log("[CSLMusic] ... failed to create content " + info);
                        return false;
                    }
                }

                Debug.Log("[CSLMusic] Radio switches to track " + info);

                //Debug.Log("Content index: " + contentindex);

                // Next content
                ushort activechannel = ReflectionHelper.GetPrivateField<ushort>(mgr, "m_activeRadioChannel");

                if(activechannel >= 0)
                {
                    RadioChannelData data = mgr.m_radioChannels[activechannel];
                    data.m_currentContent = contentindex;
                    //data.m_nextContent = contentindex;
                    mgr.m_radioChannels[activechannel] = data;
                    //mgr.m_radioChannels[activechannel].ChangeContent(activechannel);

                    return true;
                }

                //var player = ReflectionHelper.GetPrivateField<AudioManager.AudioPlayer>(mgr, "m_currentRadioPlayer");
                //player.m_source.Stop();

                return false;
            }
            else
            {
                return false;
            }
        }
    }
}

