using System;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using System.IO;
using System.Collections.Generic;

namespace CSLMusicMod.UI
{
    public class UIMusicListPanel : UIPanel
    {
        private bool _initialized = false;

        UILabel _currentMusic;
        String _currentMusic_current;
        UIListBox _musicList;
        //        UIButton _openSettings;
        //UIMusicSettingsPanel _settingsPanel;

        public CSLAudioWatcher AudioWatcher { get; set; }

        public SettingsManager SettingsManager { get; set; }

        public MusicManager MusicManager { get; set; }

        private MusicEntry _resort_CurrentItem;
        private int _resort_currentPivotIndex;
        private bool _resort_resorted;

        private bool _music_list_initialized = false;
        private List<MusicEntry> _filtered_MusicEntryList = new List<MusicEntry>();

        //Texture atlas
        private UITextureAtlas _atlas;

        //Settings UI
        private SavedFloat _MusicAudioVolume = new SavedFloat(Settings.musicAudioVolume, Settings.gameSettingsFile, DefaultSettings.musicAudioVolume, true);
        private UISlider _MusicVolumeSlider;
        private UIButton _Next_Track;
        private UIButton _Previous_Track;
        private UICheckButton _enable_Playlist_random;
        private UITextField _Filter;
        private UIButton _Filter_Clear;
        private UIButton _SortAscending;
        private UIButton _SortDescending;

        private bool Filtered
        {
            get
            {
                return _Filter.text.Trim() != "";
            }
        }

        public UIMusicListPanel()
        {
        }

        public override void Start()
        {
            base.Start();

            //Create atlas
            InitAtlases();

            Vector2 screenResolution = GetUIView().GetScreenResolution();

            backgroundSprite = "MenuPanel2";
            wrapLayout = true;
            this.width = 500;
            this.height = SettingsManager.ModOptions.LargePlayList ? screenResolution.y - 120 - 60 : 480;
            relativePosition = new Vector3(screenResolution.x - width - 10, screenResolution.y - height - 120);
            this.isVisible = false;
            this.canFocus = true;
            this.isInteractive = true;

            //Add header
            InitializeHeaderToolbar();

            //Adding "Current playing"
            InitializeCurrentPlaying();

            //Add list
            InitializeMusicList();      

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

            _initialized = true;
        }

        private void InitAtlases()
        {
            if (_atlas == null)
            {

                Debug.Log("[CSLMusicMod] Creating icon atlases ...");
                _atlas = TextureHelper.CreateAtlas("Icons.png", "CSLMusicModUI", UIView.Find<UITabstrip>("ToolMode").atlas.material, 31, 31, new string[]
                    {
                        "OptionBase",
                        "OptionBaseDisabled",
                        "OptionBaseFocused",
                        "OptionBaseHovered",
                        "OptionBasePressed",
                        "Music",
                        "Next",
                        "Previous",
                        "Shuffle",
                        "SortAscending",
                        "SortDescending",
                        "Search",
                        "Clear"
                    });  

            }
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (isVisible)
                Focus();

            UpdateValues();
        }

