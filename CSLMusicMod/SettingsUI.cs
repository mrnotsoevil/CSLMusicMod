using System;
using UnityEngine;
using ICities;
using System.Collections.Generic;

namespace CSLMusicMod
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
            group.AddCheckbox("Music while loading", ModOptions.MusicWhileLoading, new OnCheckChanged((isChecked) =>
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
        }
    }
}

