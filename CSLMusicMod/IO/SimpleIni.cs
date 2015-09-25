using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CSLMusicMod.IO
{
    /**
     * 
     * I don't trust .NET serializer classes,
     * so I write my own!
     * 
     * Simple without support for special characters
     * use with caution!
     * 
     * */
    public class SimpleIni
    {
        public const String GROUP_NO_GROUP = "Default";

        public bool FoundNonExistingKeys { get; private set; }

        public String Filename { get; private set; }

        public List<string> Groups
        {
            get
            {
                return new List<String>(_data.Keys);
            }
        }

        private Dictionary<string, Dictionary<string, string>> _data;

        public SimpleIni(String filename)
        {
            _data = new Dictionary<string, Dictionary<string,string>>();
            Filename = filename;
        }

        public List<string> Keys(string group)
        {
            if (_data.ContainsKey(group))
                return new List<string>(_data[group].Keys);
            return new List<string>();
        }

        public String Get(String group, String key, String defaultvalue)
        {
            if (group == null || key == null)
                throw new ArgumentNullException("Group or key cannot be null!");

            if (_data.ContainsKey(group))
            {
                if (_data[group].ContainsKey(key))
                {
                    return _data[group][key];
                }
            }

            FoundNonExistingKeys = true;

            return defaultvalue;
        }

        public bool GetAsBool(String group, String key, bool defaultvalue)
        {
            bool res;
            if (bool.TryParse(Get(group, key, defaultvalue.ToString()), out res))
                return res;
            return defaultvalue;
        }

        public int GetAsInt(String group, String key, int defaultvalue)
        {
            int res;
            if (int.TryParse(Get(group, key, defaultvalue.ToString()), out res))
                return res;
            return defaultvalue;
        }

        public float GetAsFloat(String group, String key, float defaultvalue)
        {
            float res;
            if (float.TryParse(Get(group, key, defaultvalue.ToString()), out res))
                return res;
            return defaultvalue;
        }

        public KeyCode GetAsKeyCode(String group, String key, KeyCode defaultvalue)
        {
            try
            {
                return (KeyCode)Enum.Parse(typeof(KeyCode), Get(group, key, defaultvalue.ToString()), true);
            }
            catch (Exception)
            {           
                return defaultvalue;
            }
        }

        public void Set(String group, String key, object value)
        {
            if (group == null || key == null)
                throw new ArgumentNullException("Group or key cannot be null!");
            if (key.Contains("="))
                throw new ArgumentException("Key cannot contain '='");

            if (_data.ContainsKey(group))
            {
                _data[group][key] = (value != null ? value.ToString() : "");
            }
            else
            {
                Dictionary<string, string> nd = new Dictionary<string, string>();
                nd.Add(key, (value != null ? value.ToString() : ""));

                _data.Add(group, nd);
            }
        }

        public void Load()
        {
            _data.Clear();
            FoundNonExistingKeys = false;

            try
            {
                using (StreamReader w = new StreamReader(new FileStream(Filename, FileMode.Open)))
                {
                    String line;
                    String group = GROUP_NO_GROUP;

                    while ((line = w.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (String.IsNullOrEmpty(line) || line.StartsWith("#"))
                        {
                            continue;
                        }

                        //Group defintion found
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            group = line.Substring(1, line.Length - 2);

                            if (!_data.ContainsKey(group))
                            {
                                _data.Add(group, new Dictionary<string, string>());
                            }
                        }
                        //Key/Value pair found
                        else if (line.Contains("="))
                        {
                            int split_pos = line.IndexOf('=');
                            String key = line.Substring(0, split_pos);
                            String value = split_pos == line.Length - 1 ? "" : line.Substring(split_pos + 1);

                            _data[group][key] = value;
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Debug.LogError("[SimpleIni] Error while loading ini: " + ex.Message);
            }
        }

        public void Save()
        {
            try
            {
                using (StreamWriter w = new StreamWriter(new FileStream(Filename, FileMode.Create)))
                {
                    foreach (String group in Groups)
                    {
                        if (group != null)
                        {
                            w.WriteLine("[" + group + "]");
                        }

                        foreach (String key in Keys(group))
                        {
                            w.WriteLine(key + "=" + Get(group, key, ""));
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Debug.LogError("[SimpleIni] Error while saving ini: " + ex.Message);
            }
        }
    }
}

