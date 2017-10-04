using System;
using System.Linq;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;
using System.IO;
using ColossalFramework.IO;
using CSLMusicMod.LitJson;

namespace CSLMusicMod
{
    /// <summary>
    /// Class that contains a custom radio channel.
    /// </summary>
    public class UserRadioChannel
    {
        public String m_Name;

        public HashSet<String> m_Collections;

        public List<UserRadioContent> m_Content;

        public List<RadioContext> m_Contexts = new List<RadioContext>();

        public RadioChannelInfo.State[] m_StateChain;

        public RadioContentInfo.ContentType[] m_SupportedContent = (RadioContentInfo.ContentType[])Enum.GetValues(typeof(RadioContentInfo.ContentType));

        public String m_ThumbnailFile;

        public String m_DefinitionDirectory = DataLocation.gameContentPath;

        // Post-launch
        public RadioChannelInfo m_VanillaChannelInfo;

        public UserRadioChannel()
        {
        }

        public UserRadioChannel(String name)
        {
            m_Name = name;
            m_Collections = new HashSet<string>() { name };
        }

        public UITextureAtlas GetThumbnailAtlas(Material baseMaterial)
        {
            String filename;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            if(m_ThumbnailFile != null && File.Exists(m_ThumbnailFile))
            {
                filename = m_ThumbnailFile;
            }
            else if(m_ThumbnailFile != null && assembly.GetManifestResourceNames().Contains("CSLMusicMod." + m_ThumbnailFile))
            {
                filename = m_ThumbnailFile;
            }
            else
            {
                filename = "thumbnail.png";
            }

            return TextureHelper.CreateAtlas(filename,
                "CSLMusicMod_Station_Thumbnail " + m_Name,
                baseMaterial,
                64,
                64,
                new string[] { "thumbnail" });
        }

        /// <summary>
        /// Returns the list of collections that should be active.
        /// </summary>
        /// <returns>The applying content collections.</returns>
        public HashSet<String> GetApplyingContentCollections()
        {
            // If we have no contexts, all collections can be used.
            if(m_Contexts.Count == 0)
            {
                return m_Collections;
            }

            HashSet<String> collections = new HashSet<string>();

            foreach(RadioContext context in m_Contexts)
            {
                if(context.Applies())
                {
                    collections.UnionWith(context.m_Collections);
                }
            }

            return collections;
        }

        public bool IsValid()
        {
            return m_Content != null && m_Content.Count != 0 &&
                m_StateChain != null && m_StateChain.Length != 0;
        }

        public static UserRadioChannel LoadFromJson(String filename)
        {
            try
            {
                string data = File.ReadAllText(filename);
                JsonData json = JsonMapper.ToObject(data);

                UserRadioChannel channel = new UserRadioChannel();
                channel.m_Name = (String)json["name"];

                if(json.Keys.Contains("collections"))
                {
                    List<String> collections = new List<string>();

                    foreach(JsonData v in json["collections"])
                    {
                        collections.Add((String)v);
                    }

                    channel.m_Collections = new HashSet<string>(collections);
                }
                else
                {
                    channel.m_Collections = new HashSet<string>(new string[] { channel.m_Name });
                }

                if(json.Keys.Contains("thumbnail"))
                {
                    channel.m_ThumbnailFile = Path.Combine(Path.GetDirectoryName(filename), (String)json["thumbnail"]);
                }

                if(json.Keys.Contains("schedule"))
                {
                    List<RadioChannelInfo.State> states = new List<RadioChannelInfo.State>();

                    foreach(JsonData entry in json["schedule"])
                    {
                        RadioChannelInfo.State state = new RadioChannelInfo.State();
                        state.m_contentType = (RadioContentInfo.ContentType)Enum.Parse(typeof(RadioContentInfo.ContentType), (String)entry["type"], true);
                        state.m_minCount = (int)entry["min"];
                        state.m_maxCount = (int)entry["max"];

                        states.Add(state);
                    }

                    channel.m_StateChain = states.ToArray();
                }
                
                // Named conditions
                var namedConditions = new Dictionary<string, RadioContextCondition>();
                if (json.Keys.Contains("filters") && json["filters"].IsObject)
                {
                    foreach (var name in json["filters"].Keys)
                    {
                        var entry = json["filters"][name];
                        var cond = RadioContextCondition.LoadFromJsonUsingType(entry);

                        if (cond != null)
                        {
                            namedConditions[name] = cond;
                        }
                    }
                }

                if(json.Keys.Contains("contexts"))
                {
                    foreach(JsonData entry in json["contexts"])
                    {
                        var context = RadioContext.LoadFromJson(entry, namedConditions);

                        if(context != null)
                        {
                            channel.m_Contexts.Add(context);

                            // Auto-load collections that are defined in contexts
                            foreach(var coll in context.m_Collections)
                            {
                                channel.m_Collections.Add(coll);
                            }
                        }
                    }
                }

                return channel;
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }
    }
}

