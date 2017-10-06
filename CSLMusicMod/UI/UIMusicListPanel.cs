using System;
using System.Linq;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using System.IO;
using System.Collections.Generic;
using ColossalFramework.IO;
using CSLMusicMod.Helpers;

namespace CSLMusicMod.UI
{
    /// <summary>
    /// UI of the music list panel
    /// </summary>
    public class UIMusicListPanel : UIPanel
    {
        private bool m_Initialized = false;

        //UILabel m_LabelCurrentMusic;
        UIListBox m_MusicList;      

        private bool m_MusicListInitialized = false;

        //Texture atlas
        private UITextureAtlas m_IconAtlas;

        //Settings UI
        private SavedFloat m_MusicAudioVolume = new SavedFloat(Settings.musicAudioVolume, 
            Settings.gameSettingsFile,
            DefaultSettings.musicAudioVolume, true);
        private UISlider m_VolumeSlider;
        private UIButton m_NextTrack;
        private UITextField m_Filter;
        private UIButton m_ClearFilter;
        private UIButton m_ButtonSortAscending;
        private UIButton m_ButtonSortDescending;
        private UIButton m_Close;
        private UILabel m_RadioChannelInfo;
        
        // Additional options UI
        private int m_AdditionalButtonCount = 0;
        private UIButton m_TopShowMusicList;
        private UIButton m_TopNextTrack;
        private UIButton m_TopOpenStationDirectory;

        private bool m_SortAscending = true;

        private List<RadioContentInfo> m_CurrentContent = new List<RadioContentInfo>();

        private ModOptions m_ModOptionsInstance = ModOptions.Instance;

        private RadioPanel m_CurrentRadioPanel = null;

        private RadioPanel CurrentRadioPanel
        {
            get
            {
                if (m_CurrentRadioPanel != null)
                    return m_CurrentRadioPanel;
                else
                {
                    var radiopanel = Resources.FindObjectsOfTypeAll<RadioPanel>().FirstOrDefault();
                    m_CurrentRadioPanel = radiopanel;

                    return radiopanel;
                }
            }
        }