        public override void Update()
        {
            base.Update();

            if (isVisible && _initialized)
            {
                if (!_music_list_initialized)
                {
                    try
                    {
                        UpdateMusicList();
                        _music_list_initialized = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }

                }

                if (_currentMusic != null && AudioWatcher != null && AudioWatcher.CurrentMusicEntry != null && AudioWatcher.CurrentMusicFile != null)
                {
                    var music = Path.GetFileNameWithoutExtension(AudioWatcher.CurrentMusicFile);

                    if (music != _currentMusic_current)
                    {
                        _currentMusic_current = music;
                        _currentMusic.text = ShortenString("Now playing: " + music, 45);
                        _currentMusic.tooltip = music + "\nbelongs to " + AudioWatcher.CurrentMusicEntry.BaseName;
                    }
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


                //Makes the slider a little bit jitterish, but successfuly synchronizes volumes from outer (eg the options menu)
                   
                if (_MusicVolumeSlider.value / 100f != _MusicAudioVolume.value)
                {
                    _MusicVolumeSlider.value = _MusicAudioVolume.value * 100f;
                }

            }
        }

        private void UpdateValues()
        {
            if (_initialized)
            {               
                _enable_Playlist_random.isChecked = SettingsManager.ModOptions.RandomTrackSelection;               
                _MusicVolumeSlider.value = _MusicAudioVolume.value * 100f; //I use x100 because it failed with 0..1?

            }
        }

        private void InitializeHeaderToolbar()
        {
            //Header background
            {
                var header = AddUIComponent<UIPanel>();
                header.relativePosition = Vector3.zero;
                header.width = this.width;
                header.height = 80;
                header.backgroundSprite = "GenericTab";
            }
            {
                _MusicVolumeSlider = AddUIComponent<UISlider>();
                _MusicVolumeSlider.relativePosition = new Vector3(15, 22);
                _MusicVolumeSlider.width = 100;
                _MusicVolumeSlider.height = 10;
                //_MusicVolumeSlider.backgroundSprite = "GenericPanelLight";
                //_MusicVolumeSlider.color = new Color32(255, 255, 255, 100);
                _MusicVolumeSlider.minValue = 0;
                _MusicVolumeSlider.maxValue = 100;
                _MusicVolumeSlider.tooltip = "Drag to change the music volume";

                UIPanel thumb = _MusicVolumeSlider.AddUIComponent<UIPanel>();
                thumb.width = 15;
                thumb.height = 15;
                thumb.backgroundSprite = "GenericProgressBarFill";

                UIPanel fill = _MusicVolumeSlider.AddUIComponent<UIPanel>();
                fill.backgroundSprite = "GenericProgressBarFill";
                //fill.color = new Color32(79, 210, 233, 255);
                _MusicVolumeSlider.fillIndicatorObject = fill;

                _MusicVolumeSlider.thumbObject = thumb;

                _MusicVolumeSlider.eventValueChanged += delegate(UIComponent component, float value)
                {
                    if (this.m_IsDisposing)
                        return;

                    //I use x100 because it failed with 0..1?
                    value = value / 100f;

                    if (_MusicAudioVolume.value != value)
                    {
                        _MusicAudioVolume.value = value;                   
                    }
                };
            }
            {
                _Previous_Track = AddUIComponent<UIButton>();
                _Previous_Track.width = 36;
                _Previous_Track.height = 36;
                _Previous_Track.relativePosition = new Vector3(130, 10);
                _Previous_Track.tooltip = "Play previous track";

                _Previous_Track.atlas = _atlas;
                _Previous_Track.hoveredBgSprite = "OptionBaseFocused";
                _Previous_Track.pressedBgSprite = "OptionBasePressed";
                _Previous_Track.normalFgSprite = "Previous";

                _Previous_Track.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
                {
                    AudioWatcher.RequestSwitchToPreviousMusic();
                };
            }
            {
                _Next_Track = AddUIComponent<UIButton>();
                _Next_Track.width = 36;
                _Next_Track.height = 36;
                _Next_Track.relativePosition = new Vector3(130 + 40, 10);
                _Next_Track.normalBgSprite = "GenericPanel";
                _Next_Track.tooltip = "Play next track";

                _Next_Track.atlas = _atlas;
                _Next_Track.hoveredBgSprite = "OptionBaseFocused";
                _Next_Track.pressedBgSprite = "OptionBasePressed";
                _Next_Track.normalFgSprite = "Next";

                _Next_Track.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
                {
                    AudioWatcher.RequestSwitchMusic();
                };
            }
            {
                _SortAscending = AddUIComponent<UIButton>();
                _SortAscending.width = 36;
                _SortAscending.height = 36;
                _SortAscending.relativePosition = new Vector3(130 + 40 * 2, 10);
                _SortAscending.normalBgSprite = "GenericPanel";
                _SortAscending.tooltip = "Sort ascending";

                _SortAscending.atlas = _atlas;
                _SortAscending.hoveredBgSprite = "OptionBaseFocused";
                _SortAscending.pressedBgSprite = "OptionBasePressed";
                _SortAscending.normalFgSprite = "SortAscending";

                _SortAscending.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
                {
                    MusicManager.MusicEntries.Sort(new Comparison<MusicEntry>((x, y) =>
                            {
                                return x.BaseName.CompareTo(y.BaseName);
                            }));
                    MusicManager.SaveMusicFileSettings();

                    UpdateMusicListPreserveScroll();

                };
            }
            {
                _SortDescending = AddUIComponent<UIButton>();
                _SortDescending.width = 36;
                _SortDescending.height = 36;
                _SortDescending.relativePosition = new Vector3(130 + 40 * 3, 10);
                _SortDescending.normalBgSprite = "GenericPanel";
                _SortDescending.tooltip = "Sort ascending";

                _SortDescending.atlas = _atlas;
                _SortDescending.hoveredBgSprite = "OptionBaseFocused";
                _SortDescending.pressedBgSprite = "OptionBasePressed";
                _SortDescending.normalFgSprite = "SortDescending";

                _SortDescending.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
                {
                    MusicManager.MusicEntries.Sort(new Comparison<MusicEntry>((x, y) =>
                            {
                                return -x.BaseName.CompareTo(y.BaseName);
                            }));
                    MusicManager.SaveMusicFileSettings();

                    UpdateMusicListPreserveScroll();
                };
            }
            {
                _Filter = AddUIComponent<UITextField>();
                _Filter.width = width - 130 - 40 - 36 - 10 - 10 - 36 * 3 - 10;
                _Filter.height = 36;
                _Filter.relativePosition = new Vector3(130 + 40 + 10 + 36 * 3, 10);
                _Filter.padding = new RectOffset(6, 6, 3, 3);
                _Filter.builtinKeyNavigation = true;
                _Filter.isInteractive = true;
                _Filter.readOnly = false;
                _Filter.horizontalAlignment = UIHorizontalAlignment.Left;
                _Filter.verticalAlignment = UIVerticalAlignment.Middle;
                _Filter.selectionSprite = "EmptySprite";
                _Filter.selectionBackgroundColor = new Color32(0, 172, 234, 255);
                _Filter.normalBgSprite = "TextFieldPanel";
                _Filter.disabledTextColor = new Color32(0, 0, 0, 128);
                _Filter.color = new Color32(60, 60, 60, 255);
                _Filter.textColor = Color.gray;
                _Filter.padding = new RectOffset(6, 6, 9, 9);

                _Filter_Clear = AddUIComponent<UIButton>();
                _Filter_Clear.width = 22;
                _Filter_Clear.height = 22;
                _Filter_Clear.relativePosition = _Filter.relativePosition + new Vector3(7 + _Filter.width - 36, 7);
                _Filter_Clear.atlas = _atlas;
                _Filter_Clear.normalFgSprite = "Search";
                _Filter_Clear.hoveredColor = new Color32(255, 255, 255, 128);
            
                _Filter.eventTextChanged += delegate
                {
                    if (!Filtered)
                        _Filter_Clear.normalFgSprite = "Search";
                    else
                        _Filter_Clear.normalFgSprite = "Clear";

                    UpdateMusicList();
                };
                _Filter_Clear.eventClick += (component, eventParam) =>
                {
                    _Filter.text = "";
                };
            }
            {
                _enable_Playlist_random = AddUIComponent<UICheckButton>();
                _enable_Playlist_random.width = 36;
                _enable_Playlist_random.height = 36;
                _enable_Playlist_random.relativePosition = new Vector3(width - 10 - 36, 10);
                _enable_Playlist_random.normalBgSprite = "GenericPanel";
                _enable_Playlist_random.tooltip = "Enable randomized playback";

                _enable_Playlist_random.atlas = _atlas;
                _enable_Playlist_random.normalBgSprite = "OptionBase";
                _enable_Playlist_random.hoveredBgSprite = "OptionBaseFocused";
                _enable_Playlist_random.pressedBgSprite = "OptionBasePressed";
                _enable_Playlist_random.normalFgSprite = "Shuffle";

                _enable_Playlist_random.eventCheckStateChanged += delegate(UICheckButton sender, bool state)
                {
                    if (SettingsManager.ModOptions.RandomTrackSelection != state)
                    {
                        SettingsManager.ModOptions.RandomTrackSelection = state;
                        SettingsManager.SaveModSettings();
                    }
                };
            }
        }

        private void InitializeCurrentPlaying()
        {
            _currentMusic = AddUIComponent<UILabel>();
            _currentMusic.textColor = new Color32(50, 50, 50, 255);
            _currentMusic.width = width - 10;
            _currentMusic.height = 50;
            _currentMusic.relativePosition = new Vector3(15, 55);
            _currentMusic.isVisible = true;    
        }

        private void InitializeMusicList()
        {
            var panel = _musicList = AddUIComponent<UIListBox>();

            panel.width = width - 34;
            panel.height = height - 80 - 20;
            panel.relativePosition = new Vector3(10, 80 + 10);
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
                scroller.height = panel.height;
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
                        UISlicedSprite thumbSprite = track.AddUIComponent<UISlicedSprite>();
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

            //UpdateMusicList();

            panel.eventItemClicked += delegate(UIComponent component, int value)
            {
                if (AudioWatcher != null)
                {
                    //+ Only if not resorted, switch to track
                    if (!_resort_resorted && value >= 0 && _filtered_MusicEntryList.Count > value)
                    {
                        //AudioWatcher.RequestSwitchMusic(MusicManager.MusicEntries[value]);
                        AudioWatcher.RequestSwitchMusic(_filtered_MusicEntryList[value], true); //use filtered list
                    }
                }
            };
            panel.eventItemDoubleClicked += delegate(UIComponent component, int value)
            {
                if (value >= 0 && _filtered_MusicEntryList.Count > value)
                {
                    //Store old entry
                    MusicEntry current = AudioWatcher.CurrentMusicEntry;

                    //MusicEntry entry = MusicManager.MusicEntries[value];
                    var entry = _filtered_MusicEntryList[value];
                    entry.Enable = !entry.Enable;

                    UpdateMusicListPreserveScroll();

                    //Restore the current entry
                    AudioWatcher.RequestSwitchMusic(current, true);
                }
            };

            //Add feature to resort the music list
            panel.eventItemMouseDown += delegate(UIComponent component, int value)
            {
                if (AudioWatcher != null)
                {
                    //Disable resort while filtering
                    if (Filtered)
                        return;

                    if (value >= 0 && MusicManager.MusicEntries.Count > value)
                    {
                        _resort_CurrentItem = MusicManager.MusicEntries[value];
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
                    MusicManager.SaveMusicFileSettings();
                }
            };
            panel.eventItemMouseHover += delegate(UIComponent component, int value)
            {
                if (value >= 0 && _filtered_MusicEntryList.Count > value)
                {
                    if (_resort_CurrentItem != null && value != _resort_currentPivotIndex)
                    {
                        //Disable resort while filtering
                        if (Filtered)
                            return;

                        MusicManager.MusicEntries.Remove(_resort_CurrentItem);
                        MusicManager.MusicEntries.Insert(value, _resort_CurrentItem);
                        _resort_currentPivotIndex = value;

                        UpdateMusicListPreserveScroll();

                        _resort_resorted = true;
                    }
                    else
                    {
                        var entry = _filtered_MusicEntryList[value];

                        String tooltip = entry.BaseName + "\n----\n";
                        tooltip += "Supported tags:\n";
                        foreach (var tag in entry.TagSongs.Keys)
                        {
                            if (tag == "")
                                tooltip += "Default music\n";
                            else
                                tooltip += MusicManager.TagIndicator + tag + "\n";
                        }
                        tooltip += "\n\nClick on an item to play the song.\nDouble click to enable/disable it.\nDrag to resort the list.";

                        _musicList.tooltip = tooltip;
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

        public void UpdateMusicList()
        {
            List<String> entries = new List<string>();
            _filtered_MusicEntryList.Clear();

            Debug.Log("[CSLMusic] Generating music list");

            foreach (MusicEntry entry in MusicManager.MusicEntries)
            {
                var entrytext = EntryText(entry);

                if (IsFiltered(entrytext))
                    continue;

                _filtered_MusicEntryList.Add(entry);
                entries.Add(entrytext);
            }

            _musicList.items = entries.ToArray();
        }

        private String EntryText(MusicEntry entry)
        {
            String annot = "";

            if (!entry.Enable)
                annot += "[Disabled]";               

            String music = entry.BaseName;

            return String.Format("{0} {1}", annot, music);
        }

        private bool IsFiltered(String entrytext)
        {
            if (!Filtered)
                return false;

            return !entrytext.ToLower().Contains(_Filter.text.ToLower());
        }

        public String ShortenString(String str, int size)
        {
            var diff = str.Length - size;

            if (diff > 0)
            {
                return str.Substring(0, str.Length - diff - 4) + " ...";
            }


            return str;
        }
    }
}
