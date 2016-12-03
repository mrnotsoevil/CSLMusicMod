using System;
using UnityEngine;
using ColossalFramework;

namespace CSLMusicMod
{
    /// <summary>
    /// Redirects RadioChannelData. Somehow. Works for some reason. Don't ask me why.
    /// </summary>
    public struct CustomRadioChannelData
    {
        public RadioChannelData.Flags m_flags;

        public ushort m_infoIndex;

        public ushort m_currentContent;

        public ushort m_nextContent;

        public ushort m_playPosition;

        public byte m_stateChainIndex;

        public byte m_stateRepeatCount;

        public RadioChannelInfo Info
        {
            get
            {
                return PrefabCollection<RadioChannelInfo>.GetPrefab((uint)this.m_infoIndex);
            }
            set
            {
                this.m_infoIndex = (ushort)Mathf.Clamp(value.m_prefabDataIndex, 0, 65535);
            }
        }

        public void SimulationTick(ushort channelIndex)
        {
            if (this.m_currentContent != 0)
            {
                this.m_playPosition = (ushort)Mathf.Min((int)(this.m_playPosition + 1), 65535);
                int num = (int)(this.m_playPosition * 256 / 60);
                AudioManager instance = Singleton<AudioManager>.instance;
                RadioContentInfo info = instance.m_radioContents.m_buffer[(int)this.m_currentContent].Info;
                if (num >= info.m_lengthSeconds && (this.m_flags & RadioChannelData.Flags.Active) == RadioChannelData.Flags.None)
                {
                    this.ChangeContent(channelIndex);
                }
            }
            else
            {
                this.ChangeContent(channelIndex);
            }
            this.UpdateNext(channelIndex);
        }

        public void ChangeContent(ushort channelIndex)
        {
            AudioManager instance = Singleton<AudioManager>.instance;
            if (this.m_currentContent != 0)
            {
                RadioContentData[] expr_27_cp_0 = instance.m_radioContents.m_buffer;
                ushort expr_27_cp_1 = this.m_currentContent;
                expr_27_cp_0[(int)expr_27_cp_1].m_flags = (expr_27_cp_0[(int)expr_27_cp_1].m_flags & ~RadioContentData.Flags.Playing);
                this.m_currentContent = 0;
            }
            if ((this.m_flags & RadioChannelData.Flags.PlayDefault) == RadioChannelData.Flags.None)
            {
                if (this.m_nextContent != 0)
                {
                    this.m_currentContent = this.m_nextContent;
                    this.m_nextContent = 0;
                    this.m_playPosition = 0;
                    RadioContentData[] expr_84_cp_0 = instance.m_radioContents.m_buffer;
                    ushort expr_84_cp_1 = this.m_currentContent;
                    expr_84_cp_0[(int)expr_84_cp_1].m_flags = (expr_84_cp_0[(int)expr_84_cp_1].m_flags & ~RadioContentData.Flags.Queued);
                    RadioContentData[] expr_A8_cp_0 = instance.m_radioContents.m_buffer;
                    ushort expr_A8_cp_1 = this.m_currentContent;
                    expr_A8_cp_0[(int)expr_A8_cp_1].m_flags = (expr_A8_cp_0[(int)expr_A8_cp_1].m_flags | RadioContentData.Flags.Playing);
                }
                else
                {
                    RadioContentInfo radioContentInfo = this.FindNextContentInfo();
                    if (radioContentInfo != null)
                    {
                        if (instance.CreateRadioContent(out this.m_currentContent, radioContentInfo))
                        {
                            this.m_playPosition = 0;
                            RadioContentData[] expr_F6_cp_0 = instance.m_radioContents.m_buffer;
                            ushort expr_F6_cp_1 = this.m_currentContent;
                            expr_F6_cp_0[(int)expr_F6_cp_1].m_flags = (expr_F6_cp_0[(int)expr_F6_cp_1].m_flags | RadioContentData.Flags.Playing);
                        }
                    }
                    else
                    {
                        this.m_flags |= RadioChannelData.Flags.PlayDefault;
                    }
                }
            }
            this.UpdateNext(channelIndex);
        }

        private void UpdateNext(ushort channelIndex)
        {
            if ((this.m_flags & RadioChannelData.Flags.PlayDefault) == RadioChannelData.Flags.None && this.m_nextContent == 0)
            {
                AudioManager instance = Singleton<AudioManager>.instance;
                RadioContentInfo radioContentInfo = this.FindNextContentInfo();
                if (radioContentInfo != null)
                {
                    if (instance.CreateRadioContent(out this.m_nextContent, radioContentInfo))
                    {
                        RadioContentData[] expr_53_cp_0 = instance.m_radioContents.m_buffer;
                        ushort expr_53_cp_1 = this.m_nextContent;
                        expr_53_cp_0[(int)expr_53_cp_1].m_flags = (expr_53_cp_0[(int)expr_53_cp_1].m_flags | RadioContentData.Flags.Queued);
                    }
                }
                else
                {
                    this.m_flags |= RadioChannelData.Flags.PlayDefault;
                }
            }
        }

