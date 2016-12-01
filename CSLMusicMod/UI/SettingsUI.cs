using System;
using System.Linq;
using UnityEngine;
using ICities;
using System.Collections.Generic;

namespace CSLMusicMod.UI
{
    public static class SettingsUI
    {
        public static void InitializeSettingsUI(UIHelperBase ui)
        {
            UIHelperBase group = ui.AddGroup("CSL Music Mod");

            Debug.Log("[CSLMusicMod] Populating settings menu ...");          

            ModOptions options = ModOptions.Instance;

            group.AddGroup("Note: Settings only take effect after reloading the map.");

            {
                var subgroup = group.AddGroup("Default channels");

                subgroup.AddCheckbox("Add channel with all content ('CSLMusic Mix')", 
                    options.CreateMixChannels, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                        options.CreateMixChannels = isChecked;
                        }));
                subgroup.AddCheckbox("Create channels from unused music files", 
                    options.CreateChannelsFromLegacyPacks, 
                    new OnCheckChanged((bool isChecked) =>
                    {
                        options.CreateChannelsFromLegacyPacks = isChecked;
                    }));
            }
            {
                var subgroup = group.AddGroup("Music packs");
                subgroup.AddCheckbox("Use music from music packs", 
                    options.EnableMusicPacks, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.EnableMusicPacks = isChecked;
                        }));
            }
            {
                var subgroup = group.AddGroup("Allowed content");
                subgroup.AddCheckbox("Music", 
                    options.AllowContentMusic, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentMusic = isChecked;
                        }));
                subgroup.AddCheckbox("Blurbs", 
                    options.AllowContentBlurb, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentBlurb = isChecked;
                        }));
                subgroup.AddCheckbox("Talks", 
                    options.AllowContentTalk, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentTalk = isChecked;
                        }));
                subgroup.AddCheckbox("Commercials", 
                    options.AllowContentCommercial, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentCommercial = isChecked;
                        }));
                subgroup.AddCheckbox("Broadcasts", 
                    options.AllowContentBroadcast, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.AllowContentBroadcast = isChecked;
                        }));
            }
        }
    }
}

