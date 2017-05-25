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
        public static Dictionary<RadioChannelInfo, HashSet<RadioContentInfo>> AllowedContent_ContextSensitivity = 
            new Dictionary<RadioChannelInfo, HashSet<RadioContentInfo>>();

        public RadioContentWatcher()
        {
        }

        public void Start()
        {
            if (ModOptions.Instance.EnableContextSensitivity)
                InvokeRepeating("ApplyContextRules", 1f, 5f);
        }

        /// <summary>
        /// Rebuilds the list of allowed content based on the currently selected channel and the current context
        /// </summary>
        private void RebuildContextRulesAllowedContent()
        {           
            RadioChannelData? currentchannel = AudioManagerHelper.GetActiveChannelData();

            if(currentchannel != null)
            {
                HashSet<RadioContentInfo> allowed;
                if(!AllowedContent_ContextSensitivity.TryGetValue(currentchannel.Value.Info, out allowed))
                {
                    allowed = new HashSet<RadioContentInfo>();
                    AllowedContent_ContextSensitivity[currentchannel.Value.Info] = allowed;
                }
                else
                {
                    allowed.Clear();
                }

                UserRadioChannel userchannel = AudioManagerHelper.GetUserChannelInfo(currentchannel.Value.Info);

                if(userchannel != null)
                {
                    var allowedcollections = userchannel.GetApplyingContentCollections();

                    foreach(UserRadioContent usercontent in userchannel.m_Content)
                    {
                        if(usercontent.m_VanillaContentInfo != null && allowedcollections.Contains(usercontent.m_Collection))
                        {
                            allowed.Add(usercontent.m_VanillaContentInfo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies the content sensitivity
        /// </summary>
        public void ApplyContextRules()
        {
            RebuildContextRulesAllowedContent();

            // Find the current content and check if it is in the list of allowed content
            // Otherwise trigger radio content rebuild and stop playback
            RadioChannelData? currentchannel = AudioManagerHelper.GetActiveChannelData();

            if(currentchannel != null)
            {
                RadioContentData? currentcontent = AudioManagerHelper.GetActiveContentInfo();

                if(currentcontent != null)
                {
                    HashSet<RadioContentInfo> allowed;

                    if(AllowedContent_ContextSensitivity.TryGetValue(currentchannel.Value.Info, out allowed))
                    {
						if (!allowed.Contains(currentcontent.Value.Info))
						{
							CSLMusicMod.Log("Wrong context for " + currentcontent.Value.Info.m_fileName);
							AudioManagerHelper.TriggerRebuildInternalSongList();
							AudioManagerHelper.NextTrack();
						}
                    }					
                }
            }
        }

   //     /// <summary>
   //     /// Returns true if a radio content info is disabled.
   //     /// </summary>
   //     /// <returns><c>true</c>, if is marked as disabled, <c>false</c> otherwise.</returns>
   //     /// <param name="info">Info.</param>
   //     public static bool ContentIsEnabled(RadioContentInfo info)
   //     {
   //         if (info == null)
   //             return true;

			//string id = info.m_folderName + "/" + info.m_fileName;
   //         return !ModOptions.Instance.DisabledContent.Contains(id);			
   //     }

   //     /// <summary>
   //     /// Disables radio content that is currently disabled
   //     /// </summary>
   //     public void RemoveDisabledContent()
   //     {
   //         if (!ModOptions.Instance.EnableDisabledContent)
   //             return;
   //         if (ModOptions.Instance.DisabledContent.Count == 0)
   //             return;

   //         AudioManager mgr = Singleton<AudioManager>.instance;
   //         ushort activechannel = ReflectionHelper.GetPrivateField<ushort>(mgr, "m_activeRadioChannel");

   //         if(activechannel >= 0)
   //         {
   //             RadioChannelData data = mgr.m_radioChannels[activechannel];

   //             if(data.m_currentContent != 0)
   //             {
   //                 var content = mgr.m_radioContents[data.m_currentContent];

   //                 if(!ContentIsEnabledcontent.Info))
   //                 {
   //                     var newcontent = FindNewContent(data.Info);

   //                     // If no alternatives are existing, ignore
   //                     if(newcontent != null)
   //                     {
   //                         AudioManagerHelper.SwitchToContent(newcontent);
   //                     }
   //                 }

   //             }
   //         }
   //     }

   //     public static bool ContentIsInContext(RadioChannelInfo channel_info, RadioContentInfo content_info)
   //     {
   //         if (channel_info == null)
   //             return true;
			//if (content_info == null)
			//	return true;

			//UserRadioContent usercontent;

    //        if (LoadingExtension.UserRadioContainer.m_UserContentDict.TryGetValue(content_info, out usercontent))
    //        {
				//HashSet<String> allowedcollections = userchannel.GetApplyingContentCollections();

        //        // Check if the current content's collection is supported
        //        if (allowedcollections != null && !allowedcollections.Contains(usercontent.m_Collection))
        //        {
        //        }
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        ///// <summary>
        ///// Disables radio content that is not in the current radio context
        ///// </summary>
        //public void ApplyContextRules()
        //{
        //    if (!ModOptions.Instance.EnableContextSensitivity)
        //        return;

        //    RadioChannelData? currentchannel = AudioManagerHelper.GetActiveChannelData();

        //    if(currentchannel != null)
        //    {
        //        RadioChannelData data = currentchannel.Value;

        //        if(data.m_currentContent != 0)
        //        {
        //            UserRadioChannel userchannel = AudioManagerHelper.GetUserChannelInfo(data.Info);

        //            if(userchannel != null)
        //            {                   
        //                AudioManager mgr = Singleton<AudioManager>.instance;
        //                var content = mgr.m_radioContents[data.m_currentContent];

        //                UserRadioContent usercontent = AudioManagerHelper.GetUserContentInfo(content.Info);

        //                if(usercontent != null)
        //                {
        //                    HashSet<String> allowedcollections = userchannel.GetApplyingContentCollections();
                         
        //                    // Check if the current content's collection is supported
        //                    if(allowedcollections != null && !allowedcollections.Contains(usercontent.m_Collection))
        //                    {
        //                        var newcontent = FindNewContent(data.Info);

        //                        // If no alternatives are existing, ignore
        //                        if(newcontent != null)
        //                        {
        //                            AudioManagerHelper.SwitchToContent(newcontent);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public RadioContentInfo FindNewContent(RadioChannelInfo channel)
        //{
        //    List<RadioContentInfo> available = new List<RadioContentInfo>();
        //    HashSet<string> allowedcollections = null; //O(logn) HashSet

        //    // Handle context-sensitivity
        //    if(ModOptions.Instance.EnableContextSensitivity)
        //    {
        //        UserRadioChannel userchannel;
        //        if(LoadingExtension.UserRadioContainer.m_Stations.TryGetValue(channel.name, out userchannel))
        //        {
        //            allowedcollections = userchannel.GetApplyingContentCollections();
        //        }
        //    }

        //    // Look for a matching content
        //    for(uint i = 0; i < PrefabCollection<RadioContentInfo>.PrefabCount(); ++i)
        //    {
        //        RadioContentInfo content = PrefabCollection<RadioContentInfo>.GetPrefab(i);

        //        if (content == null)
        //            continue;
        //        if (content.m_radioChannels == null)
        //            continue;

        //        if(content.m_radioChannels.Contains(channel))
        //        {
        //            string id = content.m_folderName + "/" + content.m_fileName;

        //            if (ModOptions.Instance.EnableDisabledContent && ModOptions.Instance.DisabledContent.Contains(id))
        //            {
        //                continue;
        //            }
        //            if(allowedcollections != null)
        //            {
        //                UserRadioContent usercontent;

        //                if (LoadingExtension.UserRadioContainer.m_UserContentDict.TryGetValue(content, out usercontent))
        //                {
        //                    if(!allowedcollections.Contains(usercontent.m_Collection))
        //                    {
        //                        continue;
        //                    }
        //                }
        //            }

        //            available.Add(content);
        //        }
        //    }

        //    if (available.Count == 0)
        //        return null;

        //    return available[CSLMusicMod.RANDOM.Next(available.Count)];
        //}
    }
}

