using System;
using System.Threading;
using UnityEngine;
using ColossalFramework;
using System.Collections.Generic;
using CSLMusicMod.Helpers;

namespace CSLMusicMod
{
    /// <summary>
    /// Used for detouring methods from AudioManger. See Detours class for the detour code.
    /// </summary>
    public class CustomAudioManager
    {
        public CustomAudioManager()
        {
        }

        /// <summary>
        /// Allows custom playback of broadcasts.
        /// </summary>
        /// <param name="info">The content to be played</param>
        public void CustomQueueBroadcast(RadioContentInfo info)
        {
            if (!ModOptions.Instance.AllowContentBroadcast)
                return;

            var mgr = Singleton<AudioManager>.instance;

            var broadcastQueue = ReflectionHelper.GetPrivateField<FastList<RadioContentInfo>>(this, "m_broadcastQueue"); //Why does CO make everything private, so you can't access it ??

            while (!Monitor.TryEnter(broadcastQueue, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                if (broadcastQueue.m_size < 5)
                {
                    for (int i = 0; i < broadcastQueue.m_size; i++)
                    {
                        if (broadcastQueue.m_buffer[i] == info)
                        {
                            return;
                        }
                    }
                    broadcastQueue.Add(info);
                }
            }
            finally
            {
                Monitor.Exit(broadcastQueue);
            }
        }

        /// <summary>
        /// Allows restriction of content to specific songs
        /// </summary>
        /// <returns>The collect radio content info.</returns>
        /// <param name="type">Type.</param>
        /// <param name="channel">Channel.</param>
        public FastList<ushort> CustomCollectRadioContentInfo(RadioContentInfo.ContentType type, RadioChannelInfo channel)
        {
            var mgr = Singleton<AudioManager>.instance;
            //Debug.Log("[CSLMusic][Internal] Rebuilding the radio content of channel " + channel.GetLocalizedTitle());

            // CO makes some things public and other things private. This is completely insane.
            var m_tempRadioContentBuffer = ReflectionHelper.GetPrivateField<FastList<ushort>>(mgr, "m_tempRadioContentBuffer"); // This variable is being worked on
            var m_radioContentTable = ReflectionHelper.GetPrivateField<FastList<ushort>[]>(mgr, "m_radioContentTable");

            m_tempRadioContentBuffer.Clear(); 

            if (m_radioContentTable == null)
            {
                // Let's all sing the "Expensive Song!" ♬Expensive, Expensive♬ ♩OMG it's so expensive♩ (Rest of lyrics didn't load, yet)
				ReflectionHelper.InvokePrivateVoidMethod(mgr, "RefreshRadioContentTable");
                m_radioContentTable = ReflectionHelper.GetPrivateField<FastList<ushort>[]>(mgr, "m_radioContentTable");
            }

            // Get the allowed radio content
            HashSet<RadioContentInfo> disallowed_content = null;
            if(channel != null)
            {
                RadioContentWatcher.DisallowedContent.TryGetValue(channel, out disallowed_content);
            }

            //Debug.Log("[update]" + channel.GetLocalizedTitle() + " | " + allowed_content);
            /*if(allowed_content == null || allowed_content.Count == 0)
            {
                Debug.Log(channel.GetLocalizedTitle() + ": All content enabled!");
            }*/

            int prefabDataIndex = channel.m_prefabDataIndex;
            if (prefabDataIndex != -1)
            {
                int num = (int)(prefabDataIndex * 5 + type);
                if (num < m_radioContentTable.Length)
                {
                    FastList<ushort> fastList = m_radioContentTable[num];
                    if (fastList != null)
                    {
                        for (int i = 0; i < fastList.m_size; i++)
                        {
                            ushort num2 = fastList.m_buffer[i];
                            RadioContentInfo prefab = PrefabCollection<RadioContentInfo>.GetPrefab((uint)num2);

                            if (prefab != null && Singleton<UnlockManager>.instance.Unlocked(prefab.m_UnlockMilestone))
                            {
                                // Filter only content info that should be kept
                                if( disallowed_content == null || disallowed_content.Count ==  0 || !disallowed_content.Contains(prefab))
                                {
									prefab.m_cooldown = 1000000;
									m_tempRadioContentBuffer.Add(num2);
                                }
                            }
                        }
                    }
                }
            }

            for (int j = 0; j < mgr.m_radioContents.m_size; j++)
            {
                RadioContentData.Flags flags = mgr.m_radioContents.m_buffer[j].m_flags;
                if ((flags & RadioContentData.Flags.Created) != RadioContentData.Flags.None)
                {
                    RadioContentInfo info = mgr.m_radioContents.m_buffer[j].Info;
                    if (info != null)
                    {
                        info.m_cooldown = Mathf.Min(info.m_cooldown, (int)mgr.m_radioContents.m_buffer[j].m_cooldown);
                    }
                }
            }

            return m_tempRadioContentBuffer;

        }
    }
}