        private bool Filtered
        {
            get
            {
                return m_Filter.text.Trim() != "";
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
            this.height = screenResolution.y - 120 - 100;
            relativePosition = new Vector3(screenResolution.x - width - 10, screenResolution.y - height - 120);
            this.isVisible = false;
            this.canFocus = true;
            this.isInteractive = true;
            this.m_ZIndex = -100;
            
            InitializeShowMusicPanelButton();
            InitializeTopNextTrackButton();
            
            if(ModOptions.Instance.EnableOpenStationDirButton)
                InitializeOpenStationDirectoryButton();

            //Add header
            InitializeHeaderToolbar();         

            //Add list
            InitializeMusicList();  

            m_Initialized = true;

            Singleton<AudioManager>.instance.m_radioContentChanged += RadioContentChanged;
        }

        void RadioContentChanged ()
        {
            RebuildList();
        }

        public override void OnDestroy()
        {
            Singleton<AudioManager>.instance.m_radioContentChanged -= RadioContentChanged;

            base.OnDestroy();
        }

        private void InitAtlases()
        {
            if (m_IconAtlas == null)
            {

                CSLMusicMod.Log("Creating icon atlases ...");
                m_IconAtlas = TextureHelper.CreateDefaultIconAtlas();

            }
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            // Bring the radio panel to the front
            var radiopanel = CurrentRadioPanel;

            if(radiopanel != null)
            {
                var panel = ReflectionHelper.GetPrivateField<UIPanel>(radiopanel, "m_radioPanel");
                var list = ReflectionHelper.GetPrivateField<UIPanel>(radiopanel, "m_radioList");

                if (panel != null)
                    panel.BringToFront();
                if (list != null)
                    panel.BringToFront();

                this.SendToBack();
            }

            UpdateValues();
        }

        public override void Update()
        {
            base.Update();

            if (isVisible && m_Initialized)
            {
                if (!m_MusicListInitialized)
                {
                    try
                    {
                        RebuildList();
                        m_MusicListInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }

                }

                //m_LabelCurrentMusic.text = "";               

                if (Math.Abs(m_VolumeSlider.value / 100f - m_MusicAudioVolume.value) > 0.01)
                {
                    m_VolumeSlider.value = m_MusicAudioVolume.value * 100f;
                }            
            }           
        }

        private void RebuildList()
        {
            AudioManager mgr = Singleton<AudioManager>.instance;

            ushort activechannel = ReflectionHelper.GetPrivateField<ushort>(mgr, "m_activeRadioChannel");

            //Debug.Log("Selected active channel " + activechannel + " of " + mgr.m_radioChannelCount);

            //Dictionary<RadioContentInfo, String> entrytexts = new Dictionary<RadioContentInfo, string>();

            if(activechannel >= 0)
            {
                RadioChannelData channeldata = mgr.m_radioChannels[activechannel];
                RadioChannelInfo info = channeldata.Info;

                m_CurrentContent.Clear();

                if(info != null)
                {
                    // Only show supported content entries
                    HashSet<RadioContentInfo.ContentType> supported_content = new HashSet<RadioContentInfo.ContentType>();

                    foreach(var state in info.m_stateChain)
                    {
                        supported_content.Add(state.m_contentType);
                    }

                    for(uint i = 0; i < PrefabCollection<RadioContentInfo>.PrefabCount(); ++i)
                    {
                        var c = PrefabCollection<RadioContentInfo>.GetPrefab(i);

                        if (c == null)
                            continue;
                        if (c.m_radioChannels == null)
                            continue;

                        if(supported_content.Contains(c.m_contentType) && c.m_radioChannels.Contains(info))
                        {
                            //entrytexts[c] = GetEntryTextFor(c);

                            //if(!IsFiltered(entrytexts[c]))
                            if(!IsFiltered(AudioManagerHelper.GetContentName(c)))
                            {
                                m_CurrentContent.Add(c);
                            }
                        }
                    }
                }

                m_RadioChannelInfo.isVisible = m_CurrentContent.Count == 0;

                //Debug.Log(m_CurrentContent.Count + " entries ");
            }

            m_CurrentContent.Sort((RadioContentInfo x, RadioContentInfo y) =>
                {
                    if(m_SortAscending)
                    {
                        //return string.Compare(entrytexts[x], entrytexts[y], StringComparison.CurrentCulture);
                        return string.Compare(AudioManagerHelper.GetContentName(x), AudioManagerHelper.GetContentName(y), StringComparison.CurrentCulture);
                    }
                    else
                    {
                        //return string.Compare(entrytexts[y], entrytexts[x], StringComparison.CurrentCulture);
                        return string.Compare(AudioManagerHelper.GetContentName(y), AudioManagerHelper.GetContentName(x), StringComparison.CurrentCulture);
                    }
                });

            RefreshListWidget();
        }

        private String GetEntryTextFor(RadioContentInfo content)
        {
            String name = AudioManagerHelper.GetContentName(content);

            switch(content.m_contentType)
            {
                case RadioContentInfo.ContentType.Blurb:
                    name = "<sprite Blurb> " + name;
                    break;
                case RadioContentInfo.ContentType.Broadcast:
                    name = "<sprite Broadcast> " + name;
                    break;
                case RadioContentInfo.ContentType.Commercial:
                    name = "<sprite Commercial> " + name;
                    break;
                case RadioContentInfo.ContentType.Music:
                    name = "<sprite Music> " + name;
                    break;
                case RadioContentInfo.ContentType.Talk:
                    name = "<sprite Talk> " + name;
                    break;
            }

            if(!AudioManagerHelper.ContentIsEnabled(content))
            {
                name = "<sprite ContentDisabled>" + name;
            }


            return name;
        }

        private void RefreshListWidget()
        {
            float scroll = m_MusicList.scrollPosition;

            m_MusicList.items = m_CurrentContent.Select(content => GetEntryTextFor(content)).ToArray();


            //Restore the scroll position
            try
            {
                m_MusicList.scrollPosition = scroll;
            }
            catch (Exception)
            {
            }
        }

        private void UpdateValues()
        {
            if (m_Initialized)
            {               
                //_enable_Playlist_random.isChecked = SettingsManager.ModOptions.RandomTrackSelection;               
                m_VolumeSlider.value = m_MusicAudioVolume.value * 100f; //I use x100 because it failed with 0..1?

            }
        }

        private void InitializeHeaderToolbarVolumeSlider()
        {
            m_VolumeSlider = AddUIComponent<UISlider>();
            m_VolumeSlider.relativePosition = new Vector3(15, 22);
            m_VolumeSlider.width = 100;
            m_VolumeSlider.height = 10;
            //_MusicVolumeSlider.backgroundSprite = "GenericPanelLight";
            //_MusicVolumeSlider.color = new Color32(255, 255, 255, 100);
            m_VolumeSlider.minValue = 0;
            m_VolumeSlider.maxValue = 100;
            m_VolumeSlider.tooltip = "Drag to change the music volume";

            UIPanel thumb = m_VolumeSlider.AddUIComponent<UIPanel>();
            thumb.width = 15;
            thumb.height = 15;
            thumb.backgroundSprite = "GenericProgressBarFill";

            UIPanel fill = m_VolumeSlider.AddUIComponent<UIPanel>();
            fill.backgroundSprite = "GenericProgressBarFill";
            //fill.color = new Color32(79, 210, 233, 255);
            m_VolumeSlider.fillIndicatorObject = fill;

            m_VolumeSlider.thumbObject = thumb;

            m_VolumeSlider.eventValueChanged += delegate (UIComponent component, float value)
            {
                if (this.m_IsDisposing)
                    return;

                    //I use x100 because it failed with 0..1?
                    value = value / 100f;

                if (Math.Abs(m_MusicAudioVolume.value - value) > 0.01)
                {
                    m_MusicAudioVolume.value = value;
                }
            };
        }

        private void InitializeHeaderToolbarNextTrackButton()
        {
            m_NextTrack = AddUIComponent<UIButton>();
            m_NextTrack.width = 36;
            m_NextTrack.height = 36;
            m_NextTrack.relativePosition = new Vector3(130, 10);
            m_NextTrack.normalBgSprite = "GenericPanel";
            m_NextTrack.tooltip = "Play next track";

            m_NextTrack.atlas = m_IconAtlas;
            m_NextTrack.hoveredBgSprite = "OptionBaseFocused";
            m_NextTrack.pressedBgSprite = "OptionBasePressed";
            m_NextTrack.normalFgSprite = "Next";

            m_NextTrack.eventClick += buttonNextTrackClicked;
        }

        private void InitializeHeaderToolbarSortAscendingButton()
        {
            m_ButtonSortAscending = AddUIComponent<UIButton>();
            m_ButtonSortAscending.width = 36;
            m_ButtonSortAscending.height = 36;
            m_ButtonSortAscending.relativePosition = new Vector3(130 + 40, 10);
            m_ButtonSortAscending.normalBgSprite = "GenericPanel";
            m_ButtonSortAscending.tooltip = "Sort ascending";

            m_ButtonSortAscending.atlas = m_IconAtlas;
            m_ButtonSortAscending.hoveredBgSprite = "OptionBaseFocused";
            m_ButtonSortAscending.pressedBgSprite = "OptionBasePressed";
            m_ButtonSortAscending.normalFgSprite = "SortAscending";

            m_ButtonSortAscending.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                m_SortAscending = true;
                RebuildList();
            };
        }

