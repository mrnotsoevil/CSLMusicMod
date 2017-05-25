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
        public static Dictionary<RadioChannelInfo, HashSet<RadioContentInfo>> AllowedContent = 
            new Dictionary<RadioChannelInfo, HashSet<RadioContentInfo>>();

        public RadioContentWatcher()
        {
        }

        public void Start()
        {
            InvokeRepeating("ApplyAllowedContentRestrictions", 1f, 5f);
        }

        /// <summary>
        /// Rebuilds the list of allowed content based on the currently selected channel and the current context
        /// </summary>
        public void RebuildAllowedContent()
        {           
            RadioChannelData? currentchannel = AudioManagerHelper.GetActiveChannelData();

            if(currentchannel != null)
            {
                HashSet<RadioContentInfo> allowed;
                if(!AllowedContent.TryGetValue(currentchannel.Value.Info, out allowed))
                {
                    allowed = new HashSet<RadioContentInfo>();
                    AllowedContent[currentchannel.Value.Info] = allowed;
                }
                else
                {
                    allowed.Clear();
                }

                UserRadioChannel userchannel = AudioManagerHelper.GetUserChannelInfo(currentchannel.Value.Info);

                if(userchannel != null)
                {
                    // If the channel is a custom channel, we can check for context and for content disabling
                    var allowedcollections = userchannel.GetApplyingContentCollections();

                    foreach(UserRadioContent usercontent in userchannel.m_Content)
                    {
                        if(usercontent.m_VanillaContentInfo != null && 
                           allowedcollections.Contains(usercontent.m_Collection) && 
                           ContentIsEnabled(usercontent.m_VanillaContentInfo))
                        {
                            allowed.Add(usercontent.m_VanillaContentInfo);
                        }
                    }
                }
                else
                {
                    // If the channel is a vanilla channel, we can still disable content
                    AudioManager mgr = Singleton<AudioManager>.instance;

                    if(mgr.m_radioContentInfoCount > 0)
                    {
                        for (int i = 0; i < mgr.m_radioContentInfoCount; ++i)
                        {
                            var content = mgr.m_radioContents[i];
                            if (content.Info.m_radioChannels.Contains(currentchannel.Value.Info))
                            {
                                if(ContentIsEnabled(content.Info))
                                {
                                    allowed.Add(content.Info);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies the content sensitivity
        /// </summary>
        public void ApplyAllowedContentRestrictions()
        {
            if (!ModOptions.Instance.EnableContextSensitivity)
                return;

            RebuildAllowedContent();

            // Find the current content and check if it is in the list of allowed content
            // Otherwise trigger radio content rebuild and stop playback
            RadioChannelData? currentchannel = AudioManagerHelper.GetActiveChannelData();

            if(currentchannel != null)
            {
                RadioContentData? currentcontent = AudioManagerHelper.GetActiveContentInfo();

                if(currentcontent != null)
                {
                    HashSet<RadioContentInfo> allowed;

                    if(AllowedContent.TryGetValue(currentchannel.Value.Info, out allowed))
                    {
                        // Special case: allowed content is null or empty: Then just play everything
                        if (allowed != null && allowed.Count != 0 && !allowed.Contains(currentcontent.Value.Info))
						{
							CSLMusicMod.Log("Wrong context for " + currentcontent.Value.Info.m_fileName);
							AudioManagerHelper.TriggerRebuildInternalSongList();
							AudioManagerHelper.NextTrack();
						}
                    }					
                }
            }
        }

        /// <summary>
        /// Returns true if a radio content info is disabled.
        /// </summary>
        /// <returns><c>true</c>, if is marked as disabled, <c>false</c> otherwise.</returns>
        /// <param name="info">Info.</param>
        public static bool ContentIsEnabled(RadioContentInfo info)
        {
            if (info == null)
                return true;

			string id = info.m_folderName + "/" + info.m_fileName;
            return !ModOptions.Instance.DisabledContent.Contains(id);			
        }

    }
}

