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
        public static Dictionary<RadioChannelInfo, HashSet<RadioContentInfo>> DisallowedContent = 
            new Dictionary<RadioChannelInfo, HashSet<RadioContentInfo>>();       

        /// <summary>
        /// Counts how many times the watcher was updated.
        /// </summary>
        private int m_WatcherUpdateTicker = 0;

        private ushort m_currentChannel = 0;
        private string[] m_musicFileBackup = null;

        public RadioContentWatcher()
        {
        }

        public void Start()
        {
            InvokeRepeating("ApplyDisallowedContentRestrictions", 1f, 5f);

            if (ModOptions.Instance.EnableSmoothTransitions)
            {
                InvokeRepeating("ApplySmoothTransition", 1f, 0.5f);
            }

            if (m_musicFileBackup == null)
            {
                AudioManager mgr = Singleton<AudioManager>.instance;
                m_musicFileBackup = ReflectionHelper.GetPrivateField<string[]>(mgr, "m_musicFiles");
            }
        }

        public void OnDestroy()
        {
            if (m_musicFileBackup != null)
            {
                AudioManager mgr = Singleton<AudioManager>.instance;
                ReflectionHelper.SetPrivateField(mgr, "m_musicFiles", m_musicFileBackup);
            }
        }

        /// <summary>
        /// Rebuilds the allowed content for a channel.
        /// </summary>
        /// <param name="channel">Channel.</param>
        private void RebuildDisallowedContentForChannel(RadioChannelData channel)
        {
            if(channel.Info == null)
            {
                return;
            }

            HashSet<RadioContentInfo> disallowed;
			if (!DisallowedContent.TryGetValue(channel.Info, out disallowed))
			{
				disallowed = new HashSet<RadioContentInfo>();
				DisallowedContent[channel.Info] = disallowed;
			}
			else
			{
				disallowed.Clear();
			}

			UserRadioChannel userchannel = AudioManagerHelper.GetUserChannelInfo(channel.Info);

			if (userchannel != null)
			{
				// If the channel is a custom channel, we can check for context and for content disabling
				var allowedcollections = userchannel.GetApplyingContentCollections();

				foreach (UserRadioContent usercontent in userchannel.m_Content)
				{
					if (usercontent.m_VanillaContentInfo != null)
					{
                        bool isincontext = (!ModOptions.Instance.EnableContextSensitivity || allowedcollections.Contains(usercontent.m_Collection));
                        bool isenabled = (!ModOptions.Instance.EnableDisabledContent || AudioManagerHelper.ContentIsEnabled(usercontent.m_VanillaContentInfo));

                        if(!isincontext || !isenabled)
                        {
                            disallowed.Add(usercontent.m_VanillaContentInfo);   
                        }						
					}
				}
			}
			else
			{
				// If the channel is a vanilla channel, we can still disable content
				AudioManager mgr = Singleton<AudioManager>.instance;

				if (mgr.m_radioContents.m_size > 0)
				{
					for (int i = 0; i < mgr.m_radioContents.m_size; ++i)
					{
						var content = mgr.m_radioContents[i];
                        if (content.Info != null && content.Info.m_radioChannels != null && content.Info.m_radioChannels.Contains(channel.Info))
						{
							if (!AudioManagerHelper.ContentIsEnabled(content.Info))
							{
								disallowed.Add(content.Info);
							}
						}
					}
				}
			}
        }

        /// <summary>
        /// Rebuilds the list of allowed content
        /// </summary>
        public void RebuildDisallowedContent()
        {
            if(m_WatcherUpdateTicker++ % 10 == 0)
            {
                AudioManager mgr = Singleton<AudioManager>.instance;

                for (int i = 0; i < mgr.m_radioChannels.m_size; ++i)
                {
                    RebuildDisallowedContentForChannel(mgr.m_radioChannels[i]);
                }
            }
            else
            {
				RadioChannelData? currentchannel = AudioManagerHelper.GetActiveChannelData();

				if (currentchannel != null)
				{
					RebuildDisallowedContentForChannel(currentchannel.Value);
				}
            }
        }

        /// <summary>
        /// Applies the content sensitivity
        /// </summary>
        public void ApplyDisallowedContentRestrictions()
        {
            if (!ModOptions.Instance.EnableContextSensitivity && !ModOptions.Instance.EnableDisabledContent)
                return;

            RebuildDisallowedContent();

            // Find the current content and check if it is in the list of allowed content
            // Otherwise trigger radio content rebuild and stop playback
            RadioChannelData? currentchannel = AudioManagerHelper.GetActiveChannelData();

            if(currentchannel != null)
            {
                RadioContentData? currentcontent = AudioManagerHelper.GetActiveContentInfo();

                if(currentcontent != null && currentcontent.Value.Info != null)
                {
                    HashSet<RadioContentInfo> disallowed;

                    if(DisallowedContent.TryGetValue(currentchannel.Value.Info, out disallowed))
                    {
                        foreach(var v in disallowed)
                        {
                            Debug.Log("Disallowed:" + v.name + "," + v.m_displayName);
                        }

                        if (disallowed != null && disallowed.Contains(currentcontent.Value.Info))
						{							
							AudioManagerHelper.TriggerRebuildInternalSongList();
                            CSLMusicMod.Log("Wrong context for " + currentcontent.Value.Info.m_fileName);
                            AudioManagerHelper.NextTrack();
						}
                    }					
                }
            }
        }

        public void ApplySmoothTransition ()
        {
            RadioChannelData? channel = AudioManagerHelper.GetActiveChannelData ();

            if (channel != null)
            {
                ushort index = channel.Value.m_infoIndex;

                if (m_currentChannel == index)
                    return;

                m_currentChannel = index;

                if (index == 0)
                {
                    if (m_musicFileBackup != null)
                    {
                        AudioManager mgr = Singleton<AudioManager>.instance;
                        ReflectionHelper.SetPrivateField(mgr, "m_musicFiles", m_musicFileBackup);
                    }
                }
                else
                {
                    AudioManager mgr = Singleton<AudioManager>.instance;
                    ReflectionHelper.SetPrivateField(mgr, "m_musicFiles", null);
                }
            }
        }
    }
}

