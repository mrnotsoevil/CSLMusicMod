using System;
using ICities;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Reflection;

namespace CSLMusicMod
{
    public class CSLMusicMod : IUserMod
    {
        public static System.Random RANDOM = new System.Random();

        public const String VersionName = "Rewrite 1.1.1.0";

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

        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UI.SettingsUI.InitializeSettingsUI(helper);
        }
    }
}

