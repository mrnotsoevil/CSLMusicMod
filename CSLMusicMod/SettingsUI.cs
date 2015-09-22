using System;
using UnityEngine;
using ICities;

namespace CSLMusicMod
{
    /**
     * Controls the mod settings accessable by C:S settings UI API 
     * */
    public class SettingsUI : MonoBehaviour
    {
        public SettingsUI()
        {
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void InitializeSettingsUI(UIHelper ui)
        {
            UIHelperBase group = ui.AddGroup("CSL Music Mod");
        }
    }
}

