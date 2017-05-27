using System;
using System.Linq;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using System.IO;
using System.Collections.Generic;

namespace CSLMusicMod.UI
{
    /// <summary>
    /// UI of the music list panel
    /// </summary>
    public class UIMusicListPanel : UIPanel
    {
        private bool m_Initialized = false;

        //UILabel m_LabelCurrentMusic;
        //UIListBox m_MusicList;      

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

        // Experimental new UI
        private UIScrollablePanel m_MusicList;
        private UIScrollbar m_MusicListScroll;

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
                m_IconAtlas = TextureHelper.CreateAtlas("icons.png", "CSLMusicModUI", UIView.Find<UITabstrip>("ToolMode").atlas.material, 31, 31, new string[]
                    {
                        "OptionBase",
                        "OptionBaseDisabled",
                        "OptionBaseFocused",
                        "OptionBaseHovered",
                        "OptionBasePressed",
                        "Music",
                        "Next",
                        "Previous",
                        "Close",
                        "SortAscending",
                        "SortDescending",
                        "Search",
                        "Clear",
                        "Talk",
                        "Broadcast",
                        "Commercial",
                        "Blurb",
                        "MoveUp",
                        "MoveDown",
                        "Disable"
                    });
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
                            m_CurrentContent.Add(c);
                        }
                    }
                }

                m_RadioChannelInfo.isVisible = m_CurrentContent.Count == 0;

                //Debug.Log(m_CurrentContent.Count + " entries ");
            }           

            RefreshListWidget();
        }


        private void RefreshListWidget()
        {
            float scroll = m_MusicListScroll.value;

            // Rebuild the UI           
            while(m_MusicList.components.Count > 0)
            {
                var component = m_MusicList.components[0];
                m_MusicList.RemoveUIComponent(component);
                MonoBehaviour.Destroy(component);
            }
            foreach(var content in m_CurrentContent)
            {
                var entry = m_MusicList.AddUIComponent<UIMusicListEntry>();
                entry.m_IconAtlas = m_IconAtlas;
                entry.width = m_MusicList.width - 10;
                entry.height = 32;
                entry.Show();

                // Must be called after Show()
                entry.SetName(String.IsNullOrEmpty(content.m_displayName) ? content.name : content.m_displayName);
                entry.SetContentType(content.m_contentType);
                entry.SetContentEnabled(AudioManagerHelper.ContentIsEnabled(content));
            }

            //Restore the scroll position
            try
            {
                m_MusicListScroll.value = scroll;
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

        private void InitializeHeaderToolbarSortAscending()
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
                RebuildList();
            };
        }

        private void InitializeHeaderToolbarSortDescending()
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
            InitializeHeaderToolbarSortAscending();
            InitializeHeaderToolbarSortDescending();
            InitializeHeaderToolbarFilterBar();
            InitializeHeaderToolbarCloseButton();         
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
            String id = info.m_folderName + "/" + info.m_fileName;

            if(ModOptions.Instance.DisabledContent.Contains(id))
            {
                ModOptions.Instance.DisabledContent.Remove(id);
            }
            else
            {
                ModOptions.Instance.DisabledContent.Add(id);
            }
            ModOptions.Instance.SaveSettings();

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

        /// <summary>
        /// Initializes a panel that shows a message if no radio channel is selected
        /// </summary>
        private void initializeRadioChannelInfo()
        {
            m_RadioChannelInfo = AddUIComponent<UILabel>();
            m_RadioChannelInfo.relativePosition = new Vector3(10, 60 + 10);
            m_RadioChannelInfo.width = width - 34;
            m_RadioChannelInfo.height = width - 34;
            m_RadioChannelInfo.textColor = new Color32(150, 150, 150, 255);
            m_RadioChannelInfo.text = StringHelper.Wrap("This channel is not a radio-channel. " +
                                                        "Your custom music can be found in 'Userdefined', " +
                                                        "'CSLMusic Mix' and channels created by music packs." +
                                                        "Click on the 'Cities Skylines' logo in the radio " +
                                                        "panel to open the list of all radio stations.", 40);
            m_RadioChannelInfo.wordWrap = false;
            m_RadioChannelInfo.Show();
        }

        /// <summary>
        /// Initializes the scrollbar of the music list. Must be run AFTER initializing the list.
        /// </summary>
        private void initializeMusicListScroller()
        {
            m_MusicListScroll = AddUIComponent<UIScrollbar>();
            m_MusicListScroll.width = 15;
            m_MusicListScroll.height = m_MusicList.height;
            m_MusicListScroll.relativePosition = new Vector3(width - 15 - 7.5f, 60 + 10);
            m_MusicListScroll.orientation = UIOrientation.Vertical;

            //All credits to https://github.com/justacid/Skylines-ExtendedPublicTransport
            {
                var track = m_MusicListScroll.AddUIComponent<UISlicedSprite>();
                track.relativePosition = Vector2.zero;
                track.autoSize = true;
                track.size = track.parent.size;
                track.fillDirection = UIFillDirection.Vertical;
                track.spriteName = "ScrollbarTrack";
                m_MusicListScroll.trackObject = track;

                {
                    UISlicedSprite thumbSprite = track.AddUIComponent<UISlicedSprite>();
                    thumbSprite.relativePosition = Vector2.zero;
                    thumbSprite.fillDirection = UIFillDirection.Vertical;
                    thumbSprite.autoSize = true;
                    thumbSprite.width = thumbSprite.parent.width;
                    thumbSprite.spriteName = "ChirpScrollbarThumb";
                    thumbSprite.color = new Color32(255, 255, 255, 128);
                    //thumbSprite.color = new Color32(0, 100, 180, 255);

                    m_MusicListScroll.thumbObject = thumbSprite;
                }
            }

            m_MusicList.verticalScrollbar = m_MusicListScroll;

            m_MusicListScroll.isVisible = true;
        }

        private void InitializeMusicList()
        {
            initializeRadioChannelInfo();

            m_MusicList = AddUIComponent<UIScrollablePanel>();
            m_MusicList.autoLayout = true;
            m_MusicList.autoLayoutDirection = LayoutDirection.Vertical;
            m_MusicList.width = width - 34;
            m_MusicList.height = height - 60 - 20;
            m_MusicList.relativePosition = new Vector3(10, 60 + 10);
            m_MusicList.clipChildren = true;
            m_MusicList.autoLayoutPadding = new RectOffset(3, 3, 3, 3);
            m_MusicList.Show();
           

            initializeMusicListScroller();

            //UpdateMusicList();

            //panel.eventItemClicked += musicEntrySelected;
            //panel.eventItemDoubleClicked += musicEntryEnableDisable;

        }

        private bool IsFiltered(String entrytext)
        {
            if (!Filtered)
                return false;

            return !entrytext.ToLower().Contains(m_Filter.text.ToLower());
        }
       
    }
}
