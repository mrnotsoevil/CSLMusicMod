using System;
using System.Linq;
using ICities;
using UnityEngine;
using System.Collections.Generic;
using CSLMusicMod.UI;
using System.IO;
using ColossalFramework.IO;

namespace CSLMusicMod
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public static UserRadioCollection UserRadioContainer;
        public static ChannelInitializer StationContainer;
        public static ContentInitializer ContentContainer;
        public static Detours MethodDetours;
        public static MusicUI UI;
        public static RadioContentWatcher DisabledContentContainer;

        public LoadingExtension()
        {
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            if(!Directory.Exists("CSLMusicMod_Music"))
            {
                try
                {
                    Directory.CreateDirectory("CSLMusicMod_Music");
                }
                catch(Exception)
                {
                    
                }
            }

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
                RemoveUnsupportedContent();
                UserRadioContainer.CollectPostLoadingData();
                ExtendVanillaContent();
                DebugOutput();

                // Build UI and other post loadtime
                if (UI == null)
                {
                    UI = new GameObject("CSLMusicMod_UI").AddComponent<MusicUI>();
                }
                if (DisabledContentContainer == null)
                {
                    DisabledContentContainer = new GameObject("CSLMusicMod_DisabledContent").AddComponent<RadioContentWatcher>();
                }
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (UI != null)
            {
                UnityEngine.Object.Destroy(UI.gameObject);
                UI = null;
            }           
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
                UserRadioContainer = null;
            }
            if (DisabledContentContainer != null)
            {
                UnityEngine.Object.Destroy(DisabledContentContainer.gameObject);
                UserRadioContainer = null;
            }
        }

        private void RemoveUnsupportedContent()
        {
            // Apply filtering after loading
            for (uint i = 0; i < PrefabCollection<RadioChannelInfo>.PrefabCount(); ++i)
            {
                RadioChannelInfo info = PrefabCollection<RadioChannelInfo>.GetPrefab(i);
                RemoveUnsupportedContent(info);
            }
        }

        private void DebugOutput()
        {
            for (uint i = 0; i < PrefabCollection<RadioChannelInfo>.PrefabCount(); ++i)
            {
                String message = "";

                RadioChannelInfo info = PrefabCollection<RadioChannelInfo>.GetPrefab(i);

                message += "[CSLMusic][ChannelInfo] " + info + "\n";
                message += "Schedule:\n";

                foreach(RadioChannelInfo.State s in info.m_stateChain)
                {
                    message += "\t" + s.m_contentType + " " + s.m_minCount + " - " + s.m_maxCount + "\n";
                }

                message += "Content:\n";

                for (uint j = 0; j < PrefabCollection<RadioContentInfo>.PrefabCount(); ++j)
                {
                    RadioContentInfo content = PrefabCollection<RadioContentInfo>.GetPrefab(j);

                    if(content.m_radioChannels.Contains(info))
                        message += "\t[ContentInfo] " + content + " " + content.m_fileName + "\n";
                }

                Debug.Log(message);
            }

            for(uint i = 0; i < PrefabCollection<DisasterInfo>.PrefabCount(); ++i)
            {
                Debug.Log("[CSLMusic][DisasterContext] Disaster name: " + PrefabCollection<DisasterInfo>.GetPrefab(i).name);
            }
        }

        /// <summary>
        /// Removes disabled content from this channel.
        /// </summary>
        /// <param name="info">Info.</param>
        private void RemoveUnsupportedContent(RadioChannelInfo info)
        {
            if (info == null)
                return;

            Debug.Log("[CSLMusic] Removing unsupported content from " + info);

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

        /// <summary>
        /// Adds music files that are placed in the vanilla directories to the vanilla channels
        /// </summary>
        private void ExtendVanillaContent()
        {
            if (!ModOptions.Instance.EnableAddingContentToVanillaStations)
                return;

            for(uint i = 0; i < PrefabCollection<RadioChannelInfo>.PrefabCount(); ++i)
            {
                RadioChannelInfo info = PrefabCollection<RadioChannelInfo>.GetPrefab(i);

                if(!UserRadioContainer.m_UserRadioDict.ContainsKey(info))
                {
                    // Collect existing radio content
                    HashSet<string> existing = new HashSet<string>();

                    for(uint j = 0; j < PrefabCollection<RadioContentInfo>.PrefabCount(); ++j)
                    {
                        RadioContentInfo content = PrefabCollection<RadioContentInfo>.GetPrefab(j);

                        if(content.m_radioChannels.Contains(info))
                        {
                            string text = Path.Combine(DataLocation.gameContentPath, "Radio");
                            text = Path.Combine(text, content.m_contentType.ToString());
                            text = Path.Combine(text, content.m_folderName);
                            text = Path.Combine(text, content.m_fileName);

                            existing.Add(text);
                        }
                    }

                    // Check our collection for non-existing files
                    foreach(UserRadioContent usercontent in UserRadioContainer.m_Songs.Values)
                    {
                        if(!existing.Contains(usercontent.m_FileName) && usercontent.m_Collection == info.name)
                        {
                            Debug.Log("[CSLMusic][ExtendedVanillaContent] Adding " + usercontent.m_FileName + " to vanilla station " + info.name);
                            List<RadioChannelInfo> v = new List<RadioChannelInfo>(usercontent.m_VanillaContentInfo.m_radioChannels);
                            v.Add(info);
                            usercontent.m_VanillaContentInfo.m_radioChannels = v.ToArray();
                        }
                    }
                }
            }
        }
    }
}