        private void InitializeHeaderToolbarSortDescendingButton()
        {
            m_ButtonSortDescending = AddUIComponent<UIButton>();
            m_ButtonSortDescending.width = 36;
            m_ButtonSortDescending.height = 36;
            m_ButtonSortDescending.relativePosition = new Vector3(130 + 40 * 2, 10);
            m_ButtonSortDescending.normalBgSprite = "GenericPanel";
            m_ButtonSortDescending.tooltip = "Sort descending";

            m_ButtonSortDescending.atlas = m_IconAtlas;
            m_ButtonSortDescending.hoveredBgSprite = "OptionBaseFocused";
            m_ButtonSortDescending.pressedBgSprite = "OptionBasePressed";
            m_ButtonSortDescending.normalFgSprite = "SortDescending";

            m_ButtonSortDescending.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                m_SortAscending = false;
                RebuildList();
            };
        }

        private void InitializeHeaderToolbarFilterBar()
        {
            m_Filter = AddUIComponent<UITextField>();
            m_Filter.width = width - 130 - 40 - 36 - 10 - 10 - 36 * 2 - 10;
            m_Filter.height = 36;
            m_Filter.relativePosition = new Vector3(130 + 40 + 10 + 36 * 2, 10);
            m_Filter.padding = new RectOffset(6, 6, 3, 3);
            m_Filter.builtinKeyNavigation = true;
            m_Filter.isInteractive = true;
            m_Filter.readOnly = false;
            m_Filter.horizontalAlignment = UIHorizontalAlignment.Left;
            m_Filter.verticalAlignment = UIVerticalAlignment.Middle;
            m_Filter.selectionSprite = "EmptySprite";
            m_Filter.selectionBackgroundColor = new Color32(0, 172, 234, 255);
            m_Filter.normalBgSprite = "TextFieldPanel";
            m_Filter.disabledTextColor = new Color32(0, 0, 0, 128);
            m_Filter.color = new Color32(60, 60, 60, 255);
            m_Filter.textColor = Color.gray;
            m_Filter.padding = new RectOffset(6, 6, 9, 9);

            m_ClearFilter = AddUIComponent<UIButton>();
            m_ClearFilter.width = 22;
            m_ClearFilter.height = 22;
            m_ClearFilter.relativePosition = m_Filter.relativePosition + new Vector3(7 + m_Filter.width - 36, 7);
            m_ClearFilter.atlas = m_IconAtlas;
            m_ClearFilter.normalFgSprite = "Search";
            m_ClearFilter.hoveredColor = new Color32(255, 255, 255, 128);

            m_Filter.eventTextChanged += filterTextChanged;
            m_ClearFilter.eventClick += (component, eventParam) =>
            {
                m_Filter.text = "";
            };
        }

