using System;
using System.Linq;
using UnityEngine;
using ICities;
using System.Collections.Generic;

namespace CSLMusicMod.UI
{
    /**
     * Controls the mod settings accessable by C:S settings UI API 
     * */
    public class SettingsUI : MonoBehaviour
    {
        public SettingsManager SettingsManager
        {
            get
            {
                return gameObject.GetComponent<SettingsManager>();
            }
        }

        public SettingsManager.Options ModOptions
        {
            get
            {
                return gameObject.GetComponent<SettingsManager>().ModOptions;
            }
        }

        private String[] KeyStringArray = Enum.GetNames(typeof(KeyCode));
        private List<String> KeyStringList;
        private List<KeyCode> KeyList;
        private Dictionary<int, MusicEntryTag> IndexTagMapping = new Dictionary<int, MusicEntryTag>();
        private Dictionary<MusicEntryTag, int> TagIndexMapping = new Dictionary<MusicEntryTag, int>();

        public SettingsUI()
        {
            KeyStringList = new List<String>(KeyStringArray);
            KeyList = new List<KeyCode>((KeyCode[])Enum.GetValues(typeof(KeyCode)));
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void InitializeSettingsUI(UIHelperBase ui)
        {
            UIHelperBase group = ui.AddGroup("CSL Music Mod");

            Debug.Log("[CSLMusicMod] Populating settings menu ...");

            group.AddDropdown("Open playlist and settings", KeyStringArray, KeyStringList.IndexOf(ModOptions.Key_Settings.ToString()), (selection) =>
                {
                    ModOptions.Key_Settings = KeyList[selection];
                    SettingsManager.SaveModSettings();
                });
            group.AddDropdown("Next song", KeyStringArray, KeyStringList.IndexOf(ModOptions.Key_NextTrack.ToString()), (selection) =>
                {
                    ModOptions.Key_NextTrack = KeyList[selection];
                    SettingsManager.SaveModSettings();
                });
            group.AddSpace(10);
            group.AddCheckbox("Music in menu & while loading", ModOptions.MusicWhileLoading, new OnCheckChanged((isChecked) =>
                    {
                        ModOptions.MusicWhileLoading = isChecked;
                        SettingsManager.SaveModSettings();
                    }));
            group.AddCheckbox("Enable music packs", ModOptions.EnableMusicPacks, new OnCheckChanged((isChecked) =>
                    {
                        ModOptions.EnableMusicPacks = isChecked;
                        SettingsManager.SaveModSettings();
                    }));
            group.AddCheckbox("Enable direct *.ogg playback", ModOptions.PlayWithoutConvert, new OnCheckChanged((isChecked) =>
                    {
                        ModOptions.PlayWithoutConvert = isChecked;
                        SettingsManager.SaveModSettings();
                    }));
            group.AddSpace(10);

            {
                var tagtypes = gameObject.GetComponent<MusicManager>().MusicTagTypes;
                var tagpriority = ModOptions.MusicTagTypePriority;
                var tagtypeslist = new List<String>();

                IndexTagMapping.Clear();
                TagIndexMapping.Clear();

                {
                    int tag_idx = 0;
                    foreach (var tag in tagtypes.Values)
                    {
                        tagtypeslist.Add(tag.Description);
                        IndexTagMapping.Add(tag_idx, tag);
                        TagIndexMapping.Add(tag, tag_idx);

                        tag_idx++;
                    }
                }

                // Tag priority entries
                group.AddDropdown("Tag priority 1", tagtypeslist.ToArray(), TagIndexMapping[tagtypes[tagpriority[0]]], new OnDropdownSelectionChanged((selection) =>
                        {
                            var tag = IndexTagMapping[selection];
                            ModOptions.MusicTagTypePriority[0] = tag.Name;
                            SettingsManager.SaveModSettings();
                        }));
                group.AddDropdown("Tag priority 2", tagtypeslist.ToArray(), TagIndexMapping[tagtypes[tagpriority[1]]], new OnDropdownSelectionChanged((selection) =>
                    {
                        var tag = IndexTagMapping[selection];
                        ModOptions.MusicTagTypePriority[1] = tag.Name;
                        SettingsManager.SaveModSettings();
                    }));
                group.AddDropdown("Tag priority 3", tagtypeslist.ToArray(), TagIndexMapping[tagtypes[tagpriority[2]]], new OnDropdownSelectionChanged((selection) =>
                    {
                        var tag = IndexTagMapping[selection];
                        ModOptions.MusicTagTypePriority[2] = tag.Name;
                        SettingsManager.SaveModSettings();
                    }));
                group.AddDropdown("Tag priority 4", tagtypeslist.ToArray(), TagIndexMapping[tagtypes[tagpriority[3]]], new OnDropdownSelectionChanged((selection) =>
                    {
                        var tag = IndexTagMapping[selection];
                        ModOptions.MusicTagTypePriority[3] = tag.Name;
                        SettingsManager.SaveModSettings();
                    }));
            }
        }
    }
}

