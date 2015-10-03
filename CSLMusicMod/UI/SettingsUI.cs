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
        public CSLMusicMod Mod
        {
            get;
            set;
        }

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

        private List<String> KeyStringList = Enum.GetNames(typeof(KeyCode)).ToList();
        private List<KeyCode> KeyList;

        private List<String> SelectionAlgorithmStringList = Enum.GetNames(typeof(SettingsManager.Options.MusicSelectionType)).ToList();
        private List<SettingsManager.Options.MusicSelectionType> SelectionAlgorithmList;

        private Dictionary<int, MusicEntryTag> IndexTagMapping = new Dictionary<int, MusicEntryTag>();
        private Dictionary<MusicEntryTag, int> TagIndexMapping = new Dictionary<MusicEntryTag, int>();

        public SettingsUI()
        {
            KeyList = new List<KeyCode>((KeyCode[])Enum.GetValues(typeof(KeyCode)));
            SelectionAlgorithmList = new List<SettingsManager.Options.MusicSelectionType>((SettingsManager.Options.MusicSelectionType[])Enum.GetValues(typeof(SettingsManager.Options.MusicSelectionType)));
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void InitializeSettingsUI(UIHelperBase ui)
        {
            UIHelperBase group = ui.AddGroup("CSL Music Mod");

            Debug.Log("[CSLMusicMod] Populating settings menu ...");

            group.AddDropdown("Open playlist and settings", KeyStringList.ToArray(), KeyStringList.IndexOf(ModOptions.Key_Settings.ToString()), (selection) =>
                {
                    ModOptions.Key_Settings = KeyList[selection];
                    SettingsManager.SaveModSettings();
                });
            group.AddDropdown("Next song", KeyStringList.ToArray(), KeyStringList.IndexOf(ModOptions.Key_NextTrack.ToString()), (selection) =>
                {
                    ModOptions.Key_NextTrack = KeyList[selection];
                    SettingsManager.SaveModSettings();
                });
            group.AddSpace(10);
            group.AddCheckbox("Show toolbar button", ModOptions.ShowToolbarButton, new OnCheckChanged((isChecked) =>
                    {
                        ModOptions.ShowToolbarButton = isChecked;
                        SettingsManager.SaveModSettings();
                    }));
            group.AddCheckbox("Fixate toolbar button", ModOptions.FixateToolbarButton, new OnCheckChanged((isChecked) =>
                    {
                        ModOptions.FixateToolbarButton = isChecked;
                        SettingsManager.SaveModSettings();
                    }));
            group.AddButton("Reset toolbar button position", new OnButtonClicked(() =>
                    {
                        ModOptions.ToolbarButtonX = -1;
                        ModOptions.ToolbarButtonY = -1;
                        SettingsManager.SaveModSettings();
                    }));

            group.AddSpace(10);

            group.AddCheckbox("Large playlist", ModOptions.LargePlayList, new OnCheckChanged((isChecked) =>
                    {
                        ModOptions.LargePlayList = isChecked;
                        SettingsManager.SaveModSettings();

                        //Reload UI if possible
                        Mod.ReloadUI();
                    }));
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
            group.AddCheckbox("Direct playback of non-RAW files", ModOptions.PlayWithoutConvert, new OnCheckChanged((isChecked) =>
                    {
                        ModOptions.PlayWithoutConvert = isChecked;
                        SettingsManager.SaveModSettings();
                    }));
            group.AddSlider("non-RAW crossfade sample count difference (2^x)", 0, 20, 0.5f, (float)Math.Log(ModOptions.CrossfadeLimit, 2), new OnValueChanged((float val) =>
                    {
                        ModOptions.CrossfadeLimit = (int)(Math.Pow(2.0, val));
                        SettingsManager.SaveModSettings();
                    }));
            group.AddCheckbox("Force non-RAW crossfading", ModOptions.IgnoreCrossfadeLimit, new OnCheckChanged((isChecked) =>
                {
                    ModOptions.IgnoreCrossfadeLimit = isChecked;
                    SettingsManager.SaveModSettings();
                }));


            group.AddSpace(10);

            group.AddDropdown("Select music by ", new string[]
                {
                    "AND - all tags must apply",
                    "OR - at least one tag must apply"
                },
                SelectionAlgorithmStringList.IndexOf(ModOptions.MusicSelectionAlgorithm.ToString()), (selection) =>
                {
                    ModOptions.MusicSelectionAlgorithm = SelectionAlgorithmList[selection];
                    SettingsManager.SaveModSettings();
                });
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

