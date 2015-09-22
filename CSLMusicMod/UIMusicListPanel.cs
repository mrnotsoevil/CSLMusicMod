using System;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using System.IO;
using System.Collections.Generic;

namespace CSLMusicMod
{
    public class UIMusicListPanel : UIPanel
    {
        UILabel _currentMusic;
        UIListBox _musicList;
        UIButton _openSettings;
        UIMusicSettingsPanel _settingsPanel;

        public CSLAudioWatcher AudioWatcher { get; set; }

        private CSLCustomMusicEntry _resort_CurrentItem;
        private int _resort_currentPivotIndex;
        private bool _resort_resorted;

        public UIMusicListPanel()
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
            _settingsPanel = (UIMusicSettingsPanel)GetUIView().AddUIComponent(typeof(UIMusicSettingsPanel));
            _settingsPanel.isVisible = false;
            _settingsPanel.AudioWatcher = AudioWatcher;
            _settingsPanel.width = this.width;
            _settingsPanel.height = 402;
            _settingsPanel.relativePosition = new Vector3(relativePosition.x, relativePosition.y - _settingsPanel.height - 5);

            //Add a button for settings
            _openSettings = AddUIComponent<UIButton>();
            _openSettings.width = 40;
            //_openSettings.text = "Settings";
            _openSettings.normalFgSprite = "Options";
            _openSettings.focusedFgSprite = "Options";
            _openSettings.pressedFgSprite = "OptionsFocused";
            _openSettings.hoveredFgSprite = "OptionsFocused";
            _openSettings.height = 40;
            _openSettings.relativePosition = new Vector3(width - _openSettings.width - 1, 1);
            _openSettings.normalBgSprite = "SubcategoriesPanel";
            _openSettings.hoveredColor = new Color32(128, 128, 128, 255);
            _openSettings.isVisible = true;
            _openSettings.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
            {
                _settingsPanel.isVisible = !_settingsPanel.isVisible;
            };

            //Add ability to close with esc
            //Notice the other function to react to Esc in Update()
            eventKeyDown += delegate(UIComponent component, UIKeyEventParameter eventParam)
            {
                if (isVisible && !eventParam.used && eventParam.keycode == KeyCode.Escape)
                {
                    eventParam.Use();

                    isVisible = false;
                }
            };

