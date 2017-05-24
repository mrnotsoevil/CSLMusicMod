using System;
using System.Linq;
using UnityEngine;
using ColossalFramework;
using System.Collections.Generic;

namespace CSLMusicMod
{
    /// <summary>
    /// A behavior that periodically checks the currently played radio content.
    /// This is used for disabling, context sensitive etc content.
    /// </summary>
    public class RadioContentWatcher : MonoBehaviour
    {
        public RadioContentWatcher()
        {
        }

        public void Start()
        {
            if(ModOptions.Instance.EnableDisabledContent)
                InvokeRepeating("RemoveDisabledContent", 1f, 0.5f);
            if (ModOptions.Instance.EnableContextSensitivity)
                InvokeRepeating("ApplyContextRules", 1f, 5f);
                
        }

        /// <summary>
        /// Disables radio content that is currently disabled
        /// </summary>
        public void RemoveDisabledContent()
        {
            if (!ModOptions.Instance.EnableDisabledContent)
                return;
            if (ModOptions.Instance.DisabledContent.Count == 0)
                return;

            AudioManager mgr = Singleton<AudioManager>.instance;
            ushort activechannel = ReflectionHelper.GetPrivateField<ushort>(mgr, "m_activeRadioChannel");

            if(activechannel >= 0)
            {
                RadioChannelData data = mgr.m_radioChannels[activechannel];

                if(data.m_currentContent != 0)
                {
                    var content = mgr.m_radioContents[data.m_currentContent];

                    if (content.Info == null)
                        return;

                    string id = content.Info.m_folderName + "/" + content.Info.m_fileName;

                    if(ModOptions.Instance.DisabledContent.Contains(id))
                    {
                        var newcontent = FindNewContent(data.Info);

                        // If no alternatives are existing, ignore
                        if(newcontent != null)
                        {
                            AudioManagerHelper.SwitchToContent(newcontent);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Disables radio content that is not in the current radio context
        /// </summary>
        public void ApplyContextRules()
        {
            if (!ModOptions.Instance.EnableContextSensitivity)
                return;

            AudioManager mgr = Singleton<AudioManager>.instance;
            ushort activechannel = ReflectionHelper.GetPrivateField<ushort>(mgr, "m_activeRadioChannel");


            if(activechannel >= 0)
            {
                RadioChannelData data = mgr.m_radioChannels[activechannel];

                if(data.m_currentContent != 0)
                {
                    UserRadioChannel userchannel;

                    if(LoadingExtension.UserRadioContainer.m_UserRadioDict.TryGetValue(data.Info, out userchannel))
                    {                        
                        var content = mgr.m_radioContents[data.m_currentContent];

                        UserRadioContent usercontent;

                        if(LoadingExtension.UserRadioContainer.m_UserContentDict.TryGetValue(content.Info, out usercontent))
                        {
                            HashSet<String> allowedcollections = userchannel.GetApplyingContentCollections();
                         
                            // Check if the current content's collection is supported
                            if(allowedcollections != null && !allowedcollections.Contains(usercontent.m_Collection))
                            {
                                var newcontent = FindNewContent(data.Info);

                                // If no alternatives are existing, ignore
                                if(newcontent != null)
                                {
                                    AudioManagerHelper.SwitchToContent(newcontent);
                                }
                            }
                        }
                    }
                }
            }
        }

        public RadioContentInfo FindNewContent(RadioChannelInfo channel)
        {
            List<RadioContentInfo> available = new List<RadioContentInfo>();
            HashSet<string> allowedcollections = null; //O(logn) HashSet

            // Handle context-sensitivity
            if(ModOptions.Instance.EnableContextSensitivity)
            {
                UserRadioChannel userchannel;
                if(LoadingExtension.UserRadioContainer.m_Stations.TryGetValue(channel.name, out userchannel))
                {
                    allowedcollections = userchannel.GetApplyingContentCollections();
                }
            }

            // Look for a matching content
            for(uint i = 0; i < PrefabCollection<RadioContentInfo>.PrefabCount(); ++i)
            {
                RadioContentInfo content = PrefabCollection<RadioContentInfo>.GetPrefab(i);

                if (content == null)
                    continue;
                if (content.m_radioChannels == null)
                    continue;

                if(content.m_radioChannels.Contains(channel))
                {
                    string id = content.m_folderName + "/" + content.m_fileName;

                    if (ModOptions.Instance.EnableDisabledContent && ModOptions.Instance.DisabledContent.Contains(id))
                    {
                        continue;
                    }
                    if(allowedcollections != null)
                    {
                        UserRadioContent usercontent;

                        if (LoadingExtension.UserRadioContainer.m_UserContentDict.TryGetValue(content, out usercontent))
                        {
                            if(!allowedcollections.Contains(usercontent.m_Collection))
                            {
                                continue;
                            }
                        }
                    }

                    available.Add(content);
                }
            }

            if (available.Count == 0)
                return null;

            return available[CSLMusicMod.RANDOM.Next(available.Count)];
        }
    }
}