        private void InitializeHeaderToolbarCloseButton()
        {
            m_Close = AddUIComponent<UIButton>();
            m_Close.width = 36;
            m_Close.height = 36;
            m_Close.relativePosition = new Vector3(width - 10 - 36, 10);
            m_Close.normalBgSprite = "GenericPanel";
            m_Close.tooltip = "Close this panel";

            m_Close.atlas = m_IconAtlas;
            m_Close.normalBgSprite = "GenericPanel";
            m_Close.hoveredBgSprite = "OptionBaseFocused";
            m_Close.pressedBgSprite = "OptionBasePressed";
            m_Close.normalFgSprite = "Close";

            m_Close.eventClicked += buttonCloseClicked;
        }

        private void InitializeHeaderToolbar()
        {
            //Header background
            {
                var header = AddUIComponent<UIPanel>();
                header.relativePosition = Vector3.zero;
                header.width = this.width;
                header.height = 60;
                header.backgroundSprite = "GenericTab";
            }

            InitializeHeaderToolbarVolumeSlider();
            InitializeHeaderToolbarNextTrackButton();
            InitializeHeaderToolbarSortAscendingButton();
            InitializeHeaderToolbarSortDescendingButton();
            InitializeHeaderToolbarFilterBar();
            InitializeHeaderToolbarCloseButton();
         
        }

