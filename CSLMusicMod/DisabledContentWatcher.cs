using System;
using System.Linq;
using UnityEngine;
using ColossalFramework;
using System.Collections.Generic;
using CSLMusicMod.Helpers;

namespace CSLMusicMod
{
    public class DisabledContentWatcher : MonoBehaviour
    {
        private ModOptions m_ModOptions = ModOptions.Instance;

        public DisabledContentWatcher()
        {
        }

        public void Start()
        {
            InvokeRepeating("RemoveDisabledContent", 1f, 0.5f);
        }

        public void RemoveDisabledContent()
        {
            AudioManager mgr = Singleton<AudioManager>.instance;

            for(int i = 0; i < mgr.m_radioChannelCount; ++i)
            {
                RadioChannelData data = mgr.m_radioChannels[i];

                if(data.m_currentContent != 0)
                {
                    var content = mgr.m_radioContents[data.m_currentContent];
                    string id = content.Info.m_folderName + "/" + content.Info.m_fileName;

                    if(m_ModOptions.DisabledContent.Contains(id))
                    {
                        //Debug.Log("Removing content " + id);

                        var newcontent = FindNewContent(data.Info);

                        // If no alternatives are existing, ignore
                        if(newcontent != null)
                        {
                            AudioManagerHelper.SwitchToContent(newcontent);
                        }
                        return;
                    }

                }
            }
        }

        public RadioContentInfo FindNewContent(RadioChannelInfo channel)
        {
            List<RadioContentInfo> available = new List<RadioContentInfo>();

            for(uint i = 0; i < PrefabCollection<RadioContentInfo>.PrefabCount(); ++i)
            {
                RadioContentInfo content = PrefabCollection<RadioContentInfo>.GetPrefab(i);

                if(content.m_radioChannels.Contains(channel))
                {
                    string id = content.m_folderName + "/" + content.m_fileName;

                    if (!m_ModOptions.DisabledContent.Contains(id))
                    {
                        available.Add(content);
                    }
                }
            }

            if (available.Count == 0)
                return null;

            return available[CSLMusicMod.RANDOM.Next(available.Count)];
        }
    }
}

