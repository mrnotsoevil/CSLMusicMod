using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using ColossalFramework.IO;
using System.IO;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using ICities;

namespace CSLMusicMod
{
    public class UserRadioCollection : MonoBehaviour
    {
        public Dictionary<String, UserRadioContent> m_Songs = new Dictionary<string, UserRadioContent>();
        public Dictionary<String, UserRadioChannel> m_Stations = new Dictionary<string, UserRadioChannel>();

        // Post-Launch variables
        public Dictionary<RadioChannelInfo, UserRadioChannel> m_UserRadioDict = new Dictionary<RadioChannelInfo, UserRadioChannel>();
        public Dictionary<RadioContentInfo, UserRadioContent> m_UserContentDict = new Dictionary<RadioContentInfo, UserRadioContent>();

        public UserRadioCollection()
        {
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);

            RefreshCollection();
        }

        private void RefreshCollection()
        {
            m_Songs.Clear();
            m_Stations.Clear();

            LoadSongs();
            LoadChannels();
            Postprocess();
        }

        private void LoadSongs()
        {           
            LoadVanillaSongs(RadioContentInfo.ContentType.Music);
            LoadVanillaSongs(RadioContentInfo.ContentType.Talk);
            LoadVanillaSongs(RadioContentInfo.ContentType.Blurb);
            LoadVanillaSongs(RadioContentInfo.ContentType.Commercial);
            LoadVanillaSongs(RadioContentInfo.ContentType.Broadcast);

            HashSet<String> visitedmoddirs = new HashSet<string>();

            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (info.isEnabled)
                {
                    String path = Path.Combine(info.modPath, "CSLMusicMod_Music");

                    if (Directory.Exists(path))
                    {
                        if (!visitedmoddirs.Contains(path))
                        {
                            IUserMod mod = (IUserMod)info.userModInstance;
                            LoadSongsFromCollection("MusicPack " + mod.Name, path);
                            visitedmoddirs.Add(path);
                        }
                    }
                }
            }