        private void Initialize()
        {
            if ((this.m_flags & RadioChannelData.Flags.Initialized) == RadioChannelData.Flags.None)
            {
                RadioChannelInfo info = this.Info;
                if (info.m_stateChain != null && info.m_stateChain.Length != 0)
                {
                    this.m_stateChainIndex = (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)info.m_stateChain.Length);
                    int num = 1;
                    uint num2 = 0u;
                    while ((ulong)num2 < (ulong)((long)info.m_stateChain.Length))
                    {
                        if (num2 != 0u && (int)(this.m_stateChainIndex += 1) >= info.m_stateChain.Length)
                        {
                            this.m_stateChainIndex = 0;
                        }
                        int min = Mathf.Clamp(info.m_stateChain[(int)this.m_stateChainIndex].m_minCount, 1, 256);
                        int max = Mathf.Clamp(info.m_stateChain[(int)this.m_stateChainIndex].m_maxCount, min, 256);
                        num = Singleton<SimulationManager>.instance.m_randomizer.Int32(min, max);
                        if (num > 0)
                        {
                            break;
                        }
                        num2 += 1u;
                    }
                    this.m_stateRepeatCount = (byte)(num - 1);
                    this.m_stateRepeatCount = (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32(0, (int)this.m_stateRepeatCount);
                }
                this.m_flags |= RadioChannelData.Flags.Initialized;
            }
        }

        private RadioContentInfo FindNextContentInfo()
        {
            this.Initialize();
            RadioChannelInfo info = this.Info;
            RadioContentInfo.ContentType type = RadioContentInfo.ContentType.Music;
            if (info.m_stateChain != null && info.m_stateChain.Length != 0)
            {
                if ((int)this.m_stateChainIndex >= info.m_stateChain.Length)
                {
                    this.m_stateChainIndex = 0;
                }
                if (this.m_stateRepeatCount != 0)
                {
                    this.m_stateRepeatCount -= 1;
                }
                else
                {
                    int num = 1;
                    uint num2 = 0u;
                    while ((ulong)num2 < (ulong)((long)info.m_stateChain.Length))
                    {
                        if ((int)(this.m_stateChainIndex += 1) >= info.m_stateChain.Length)
                        {
                            this.m_stateChainIndex = 0;
                        }
                        int min = Mathf.Clamp(info.m_stateChain[(int)this.m_stateChainIndex].m_minCount, 1, 256);
                        int max = Mathf.Clamp(info.m_stateChain[(int)this.m_stateChainIndex].m_maxCount, min, 256);
                        num = Singleton<SimulationManager>.instance.m_randomizer.Int32(min, max);
                        if (num > 0)
                        {
                            break;
                        }
                        num2 += 1u;
                    }
                    this.m_stateRepeatCount = (byte)(num - 1);
                }
                type = info.m_stateChain[(int)this.m_stateChainIndex].m_contentType;
            }
            FastList<ushort> fastList = Singleton<AudioManager>.instance.CollectRadioContentInfo(type, info);

            var modoptions = ModOptions.Instance;

            int num3 = 0;
            int num4 = 0;
            for (int i = 0; i < fastList.m_size; i++)
            {
                ushort index = fastList.m_buffer[i];
                int cooldown = PrefabCollection<RadioContentInfo>.GetPrefab((uint)index).m_cooldown;
                num3 += Mathf.Clamp(cooldown - 150, 0, 350);
                num4 += Mathf.Clamp(cooldown, 1, 500);
            }
            if (num3 != 0)
            {
                num3 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num3);
                for (int j = 0; j < fastList.m_size; j++)
                {
                    ushort index2 = fastList.m_buffer[j];
                    RadioContentInfo prefab = PrefabCollection<RadioContentInfo>.GetPrefab((uint)index2);

                    string id = prefab.m_folderName + "/" + prefab.m_fileName;
                    if (modoptions.DisabledContent.Contains(id))
                    {                      
                        continue;
                    }
                        

                    int num5 = Mathf.Clamp(prefab.m_cooldown - 150, 0, 350);
                    if (num3 < num5)
                    {
                        return prefab;
                    }
                    num3 -= num5;
                }
            }
            else if (num4 != 0)
            {
                num4 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num4);
                for (int k = 0; k < fastList.m_size; k++)
                {
                    ushort index3 = fastList.m_buffer[k];
                    RadioContentInfo prefab2 = PrefabCollection<RadioContentInfo>.GetPrefab((uint)index3);

                    string id = prefab2.m_folderName + "/" + prefab2.m_fileName;
                    if (modoptions.DisabledContent.Contains(id))
                        continue;

                    int num6 = Mathf.Clamp(prefab2.m_cooldown, 1, 500);
                    if (num4 < num6)
                    {
                        return prefab2;
                    }
                    num4 -= num6;
                }
            }
            return null;
        }
    }
}