            _settingsPanel.eventGotFocus += delegate(UIComponent component, UIFocusEventParameter eventParam)
            {
                //Add switch for Key binding buttons
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

                //Add the ultimate killing handler to react to "Esc"
                //Why? Because this seems not to work always
                //Why keep the event? Because this correctly intercepts the ui behaviour
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    isVisible = false;
                }
            }
        }

        private void AddList()
        {
            var panel = _musicList = AddUIComponent<UIListBox>();

            panel.width = width - 34;
            panel.height = 335;
            panel.relativePosition = new Vector3(10, 55);
            panel.textColor = new Color32(150, 150, 150, 255);
            panel.itemHover = "SubcategoriesPanel";
            panel.itemHeight = 32;
            panel.itemPadding = new RectOffset(0, 0, 4, 4);
            panel.tooltip = "Click on an item to play the song.\nDouble click to enable/disable it.\nDrag to resort the list.";
         
            panel.Show();

            /**
             * Not working -.-
             * */

            //Update 3.3 scrollbar
            //if (CSLMusicModSettings.MusicListEnableScrollbar)
            {
                var scroller = panel.AddUIComponent<UIScrollbar>();
                scroller.width = 15;
                scroller.height = height - 65;
                scroller.relativePosition = new Vector3(width - 15 - 15f, 0);
                scroller.orientation = UIOrientation.Vertical;

                //All credits to https://github.com/justacid/Skylines-ExtendedPublicTransport
                {
                    var track = scroller.AddUIComponent<UISlicedSprite>();
                    track.relativePosition = Vector2.zero;
                    track.autoSize = true;
                    track.size = track.parent.size;
                    track.fillDirection = UIFillDirection.Vertical;
                    track.spriteName = "ScrollbarTrack";
                    scroller.trackObject = track;

                    {
                        UISlicedSprite thumbSprite = track.AddUIComponent<UISlicedSprite> ();
                        thumbSprite.relativePosition = Vector2.zero;
                        thumbSprite.fillDirection = UIFillDirection.Vertical;
                        thumbSprite.autoSize = true;
                        thumbSprite.width = thumbSprite.parent.width;
                        thumbSprite.spriteName = "ChirpScrollbarThumb";
                        thumbSprite.color = new Color32(255, 255, 255, 128);
                        //thumbSprite.color = new Color32(0, 100, 180, 255);

                        scroller.thumbObject = thumbSprite;
                    }
                }

                _musicList.scrollbar = scroller;

                scroller.isVisible = true;
            }

            UpdateMusicList();

            panel.eventItemClicked += delegate(UIComponent component, int value)
            {
                if (AudioWatcher != null)
                {
                    //+ Only if not resorted, switch to track
                        if (!_resort_resorted && value >= 0 && gameObject.GetComponent<MusicManager>().MusicEntries.Count > value)
                    {
                            AudioWatcher.RequestSwitchMusic(gameObject.GetComponent<MusicManager>().MusicEntries[value], false);
                    }
                }
            };
            panel.eventItemDoubleClicked += delegate(UIComponent component, int value)
            {
                    if (value >= 0 && gameObject.GetComponent<MusicManager>().MusicEntries.Count > value)
                {
                    //Store old entry
                    CSLCustomMusicEntry current = AudioWatcher.CurrentMusicEntry;

                        CSLCustomMusicEntry entry = gameObject.GetComponent<MusicManager>().MusicEntries[value];
                    entry.Enable = !entry.Enable;

                    UpdateMusicListPreserveScroll();

                    //Restore the current entry
                    AudioWatcher.RequestSwitchMusic(current, false);
                }
            };

            //Add feature to resort the music list
            panel.eventItemMouseDown += delegate(UIComponent component, int value)
            {
                if (AudioWatcher != null)
                {
                        if (value >= 0 && gameObject.GetComponent<MusicManager>().MusicEntries.Count > value)
                    {
                            _resort_CurrentItem = gameObject.GetComponent<MusicManager>().MusicEntries[value];
                        _resort_currentPivotIndex = value;
                        _resort_resorted = false;
                    }
                }
            };
            panel.eventItemMouseUp += delegate(UIComponent component, int value)
            {
                _resort_CurrentItem = null;

                if (_resort_resorted)
                {
                        gameObject.GetComponent<MusicManager>().SaveMusicFileSettings();
                }
            };
            panel.eventItemMouseHover += delegate(UIComponent component, int value)
            {
                    if (value >= 0 && gameObject.GetComponent<MusicManager>().MusicEntries.Count > value)
                {
                    if (_resort_CurrentItem != null && value != _resort_currentPivotIndex)
                    {
                            gameObject.GetComponent<MusicManager>().MusicEntries.Remove(_resort_CurrentItem);
                            gameObject.GetComponent<MusicManager>().MusicEntries.Insert(value, _resort_CurrentItem);
                        _resort_currentPivotIndex = value;

                        UpdateMusicListPreserveScroll();

                        _resort_resorted = true;
                    }
                }
            };
        }

        private void UpdateMusicListPreserveScroll()
        {
            float scroll = _musicList.scrollPosition;

            UpdateMusicList();

            //Restore the scroll position
            try
            {
                _musicList.scrollPosition = scroll;
            }
            catch (Exception)
            {
            }
        }

        private void UpdateMusicList()
        {
            List<String> entries = new List<string>();

            foreach (CSLCustomMusicEntry entry in gameObject.GetComponent<MusicManager>().MusicEntries)
            {
                String annot = "";

                if (!entry.Enable)
                    annot += "[Disabled]";               

                String music = Path.GetFileNameWithoutExtension(entry.GoodMusic);
                String extra = "";

                if (gameObject.GetComponent<SettingsManager>().ModOptions.MusicListShortNames)
                {
                    //Update 3.3 behaviour
                    List<String> e = new List<String>();
                    if (!String.IsNullOrEmpty(entry.BadMusic))
                    {
                        if (entry.EnableBadMusic)
                            e.Add("#bad");
                    }
                    if (!String.IsNullOrEmpty(entry.SkyMusic))
                    {
                        if (entry.EnableSkyMusic)
                            e.Add("#sky");
                    }

                    extra = String.Join(" ", e.ToArray());
                }
                else
                {
                    List<String> e = new List<String>();
                    if (!String.IsNullOrEmpty(entry.BadMusic))
                    {
                        if (entry.EnableBadMusic)
                            e.Add(Path.GetFileNameWithoutExtension(entry.BadMusic));
                        else
                            e.Add(Path.GetFileNameWithoutExtension("[" + entry.BadMusic + "]"));
                    }
                    if (!String.IsNullOrEmpty(entry.SkyMusic))
                    {
                        if (entry.EnableSkyMusic)
                            e.Add(Path.GetFileNameWithoutExtension(entry.SkyMusic));
                        else
                            e.Add(Path.GetFileNameWithoutExtension("[" + entry.SkyMusic + "]"));
                    }

                    extra = String.Join(", ", e.ToArray());
                }
                entries.Add(String.Format("{0} {1} {2}", annot, music, extra));
            }

            _musicList.items = entries.ToArray();
        }
    }
}
