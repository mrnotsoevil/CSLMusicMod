using System;
using ICities;
using UnityEngine;
using System.Collections.Generic;

namespace CSLMusicMod
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public static UserRadioCollection UserRadioContainer;
        public static ChannelInitializer StationContainer;
        public static ContentInitializer ContentContainer;
        public static Detours MethodDetours;

        public LoadingExtension()
        {
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            if (UserRadioContainer == null)
            {
                UserRadioContainer = new GameObject("CSLMusicMod_Definitions").AddComponent<UserRadioCollection>();
            }
            if (StationContainer == null)
            {
                StationContainer = new GameObject("CSLMusicMod_Stations").AddComponent<ChannelInitializer>();
            }
            if (ContentContainer == null)
            {
                ContentContainer = new GameObject("CSLMusicMod_Content").AddComponent<ContentInitializer>();
            }
            if (MethodDetours == null)
            {
                MethodDetours = new GameObject("CSLMusicMod_Detours").AddComponent<Detours>();
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            if(mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                // Apply filtering after loading
                for (uint i = 0; i < PrefabCollection<RadioChannelInfo>.PrefabCount(); ++i)
                {
                    RadioChannelInfo info = PrefabCollection<RadioChannelInfo>.GetPrefab(i);
                    RemoveUnsupportedContent(info);
                }

                DebugOutput();
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (StationContainer != null)
            {
                UnityEngine.Object.Destroy(StationContainer.gameObject);
                StationContainer = null;
            }
            if (ContentContainer != null)
            {
                UnityEngine.Object.Destroy(ContentContainer.gameObject);
                ContentContainer = null;
            }
            if (MethodDetours != null)
            {
                UnityEngine.Object.Destroy(MethodDetours.gameObject);
                MethodDetours = null;
            }
            if (UserRadioContainer != null)
            {
                UnityEngine.Object.Destroy(UserRadioContainer.gameObject);
                MethodDetours = null;
            }

        }

        private void DebugOutput()
        {
            for (uint i = 0; i < PrefabCollection<RadioChannelInfo>.PrefabCount(); ++i)
            {
                RadioChannelInfo info = PrefabCollection<RadioChannelInfo>.GetPrefab(i);

                Debug.Log("[CSLMusic-ChannelInfo] " + info.ToString() + " | " + info.m_hasChannelData);
                Debug.Log(info.m_prefabDataIndex + " | " + info.m_prefabInitialized);

                foreach(RadioChannelInfo.State s in info.m_stateChain)
                {
                    Debug.Log("-> " + s.m_contentType + " | " + s.m_minCount + " | " + s.m_maxCount);
                }
            }

            for (uint i = 0; i < PrefabCollection<RadioContentInfo>.PrefabCount(); ++i)
            {
                RadioContentInfo info = PrefabCollection<RadioContentInfo>.GetPrefab(i);

                Debug.Log("[CSLMusic-ContentInfo] " + info.ToString() + ", " + info.m_contentType + ", " + info.m_folderName + ", " + info.m_fileName);
                Debug.Log(info.m_prefabDataIndex + " | " + info.m_prefabInitialized);
                Debug.Log(info.m_cooldown + " | " + info.m_lengthSeconds);

                foreach(RadioChannelInfo rc in info.m_radioChannels)
                {
                    Debug.Log("# ->" + rc);
                }
            }
        }

        /// <summary>
        /// Removes disabled content from this channel.
        /// </summary>
        /// <param name="info">Info.</param>
        private void RemoveUnsupportedContent(RadioChannelInfo info)
        {
            var options = ModOptions.Instance;

            List<RadioChannelInfo.State> states = new List<RadioChannelInfo.State>(info.m_stateChain);
            states.RemoveAll((RadioChannelInfo.State obj) =>
                {
                    switch(obj.m_contentType)
                    {
                        case RadioContentInfo.ContentType.Blurb:
                            if(!options.AllowContentBlurb)
                            {
                                return true;
                            }
                            break;
                        case RadioContentInfo.ContentType.Broadcast:
                            if(!options.AllowContentBroadcast)
                            {
                                return true;
                            }
                            break;
                        case RadioContentInfo.ContentType.Commercial:
                            if(!options.AllowContentCommercial)
                            {
                                return true;
                            }
                            break;
                        case RadioContentInfo.ContentType.Music:
                            if(!options.AllowContentMusic)
                            {
                                return true;
                            }
                            break;
                        case RadioContentInfo.ContentType.Talk:
                            if(!options.AllowContentTalk)
                            {
                                return true;
                            }
                            break;
                    }
                    return false;
                });

            info.m_stateChain = states.ToArray();
        }
    }
}