        private void InitializeShowMusicPanelButton()
        {
            UIMultiStateButton mutebutton =
                ReflectionHelper.GetPrivateField<UIMultiStateButton>(CurrentRadioPanel, "m_muteButton");
            UIPanel radiopanel = ReflectionHelper.GetPrivateField<UIPanel>(CurrentRadioPanel, "m_radioPanel");

            m_TopShowMusicList = radiopanel.AddUIComponent<UIButton>();
            m_TopShowMusicList.position = mutebutton.position + new Vector3((mutebutton.size.x + 5) + (m_AdditionalButtonCount++ * (mutebutton.size.y + 5)), 0);
            m_TopShowMusicList.size = new Vector2(mutebutton.size.y, mutebutton.size.y);
            m_TopShowMusicList.atlas = m_IconAtlas;
            m_TopShowMusicList.normalBgSprite = "Menu";
            m_TopShowMusicList.hoveredBgSprite = "Menu";
            m_TopShowMusicList.color = new Color32(225, 225, 225, 255);
            m_TopShowMusicList.hoveredColor = new Color32(255, 255, 255, 255);
            m_TopShowMusicList.tooltip = "Shows/hides the playlist.";
            m_TopShowMusicList.Show();
            
            m_TopShowMusicList.eventClick += TopShowMusicListOnEventClick;
        }

        private void TopShowMusicListOnEventClick(UIComponent uiComponent, UIMouseEventParameter param)
        {
            ModOptions.Instance.MusicListVisible = !ModOptions.Instance.MusicListVisible;
        }

        private void InitializeTopNextTrackButton()
        {
            UIMultiStateButton mutebutton =
                ReflectionHelper.GetPrivateField<UIMultiStateButton>(CurrentRadioPanel, "m_muteButton");
            UIPanel radiopanel = ReflectionHelper.GetPrivateField<UIPanel>(CurrentRadioPanel, "m_radioPanel");

            m_TopNextTrack = radiopanel.AddUIComponent<UIButton>();
            m_TopNextTrack.position = mutebutton.position + new Vector3((mutebutton.size.x + 5) + (m_AdditionalButtonCount++ * (mutebutton.size.y + 5)), 0);
            m_TopNextTrack.size = new Vector2(mutebutton.size.y, mutebutton.size.y);
            m_TopNextTrack.atlas = m_IconAtlas;
            m_TopNextTrack.normalBgSprite = "Next";
            m_TopNextTrack.hoveredBgSprite = "Next";
            m_TopNextTrack.color = new Color32(225, 225, 225, 255);
            m_TopNextTrack.hoveredColor = new Color32(255, 255, 255, 255);
            m_TopNextTrack.tooltip = "Switches to the next track.";
            m_TopNextTrack.Show();

            m_TopNextTrack.eventClick += buttonNextTrackClicked;
        }

        private void InitializeOpenStationDirectoryButton()
        {
            UIMultiStateButton mutebutton =
                ReflectionHelper.GetPrivateField<UIMultiStateButton>(CurrentRadioPanel, "m_muteButton");
            UIPanel radiopanel = ReflectionHelper.GetPrivateField<UIPanel>(CurrentRadioPanel, "m_radioPanel");

            m_TopOpenStationDirectory = radiopanel.AddUIComponent<UIButton>();
            m_TopOpenStationDirectory.position = mutebutton.position + new Vector3((mutebutton.size.x + 5) + (m_AdditionalButtonCount++ * (mutebutton.size.y + 5)), 0);
            m_TopOpenStationDirectory.size = new Vector2(mutebutton.size.y, mutebutton.size.y);
            m_TopOpenStationDirectory.atlas = m_IconAtlas;
            m_TopOpenStationDirectory.normalBgSprite = "Open";
            m_TopOpenStationDirectory.hoveredBgSprite = "Open";
            m_TopOpenStationDirectory.color = new Color32(225, 225, 225, 255);
            m_TopOpenStationDirectory.hoveredColor = new Color32(255, 255, 255, 255);
            m_TopOpenStationDirectory.tooltip = "Open folder containing the radio station.";
            m_TopOpenStationDirectory.Show();
           
            m_TopOpenStationDirectory.eventClick += TopOpenStationDirectoryOnEventClick;

        }

