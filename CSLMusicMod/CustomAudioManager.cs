using System;
using System.Threading;
using UnityEngine;
using ColossalFramework;
using System.Collections.Generic;

namespace CSLMusicMod
{
    /// <summary>
    /// Used for detouring methods from AudioManger. See Detours class for the detour code.
    /// </summary>
    public class CustomAudioManager : AudioManager
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
            //Debug.Log("[CSLMusic][Internal] Rebuilding the radio content of channel " + channel.GetLocalizedTitle());

            // CO makes some things public and other things private. This is completely insane.
            var m_tempRadioContentBuffer = new FastList<ushort>(); // This variable is being worked on
            var m_radioContentTable = ReflectionHelper.GetPrivateField<FastList<ushort>[]>(this, "m_radioContentTable");

            //m_tempRadioContentBuffer.Clear(); // Not needed. We make a new one.

            if (m_radioContentTable == null)
            {
                // Let's all sing the "Expensive Song!" ♬Expensive, Expensive♬ ♩OMG it's so expensive♩ (Rest of lyrics didn't load, yet)
				ReflectionHelper.InvokePrivateVoidMethod(this, "RefreshRadioContentTable");
                m_radioContentTable = ReflectionHelper.GetPrivateField<FastList<ushort>[]>(this, "m_radioContentTable");
            }

            // Get the allowed radio content
            HashSet<RadioContentInfo> allowed_content = null;
            if(channel != null)
            {
                RadioContentWatcher.AllowedContent.TryGetValue(channel, out allowed_content);
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
                                if(allowed_content == null || allowed_content.Count ==  0 || allowed_content.Contains(prefab))
                                {
									prefab.m_cooldown = 1000000;
									m_tempRadioContentBuffer.Add(num2);
                                }
                            }
                        }
                    }
                }
            }

            for (int j = 0; j < this.m_radioContents.m_size; j++)
            {
                RadioContentData.Flags flags = this.m_radioContents.m_buffer[j].m_flags;
                if ((flags & RadioContentData.Flags.Created) != RadioContentData.Flags.None)
                {
                    RadioContentInfo info = this.m_radioContents.m_buffer[j].Info;
                    if (info != null)
                    {
                        info.m_cooldown = Mathf.Min(info.m_cooldown, (int)this.m_radioContents.m_buffer[j].m_cooldown);
                    }
                }
            }

            // Set borrowed variables in the actual class instance.
            ReflectionHelper.SetPrivateField(this, "m_tempRadioContentBuffer", m_tempRadioContentBuffer);
            return m_tempRadioContentBuffer;

        }
    }
}

