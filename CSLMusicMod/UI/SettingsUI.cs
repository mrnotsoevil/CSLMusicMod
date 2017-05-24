using System;
using System.Linq;
using UnityEngine;
using ICities;
using System.Collections.Generic;
using ColossalFramework.UI;
using System.IO;
using ColossalFramework.IO;

namespace CSLMusicMod.UI
{
    public class SettingsUI
    {
        private List<String> KeyStringList = Enum.GetNames(typeof(KeyCode)).ToList();
        private List<KeyCode> KeyList = new List<KeyCode>((KeyCode[])Enum.GetValues(typeof(KeyCode)));

        private UIScrollablePanel _optionsPanel;
        private UITabstrip m_TabStrip;
        private UITabContainer m_TabContainer;
        private UIButton m_TabTemplate;
        private int m_TabIndex = 0;

        public void InitializeSettingsUI(UIHelperBase helper)
        {
            InitializeTabStrip(helper);

            AddOptionsInfo(AddTab("Info"));
            AddOptionsChannels(AddTab("Channels"));
            AddOptionsChannelEditor(AddTab("Editor"));
            AddOptionsContent(AddTab("Content"));
            AddOptionsShortcuts(AddTab("Shortcuts"));
            AddOptionsUI(AddTab("User Interface"));
        }

        private void InitializeTabStrip(UIHelperBase helper)
        {            
            // https://github.com/Katalyst6/CSL.TransitAddonMod/blob/master/NetworkExtensions/Mod.Settings.cs
            m_TabTemplate = Resources
                .FindObjectsOfTypeAll<UITabstrip>()[0]
                .GetComponentInChildren<UIButton>();

            _optionsPanel = ((UIHelper)helper).self as UIScrollablePanel;
            _optionsPanel.autoLayout = false;

            m_TabStrip = _optionsPanel.AddUIComponent<UITabstrip>();
            m_TabStrip.relativePosition = new Vector3(0, 0);
            m_TabStrip.size = new Vector2(744, 40);

            m_TabContainer = _optionsPanel.AddUIComponent<UITabContainer>();
            m_TabContainer.relativePosition = new Vector3(0, 40);
            m_TabContainer.size = new Vector3(744, 713);
            m_TabStrip.tabPages = m_TabContainer;
        }

        private UIHelperBase AddTab(string name)
        {
            m_TabStrip.AddTab(name, m_TabTemplate, true);
            m_TabStrip.selectedIndex = m_TabIndex;

            // Get the current container and use the UIHelper to have something in there
            UIPanel stripRoot = m_TabStrip.tabContainer.components[m_TabIndex++] as UIPanel;
            stripRoot.autoLayout = true;
            stripRoot.autoLayoutDirection = LayoutDirection.Vertical;
            stripRoot.autoLayoutPadding.top = 5;
            stripRoot.autoLayoutPadding.left = 10;
            UIHelper stripHelper = new UIHelper(stripRoot);

            return stripHelper;
        }

        private void AddOptionsChannelEditor(UIHelperBase helper) 
        {
            
        }

        private void AddOptionsInfo(UIHelperBase helper)
        {
            helper.AddGroup("CSL Music Mod version " + CSLMusicMod.VersionName);

            {
                var subgroup = helper.AddGroup("Performance");
                subgroup.AddGroup("If you have performance problems, you can try\nto disable features marked with an asterisk (*).");
            }
            {
                var subgroup = helper.AddGroup("Channels & content");
                subgroup.AddGroup("You can add your own channels or music by installing\n" +
                    "music packs, putting station configurations or music\n" +
                    "files into the CSLMusicMod_Music folder or into the\n" +
                    "folder containing vanilla radio content.");
            }

        }

        private void AddOptionsUI(UIHelperBase helper)
        {
            ModOptions options = ModOptions.Instance;

            helper.AddCheckbox("Enable playlist*", 
                options.EnableCustomUI, 
                new OnCheckChanged((bool isChecked) =>
                    {
                        options.EnableCustomUI = isChecked;
                    }));               
        }

        private void AddOptionsContent(UIHelperBase helper)
        {
            ModOptions options = ModOptions.Instance;

            {
                var subgroup = helper.AddGroup("Additional features");

                subgroup.AddCheckbox("Content can be disabled* (Needs reload)", 
                    options.EnableDisabledContent, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.EnableDisabledContent = isChecked;
                        }));
                subgroup.AddCheckbox("Context-sensitive content* (Needs reload)", 
                    options.EnableContextSensitivity, 
                    new OnCheckChanged((bool isChecked) =>
                        {
                            options.EnableContextSensitivity = isChecked;
                        }));
                subgroup.AddCheckbox("Extend vanilla stations with custom content (Needs reload)", 
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
                var subgroup = helper.AddGroup("Radio station content (Needs reload)");
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
            }
        }

