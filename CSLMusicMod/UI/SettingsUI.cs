using System;
using System.Linq;
using UnityEngine;
using ICities;
using System.Collections.Generic;

namespace CSLMusicMod.UI
{
    public static class SettingsUI
    {
        private static List<String> KeyStringList = Enum.GetNames(typeof(KeyCode)).ToList();
        private static List<KeyCode> KeyList = new List<KeyCode>((KeyCode[])Enum.GetValues(typeof(KeyCode)));

        public static void InitializeSettingsUI(UIHelperBase ui)
        {
            UIHelperBase group = ui.AddGroup("CSL Music Mod " + CSLMusicMod.VersionName);

            Debug.Log("[CSLMusicMod] Populating settings menu ...");          

            ModOptions options = ModOptions.Instance;

            {
                var subgroup = group.AddGroup("User Interface");
                subgroup.AddCheckbox("Enable playlist", 
                    options.EnableCustomUI, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.EnableCustomUI = isChecked;
                        }));               
            }
            {
                var subgroup = group.AddGroup("Keyboard shortcuts");
                subgroup.AddDropdown("Open playlist and settings", KeyStringList.ToArray(), KeyStringList.IndexOf(ModOptions.Instance.KeyOpenMusicPanel.ToString()), (selection) =>
                    {
                        ModOptions.Instance.KeyOpenMusicPanel = KeyList[selection];
                    });
                subgroup.AddDropdown("Next song", KeyStringList.ToArray(), KeyStringList.IndexOf(ModOptions.Instance.KeyNextTrack.ToString()), (selection) =>
                    {
                        ModOptions.Instance.KeyNextTrack = KeyList[selection];
                    });
            }

            group.AddGroup("Note: Following settings may only take effect after reloading the map:");
           
            {
                var subgroup = group.AddGroup("Music packs");
                subgroup.AddCheckbox("Use music from music packs", 
                    options.EnableMusicPacks, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.EnableMusicPacks = isChecked;
                        }));
                subgroup.AddCheckbox("Create channels from unused music files", 
                    options.CreateChannelsFromLegacyPacks, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.CreateChannelsFromLegacyPacks = isChecked;
                        }));
            }
            {
                var subgroup = group.AddGroup("Radio content");
                subgroup.AddCheckbox("Enable music", 
                    options.AllowContentMusic, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentMusic = isChecked;
                        }));
                subgroup.AddCheckbox("Enable blurbs", 
                    options.AllowContentBlurb, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentBlurb = isChecked;
                        }));
                subgroup.AddCheckbox("Enable talks", 
                    options.AllowContentTalk, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentTalk = isChecked;
                        }));
                subgroup.AddCheckbox("Enable commercials", 
                    options.AllowContentCommercial, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentCommercial = isChecked;
                        }));
                subgroup.AddCheckbox("Enable broadcasts", 
                    options.AllowContentBroadcast, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentBroadcast = isChecked;
                        }));
                subgroup.AddCheckbox("Content can be disabled (May affect performance)", 
                    options.EnableDisabledContent, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.EnableDisabledContent = isChecked;
                        }));
                subgroup.AddCheckbox("Context-sensitive content (May affect performance)", 
                    options.EnableContextSensitivity, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.EnableContextSensitivity = isChecked;
                        }));
                subgroup.AddCheckbox("Extend vanilla stations with custom content", 
                    options.EnableAddingContentToVanillaStations, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.EnableAddingContentToVanillaStations = isChecked;
                        }));
                subgroup.AddButton("Reset disabled entries", new OnButtonClicked(() =>
                    {
                        options.DisabledContent.Clear();
                        options.SaveSettings();
                    }));
            }
            {
                var subgroup = group.AddGroup("Channel with all content (CSLMusic Mix)");
                subgroup.AddCheckbox("Create this channel", 
                    options.CreateMixChannels, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.CreateMixChannels = isChecked;
                        }));
                subgroup.AddCheckbox("Include music", 
                    options.MixContentMusic, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.MixContentMusic = isChecked;
                        }));
                subgroup.AddCheckbox("Include blurbs", 
                    options.MixContentBlurb, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.MixContentBlurb = isChecked;
                        }));
                subgroup.AddCheckbox("Include talks", 
                    options.MixContentTalk, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.MixContentTalk = isChecked;
                        }));
                subgroup.AddCheckbox("Include commercials", 
                    options.MixContentCommercial, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.MixContentCommercial = isChecked;
                        }));
                subgroup.AddCheckbox("Include broadcasts", 
                    options.MixContentBroadcast, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.MixContentBroadcast = isChecked;
                        }));
            }        
        }
    }
}