        private void TopOpenStationDirectoryOnEventClick(UIComponent uiComponent, UIMouseEventParameter uiMouseEventParameter)
        {
            var data = AudioManagerHelper.GetActiveChannelData();
            if (data != null)
            {
                var info = AudioManagerHelper.GetUserChannelInfo(data.Value.Info);
                var dir = DataLocation.gameContentPath;

                if (info != null)
                {
                    dir = info.m_DefinitionDirectory;
                }
                    
                DesktopHelper.OpenFileExternally(dir);
            }
        }

        void buttonNextTrackClicked (UIComponent component, UIMouseEventParameter eventParam)
        {
            AudioManagerHelper.NextTrack();           
        }

        void musicEntrySelected (UIComponent component, int value)
        {
            RadioContentInfo info = m_CurrentContent[value];
            AudioManagerHelper.SwitchToContent(info);
        }


        void musicEntryEnableDisable (UIComponent component, int value)
        {
            RadioContentInfo info = m_CurrentContent[value];
            AudioManagerHelper.SetContentEnabled(info, !AudioManagerHelper.ContentIsEnabled(info));

            RebuildList();
        }

        void filterTextChanged (UIComponent component, string value)
        {
            if (!Filtered)
                m_ClearFilter.normalFgSprite = "Search";
            else
                m_ClearFilter.normalFgSprite = "Clear";

            RebuildList();
        }

        void buttonCloseClicked (UIComponent component, UIMouseEventParameter eventParam)
        {
            var radiopanel = CurrentRadioPanel;
            radiopanel.HideRadio();
        }

        private void InitializeMusicListChannelInfo()
        {
            m_RadioChannelInfo = AddUIComponent<UILabel>();
            m_RadioChannelInfo.relativePosition = new Vector3(10, 60 + 10);
            m_RadioChannelInfo.width = m_MusicList.width;
            m_RadioChannelInfo.height = m_MusicList.height;
            m_RadioChannelInfo.textColor = new Color32(150, 150, 150, 255);
            m_RadioChannelInfo.text = "This channel is not a radio-channel.\nYour custom music can be found in\n'Userdefined', 'CSLMusic Mix' and\nchannels created by music packs.";
            m_RadioChannelInfo.wordWrap = false;
            m_RadioChannelInfo.Show();
        }

        private void InitializeMusicListScrollBar()
        {
            var scroller = m_MusicList.AddUIComponent<UIScrollbar>();
            scroller.width = 15;
            scroller.height = m_MusicList.height;
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

            m_MusicList.scrollbar = scroller;

            scroller.isVisible = true;
        }

        private void InitializeMusicList()
        {
            m_MusicList = AddUIComponent<UIListBox>();

            m_MusicList.width = width - 34;
            m_MusicList.height = height - 60 - 20;
            m_MusicList.relativePosition = new Vector3(10, 60 + 10);
            m_MusicList.textColor = new Color32(150, 150, 150, 255);
            m_MusicList.itemHighlight = "ListEntryNormal";
            m_MusicList.itemHover = "ListEntryHover";
            m_MusicList.itemHeight = 32;
            m_MusicList.itemPadding = new RectOffset(0, 0, 4, 4);
            m_MusicList.tooltip = "Double-click to disable an entry";
            m_MusicList.zOrder = -50;
            m_MusicList.atlas = m_IconAtlas;
            m_MusicList.processMarkup = true;
            //m_MusicList.animateHover = true;
         
            m_MusicList.Show();

            InitializeMusicListChannelInfo();

            InitializeMusicListScrollBar();

            //UpdateMusicList();

            m_MusicList.eventItemClicked += musicEntrySelected;
            m_MusicList.eventItemDoubleClicked += musicEntryEnableDisable;
           
        }

        private bool IsFiltered(String entrytext)
        {
            if (!Filtered)
                return false;

            return !entrytext.ToLower().Contains(m_Filter.text.ToLower());
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
