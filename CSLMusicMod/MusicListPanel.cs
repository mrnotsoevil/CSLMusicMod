using System;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using System.IO;
using System.Collections.Generic;

namespace CSLMusicMod
{
    public class MusicListPanel : UIPanel
    {     
        UILabel _currentMusic;
        UIListBox _musicList;
        UIButton _openSettings;
        MusicSettingsPanel _settingsPanel;

        public CSLAudioWatcher AudioWatcher { get; set; }

        public MusicListPanel()
        {
        }

        public override void Start()
        {
            base.Start();

            Vector2 screenResolution = GetUIView().GetScreenResolution();

            backgroundSprite = "MenuPanel2";
            wrapLayout = true;
            this.width = 500;
            this.height = 400;
            relativePosition = new Vector3(screenResolution.x - width - 10, screenResolution.y - height - 120);
            this.isVisible = false;
            this.canFocus = true;
            this.isInteractive = true;

            //Adding "Current playing"
            _currentMusic = AddUIComponent<UILabel>();
            _currentMusic.textColor = new Color32(128, 128, 128, 255);
            _currentMusic.width = width;
            _currentMusic.height = 50;
            _currentMusic.relativePosition = new Vector3(15, 14);
            _currentMusic.isVisible = true;

            //Add list of music
            AddList();

            //Add settings panel
            _settingsPanel = (MusicSettingsPanel)GetUIView().AddUIComponent(typeof(MusicSettingsPanel));
            _settingsPanel.isVisible = false;
            _settingsPanel.AudioWatcher = AudioWatcher;
            _settingsPanel.width = this.width;
            _settingsPanel.height = 300;
            _settingsPanel.relativePosition = new Vector3(relativePosition.x, relativePosition.y - _settingsPanel.height - 5);

            //Add a button for settings
            _openSettings = AddUIComponent<UIButton>();
            _openSettings.width = 120;
            _openSettings.text = "Settings";
            _openSettings.height = 40;
            _openSettings.relativePosition = new Vector3(width - 120 - 1, 1);
            _openSettings.normalBgSprite = "SubcategoriesPanel";
            _openSettings.hoveredColor = new Color32(128, 128, 128, 255);
            _openSettings.isVisible = true;
            _openSettings.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
            {
                _settingsPanel.isVisible = !_settingsPanel.isVisible;
            };

            //Add ability to close with esc
            eventKeyDown += delegate(UIComponent component, UIKeyEventParameter eventParam)
            {
                if (!eventParam.used && eventParam.keycode == KeyCode.Escape)
                {
                    eventParam.Use();

                    isVisible = false;
                }
            };

            _settingsPanel.eventGotFocus += delegate(UIComponent component, UIFocusEventParameter eventParam)
            {
                this.Focus();
            };
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (_settingsPanel != null)
                _settingsPanel.isVisible = false;
            if (isVisible)
                Focus();
        }

        public override void Update()
        {
            base.Update();

            if (isVisible)
            {
                if (_currentMusic != null && AudioWatcher != null && AudioWatcher.CurrentMusicEntry != null && AudioWatcher.CurrentMusicFile != null)
                {
                    _currentMusic.text = "Now playing: " + Path.GetFileNameWithoutExtension(AudioWatcher.CurrentMusicFile);
                }
                else
                {
                    _currentMusic.text = "Now playing: -";
                }
            }
        }

        private void AddList()
        {
            var panel = _musicList = AddUIComponent<UIListBox>();

            panel.width = width - 30;
            panel.height = 335;
            panel.relativePosition = new Vector3(10, 55);
            panel.textColor = new Color32(150, 150, 150, 255);
            panel.itemHover = "SubcategoriesPanel";
            panel.itemHeight = 32;
            panel.itemPadding = new RectOffset(0, 0, 4, 4);
            panel.tooltip = "Click on an item to play the song. Double click to enable/disable it";
         
            panel.Show();

            UpdateMusicList();

            panel.eventItemClicked += delegate(UIComponent component, int value)
            {
                if (AudioWatcher != null)
                {
                    if (value >= 0 && CSLMusicModSettings.MusicEntries.m_size > value)
                    {
                        AudioWatcher.RequestSwitchMusic(CSLMusicModSettings.MusicEntries[value]);
                    }
                }
            };
            panel.eventItemDoubleClicked += delegate(UIComponent component, int value)
            {
                if (value >= 0 && CSLMusicModSettings.MusicEntries.m_size > value)
                {
                    CSLCustomMusicEntry entry = CSLMusicModSettings.MusicEntries[value];
                    entry.Enable = !entry.Enable;

                    UpdateMusicList();
                }
            };
        }

        private void UpdateMusicList()
        {
            List<String> entries = new List<string>();

            foreach (CSLCustomMusicEntry entry in CSLMusicModSettings.MusicEntries)
            {
                String annot = "";

                if (!entry.Enable)
                    annot += "[Disabled]";               

                String music = Path.GetFileNameWithoutExtension(entry.GoodMusic);

                if (!String.IsNullOrEmpty(entry.BadMusic))
                    music += ", " + Path.GetFileNameWithoutExtension(entry.BadMusic);
                if (!String.IsNullOrEmpty(entry.SkyMusic))               
                    music += ", " + Path.GetFileNameWithoutExtension(entry.SkyMusic);

                entries.Add(String.Format("{0} {1}", annot, music));
            }

            _musicList.items = entries.ToArray();
        }
    }
}