        private void AddOptionsChannels(UIHelperBase helper)
        {
            ModOptions options = ModOptions.Instance;

            {
                List<string> vanillastations = new List<string>();

                foreach(RadioContentInfo.ContentType type in Enum.GetValues(typeof(RadioContentInfo.ContentType)))
                {
                    // They are no real channels
                    if (type == RadioContentInfo.ContentType.Broadcast)
                        continue;

                    String path = Path.Combine(Path.Combine(DataLocation.gameContentPath, "Radio"), type.ToString());

                    foreach (String d in Directory.GetDirectories(path))
                    {
                        if(Directory.GetFiles(d).Length != 0)
                        {
                            String station = Path.GetFileNameWithoutExtension(d);

                            if(!vanillastations.Contains(station))
                            {
                                vanillastations.Add(station);
                            }
                        }
                    }
                }

                vanillastations.Sort();
                var subgroup = helper.AddGroup("Enabled vanilla channels");

                foreach(String station in vanillastations)
                {
                    subgroup.AddCheckbox(station, 
                        !options.DisabledRadioStations.Contains(station), 
                        new OnCheckChanged((bool isChecked) =>
                            {
                                if(isChecked)
                                {
                                    options.DisabledRadioStations.Remove(station);
                                }
                                else
                                {
                                    options.DisabledRadioStations.Add(station);
                                }

                                options.SaveSettings();
                            }));              
                }
            }
            {
                var subgroup = helper.AddGroup("Music packs (Needs reload)");
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
                var subgroup = helper.AddGroup("Channel with all content (Needs reload)");

                subgroup.AddCheckbox("Create CSLMusic Mix channel", 
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

        private void AddOptionsShortcuts(UIHelperBase helper)
        {
            ModOptions options = ModOptions.Instance;
            helper.AddCheckbox("Enable* (Needs reload)", 
                options.EnableShortcuts, 
                new OnCheckChanged((bool isChecked) =>
                    {
                        options.EnableShortcuts = isChecked;
                    }));   

            {
                var gr = helper.AddGroup("Open playlist and settings");
                gr.AddDropdown("Key", KeyStringList.ToArray(), KeyStringList.IndexOf(ModOptions.Instance.ShortcutOpenRadioPanel.Key.ToString()), (selection) =>
                    {
                        ModOptions.Instance.ShortcutOpenRadioPanel.Key = KeyList[selection];
                        ModOptions.Instance.SaveSettings();
                    });
                gr.AddCheckbox("Ctrl", ModOptions.Instance.ShortcutOpenRadioPanel.ModifierControl, (bool isChecked) =>
                    {
                        ModOptions.Instance.ShortcutOpenRadioPanel.ModifierControl = isChecked;
                        ModOptions.Instance.SaveSettings();
                    });
                gr.AddCheckbox("Shift", ModOptions.Instance.ShortcutOpenRadioPanel.ModifierShift, (bool isChecked) =>
                    {
                        ModOptions.Instance.ShortcutOpenRadioPanel.ModifierShift = isChecked;
                        ModOptions.Instance.SaveSettings();
                    });
                gr.AddCheckbox("Alt", ModOptions.Instance.ShortcutOpenRadioPanel.ModifierAlt, (bool isChecked) =>
                    {
                        ModOptions.Instance.ShortcutOpenRadioPanel.ModifierAlt = isChecked;
                        ModOptions.Instance.SaveSettings();
                    });
            }
            {
                var gr = helper.AddGroup("Next track");
                gr.AddDropdown("Key", KeyStringList.ToArray(), KeyStringList.IndexOf(ModOptions.Instance.ShortcutNextTrack.Key.ToString()), (selection) =>
                    {
                        ModOptions.Instance.ShortcutNextTrack.Key = KeyList[selection];
                        ModOptions.Instance.SaveSettings();
                    });
                gr.AddCheckbox("Ctrl", ModOptions.Instance.ShortcutNextTrack.ModifierControl, (bool isChecked) =>
                    {
                        ModOptions.Instance.ShortcutNextTrack.ModifierControl = isChecked;
                        ModOptions.Instance.SaveSettings();
                    });
                gr.AddCheckbox("Shift", ModOptions.Instance.ShortcutNextTrack.ModifierShift, (bool isChecked) =>
                    {
                        ModOptions.Instance.ShortcutNextTrack.ModifierShift = isChecked;
                        ModOptions.Instance.SaveSettings();
                    });
                gr.AddCheckbox("Alt", ModOptions.Instance.ShortcutNextTrack.ModifierAlt, (bool isChecked) =>
                    {
                        ModOptions.Instance.ShortcutNextTrack.ModifierAlt = isChecked;
                        ModOptions.Instance.SaveSettings();
                    });
            }
            {
                var gr = helper.AddGroup("Next station");
                gr.AddDropdown("Key", KeyStringList.ToArray(), KeyStringList.IndexOf(ModOptions.Instance.ShortcutNextStation.Key.ToString()), (selection) =>
                    {
                        ModOptions.Instance.ShortcutNextStation.Key = KeyList[selection];
                        ModOptions.Instance.SaveSettings();
                    });
                gr.AddCheckbox("Ctrl", ModOptions.Instance.ShortcutNextStation.ModifierControl, (bool isChecked) =>
                    {
                        ModOptions.Instance.ShortcutNextStation.ModifierControl = isChecked;
                        ModOptions.Instance.SaveSettings();
                    });
                gr.AddCheckbox("Shift", ModOptions.Instance.ShortcutNextStation.ModifierShift, (bool isChecked) =>
                    {
                        ModOptions.Instance.ShortcutNextStation.ModifierShift = isChecked;
                        ModOptions.Instance.SaveSettings();
                    });
                gr.AddCheckbox("Alt", ModOptions.Instance.ShortcutNextStation.ModifierAlt, (bool isChecked) =>
                    {
                        ModOptions.Instance.ShortcutNextStation.ModifierAlt = isChecked;
                        ModOptions.Instance.SaveSettings();
                    });
            }
        }
    }
}

