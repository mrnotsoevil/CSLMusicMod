using System;
using ColossalFramework;
using UnityEngine;

namespace CSLMusicMod.Helpers
{
    public static class AudioManagerHelper
    {
        public static bool NextTrack()
        {
            AudioManager mgr = Singleton<AudioManager>.instance;         

            if(ReflectionHelper.GetPrivateField<bool>(mgr, "m_musicFileIsRadio"))
            {
                Debug.Log("[CSLMusic] Radio switches to next track");

                var player = ReflectionHelper.GetPrivateField<AudioManager.AudioPlayer>(mgr, "m_currentRadioPlayer");
                player.m_source.Stop();

                return true;
            }
            else
            {
                return false;
            }
        }

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

