using System;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using System.IO;
using System.Collections.Generic;

namespace CSLMusicMod
{
    public class MusicSettingsPanel : UIPanel
    {     
        //UIPanel _optionsBackgroundPanel;
        UILabel _currentMusic;
        UIListBox _musicList;
        /*private UICheckBox _enableMood;
        private UICheckBox _enableSky;
        private UICheckBox _loading_Music;
        private UICheckBox _enableChirpy;*/
        public CSLAudioWatcher AudioWatcher { get; set; }

        public MusicSettingsPanel()
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


            //AddOptionsPanel();
            //AddOptions();
            AddList();

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
        /*private void AddOptionsPanel()
        {
            var panel = _optionsBackgroundPanel = AddUIComponent<UIPanel>();
            panel.width = 200;
            panel.height = 335;
            panel.relativePosition = new Vector3(width - panel.width - 10, 55);
            panel.backgroundSprite = "SubcategoriesPanel";
            panel.color = new Color32(0, 0, 0, 20);
            panel.Show();
        }*/
        /*private void AddOptions()
        {
            _enableMood = AddUIComponent<UICheckBox>();
            _enableSky = AddUIComponent<UICheckBox>();
            _loading_Music = AddUIComponent<UICheckBox>();
            _enableChirpy = AddUIComponent<UICheckBox>();

            _enableMood.width = 200;
            _enableMood.height = 20;
            _enableSky.width = 200;
            _enableSky.height = 20;
            _loading_Music.width = 200;
            _loading_Music.height = 20;
            _enableChirpy.width = 200;
            _enableChirpy.height = 20;           

            _enableMood.relativePosition = new Vector3(5, 5);
            _enableSky.relativePosition = new Vector3(5, 45);
            _loading_Music.relativePosition = new Vector3(5, 85);
            _enableChirpy.relativePosition = new Vector3(5, 125);

            _enableMood.Show();
            _enableSky.Show();
            _loading_Music.Show();
            _enableChirpy.Show();


            _enableMood.text = "Enable 'bad' music";
            _enableSky.text = "Enable 'sky' music";
            _loading_Music.text = "Music while loading";
            _enableChirpy.text = "Use Chirper";
        }*/
        private void AddList()
        {
            var panel = _musicList = AddUIComponent<UIListBox>();
            //panel.width = 280;
            panel.width = width - 30;
            panel.height = 335;
            panel.relativePosition = new Vector3(10, 55);
            panel.textColor = new Color32(150, 150, 150, 255);
            panel.itemHover = "SubcategoriesPanel";
            panel.itemHeight = 32;
            panel.itemPadding = new RectOffset(0, 0, 4, 4);
            panel.tooltip = "Click on an item to play the song. Double click to enable/disable the item";
           
            /*panel.scrollbar = AddUIComponent<UIScrollbar>();
            panel.scrollbar.width = 20;
            panel.scrollbar.height = 335;
            panel.scrollbar.relativePosition = new Vector3(width - 20, 55);
            panel.scrollbar.Show();*/
           
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
