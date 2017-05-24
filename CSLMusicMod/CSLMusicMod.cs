using System;
using ICities;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Reflection;
using CSLMusicMod.UI;

namespace CSLMusicMod
{
    /// <summary>
    /// Main mod class
    /// </summary>
    public class CSLMusicMod : IUserMod
    {
        public static System.Random RANDOM = new System.Random();

        public const String VersionName = "Rewrite 1.1.4.4";

        private SettingsUI m_SettingsUI;

        public string Name
        {
            get
            {
                return "CSL Music Mod";
            }
        }

        public string Description
        {
            get
            {
                return "Adds custom radio stations the game";
            }
        }

        public CSLMusicMod()
        {
            Debug.Log("[CSLMusic] Version " + VersionName);
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            m_SettingsUI = new SettingsUI();
            m_SettingsUI.InitializeSettingsUI(helper);
        }
    }
}