            LoadSongsFromCollection("Userdefined", Path.Combine(DataLocation.applicationBase, "CSLMusicMod_Music"));
        }

        private void LoadVanillaSongs(RadioContentInfo.ContentType type)
        {
            String path = Path.Combine(Path.Combine(DataLocation.gameContentPath, "Radio"), type.ToString());

            // The content determination algorithm will always return "Music". Set it manually.
            foreach(var content in LoadSongsFromCollection("Vanilla " + type.ToString() ,path))
            {
                content.m_ContentType = type;
            }
        }

        private  List<UserRadioContent> LoadSongsFromCollection(String collection, String dir)
        {
            List<UserRadioContent> result = new List<UserRadioContent>();
            Debug.Log("[CSLMusic] Looking for songs in " + dir);

            LoadSongsFromFolder(collection, dir);

            foreach(String d in Directory.GetDirectories(dir))
            {
                result.AddRange(LoadSongsFromFolder(Path.GetFileNameWithoutExtension(d), d));
            }

            return result;
        }

        private void LoadChannels()
        {
            if(ModOptions.Instance.CreateMixChannels)
            {
                CreateDefaultMixChannel();
            }

            if (ModOptions.Instance.EnableMusicPacks)
            {
                HashSet<String> visitedmoddirs = new HashSet<string>();

                foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
                {
                    if (info.isEnabled)
                    {
                        String path = Path.Combine(info.modPath, "CSLMusicMod_Music");

                        if (Directory.Exists(path))
                        {
                            if (!visitedmoddirs.Contains(path))
                            {
                                // If enabled, add default collection
                                IUserMod mod = (IUserMod)info.userModInstance;

                                if (ModOptions.Instance.CreateChannelsFromLegacyPacks)
                                {
                                    CreateLegacyChannel("MusicPack " + mod.Name, new string[] { "MusicPack " + mod.Name });
                                }

                                LoadChannelsFromCollection(path);
                                visitedmoddirs.Add(path);
                            }
                        }
                    }
                }
            }

            if (ModOptions.Instance.CreateChannelsFromLegacyPacks)
            {
                CreateLegacyChannel("Userdefined", new string[] { "Userdefined" });
            }

            LoadChannelsFromCollection(Path.Combine(DataLocation.applicationBase, "CSLMusicMod_Music"));
        }

        private void LoadChannelsFromCollection(String dir)
        {
            Debug.Log("[CSLMusic] Looking for channels in " + dir);         

            // Load json channel configuration
            foreach(String filename in Directory.GetFiles(dir))
            {
                if(Path.GetExtension(filename) == ".json")
                {
                    UserRadioChannel channel = UserRadioChannel.LoadFromJson(filename);

                    if(channel != null)
                    {
                        m_Stations[channel.m_Name] = channel;
                    }
                    else
                    {
                        Debug.Log("[CSLMusic] Error: Cannot load channel from " + filename);
                    }
                }
            }
        }

        private void CreateLegacyChannel(String name, String[] collections)
        {
            UserRadioChannel channel = new UserRadioChannel(name);
            channel.m_Collections = new HashSet<string>(collections);
            channel.m_ThumbnailFile = "thumbnail_package.png";
            m_Stations[channel.m_Name] = channel;
        }

        private void CreateDefaultMixChannel()
        {
            UserRadioChannel channel = new UserRadioChannel("CSLMusic Mix");
            channel.m_ThumbnailFile = "thumbnail_mix.png";
            channel.m_Collections = new HashSet<string>(m_Songs.Values.Select(song => song.m_Collection)); // Default channel loads from all collections

            List<RadioContentInfo.ContentType> allowedcontent = new List<RadioContentInfo.ContentType>();

            if (ModOptions.Instance.MixContentBlurb)
                allowedcontent.Add(RadioContentInfo.ContentType.Blurb);
            if (ModOptions.Instance.MixContentBroadcast)
                allowedcontent.Add(RadioContentInfo.ContentType.Broadcast);
            if (ModOptions.Instance.MixContentCommercial)
                allowedcontent.Add(RadioContentInfo.ContentType.Commercial);
            if (ModOptions.Instance.MixContentMusic)
                allowedcontent.Add(RadioContentInfo.ContentType.Music);
            if (ModOptions.Instance.MixContentTalk)
                allowedcontent.Add(RadioContentInfo.ContentType.Talk);

            channel.m_SupportedContent = allowedcontent.ToArray();

            if(allowedcontent.Count != 0)
                m_Stations[channel.m_Name] = channel;
        }
      
        private void Postprocess()
        {
            // Associate songs to channels
            foreach(UserRadioChannel channel in m_Stations.Values)
            {
                List<UserRadioContent> content = m_Songs.Values.Where(s => channel.m_Collections.Contains(s.m_Collection)).ToList();
                channel.m_Content = content;
            }

            // Auto-Build statechain if needed
            foreach(UserRadioChannel channel in m_Stations.Values)
            {
                if(channel.m_StateChain == null || channel.m_StateChain.Length == 0)
                {
                    channel.m_StateChain = AutoBuildStateChain(channel.m_Content, channel.m_SupportedContent);
                }
            }

            // Remove empty channels
            foreach(String key in m_Stations.Keys.ToList())
            {
                if(!m_Stations[key].IsValid())
                {
                    m_Stations.Remove(key);
                }
            }

            // Associate channels to songs
            foreach(UserRadioContent song in m_Songs.Values)
            {
                song.m_Channels = m_Stations.Values.Where(channel => channel.m_Content.Contains(song)).ToArray();
            }
        }

        /// <summary>
        /// Builds a random statechain from given content
        /// </summary>
        /// <returns>The build state chain.</returns>
        /// <param name="content">Content.</param>
        private RadioChannelInfo.State[] AutoBuildStateChain(List<UserRadioContent> content, RadioContentInfo.ContentType[] allowedcontent)
        {
            if (content == null || content.Count == 0)
                return null;

            content = new List<UserRadioContent>(content);

            List<RadioContentInfo.ContentType> availabletypes = content.Select(song => song.m_ContentType).Distinct().Where(c => allowedcontent.Contains(c)).ToList();

            /*if(availabletypes.Count == 1)
            {
                List<RadioChannelInfo.State> statechain = new List<RadioChannelInfo.State>();

                for(int i = 0; i < content.Count; ++i)
                {
                    statechain.Add(new RadioChannelInfo.State() { m_contentType = availabletypes[0], m_minCount = 0, m_maxCount = 1 });
                }

                return statechain.ToArray();
            }
            else*/
            {
                List<RadioChannelInfo.State> statechain = new List<RadioChannelInfo.State>();
                int rounds = CSLMusicMod.RANDOM.Next(1, 5);

                for(int i = 0; i < rounds; ++i)
                {
                    List<RadioContentInfo.ContentType> av = new List<RadioContentInfo.ContentType>(availabletypes);

                    while(av.Count != 0)
                    {
                        var type = av[CSLMusicMod.RANDOM.Next(0, av.Count)];

                        switch(type)
                        {
                            case RadioContentInfo.ContentType.Blurb:
                                statechain.Add(new RadioChannelInfo.State() { m_contentType = type, m_minCount = 0, m_maxCount = 1 });
                                break;
                            case RadioContentInfo.ContentType.Broadcast:
                                statechain.Add(new RadioChannelInfo.State() { m_contentType = type, m_minCount = 0, m_maxCount = 1 });
                                break;
                            case RadioContentInfo.ContentType.Commercial:
                                statechain.Add(new RadioChannelInfo.State() { m_contentType = type, m_minCount = 0, m_maxCount = 3 });
                                break;
                            case RadioContentInfo.ContentType.Music:
                                statechain.Add(new RadioChannelInfo.State() { m_contentType = type, m_minCount = 1, m_maxCount = 5 });
                                break;
                            case RadioContentInfo.ContentType.Talk:
                                statechain.Add(new RadioChannelInfo.State() { m_contentType = type, m_minCount = 0, m_maxCount = 1 });
                                break;
                        }

                        av.Remove(type);

                    }
                }

                return statechain.ToArray();
            }

        }

        private List<UserRadioContent> LoadSongsFromFolder(String collection, String folder)
        {
            List<UserRadioContent> result = new List<UserRadioContent>();
            Debug.Log("[CSLMusic] Loading content from " + folder);

            foreach(String filename in Directory.GetFiles(folder))
            {
                if(Path.GetExtension(filename) == ".ogg")
                {
                    UserRadioContent content = new UserRadioContent(collection, filename);
                    m_Songs[content.m_Name] = content;
                    Debug.Log("[CSLMusic] Found content " + content.m_Name + ", path: " + filename);

                    result.Add(content);
                }
            }

            return result;
        }

        /// <summary>
        /// Collects information that is available after loading of the game
        /// </summary>
        public void CollectPostLoadingData()
        {
            for(uint i = 0; i < PrefabCollection<RadioChannelInfo>.PrefabCount(); ++i)
            {
                RadioChannelInfo info = PrefabCollection<RadioChannelInfo>.GetPrefab(i);
                UserRadioChannel user;

                if(m_Stations.TryGetValue(info.name, out user))
                {
                    m_UserRadioDict[info] = user;
                    user.m_VanillaChannelInfo = info;
                }
            }
            for(uint i = 0; i < PrefabCollection<RadioContentInfo>.PrefabCount(); ++i)
            {
                RadioContentInfo info = PrefabCollection<RadioContentInfo>.GetPrefab(i);
                UserRadioContent user;

                if(m_Songs.TryGetValue(info.name, out user))
                {
                    m_UserContentDict[info] = user;
                    user.m_VanillaContentInfo = info;
                }
            }
        }
    }
}

