using System;
using System.Linq;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using System.IO;
using System.Collections.Generic;
using CSLMusicMod.Helpers;

namespace CSLMusicMod.UI
{
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

        private bool m_SortAscending = true;

        private List<RadioContentInfo> m_CurrentContent = new List<RadioContentInfo>();

        private ModOptions m_ModOptionsInstance = ModOptions.Instance;

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

                Debug.Log("[CSLMusicMod] Creating icon atlases ...");
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
                        "Clear"
                    });  

            }
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            // Bring the radio panel to the front
            var radiopanel = Resources.FindObjectsOfTypeAll<RadioPanel>().FirstOrDefault();

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

                if (m_VolumeSlider.value / 100f != m_MusicAudioVolume.value)
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

            Dictionary<RadioContentInfo, String> entrytexts = new Dictionary<RadioContentInfo, string>();

            if(activechannel >= 0)
            {
                RadioChannelData channeldata = mgr.m_radioChannels[activechannel];
                RadioChannelInfo info = channeldata.Info;

                m_CurrentContent.Clear();

                // Only show supported content entries
                HashSet<RadioContentInfo.ContentType> supported_content = new HashSet<RadioContentInfo.ContentType>();

                foreach(var state in info.m_stateChain)
                {
                    supported_content.Add(state.m_contentType);
                }

                for(uint i = 0; i < PrefabCollection<RadioContentInfo>.PrefabCount(); ++i)
                {
                    var c = PrefabCollection<RadioContentInfo>.GetPrefab(i);

                    if(supported_content.Contains(c.m_contentType) && c.m_radioChannels.Contains(info))
                    {
                        entrytexts[c] = GetEntryTextFor(c);

                        if(!IsFiltered(entrytexts[c]))
                        {
                            m_CurrentContent.Add(c);
                        }
                    }
                }

                //Debug.Log(m_CurrentContent.Count + " entries ");
            }

            m_CurrentContent.Sort((RadioContentInfo x, RadioContentInfo y) =>
                {
                    if(m_SortAscending)
                    {
                        return entrytexts[x].CompareTo(entrytexts[y]);
                    }
                    else
                    {
                        return entrytexts[x].CompareTo(entrytexts[y]);
                    }
                });

            RefreshListWidget();
        }

        private String GetEntryTextFor(RadioContentInfo content)
        {
            String name = String.IsNullOrEmpty(content.m_displayName) ? content.name : content.m_displayName;
            name = "[" + content.m_contentType.ToString().Substring(0,2) + "] " + name;

            String id = content.m_folderName + "/" + content.m_fileName;
            if(m_ModOptionsInstance.DisabledContent.Contains(id)) // Use optimized access
            {
                name = "[X]" + name;
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

                m_VolumeSlider.eventValueChanged += delegate(UIComponent component, float value)
                {
                    if (this.m_IsDisposing)
                        return;

                    //I use x100 because it failed with 0..1?
                    value = value / 100f;

                    if (m_MusicAudioVolume.value != value)
                    {
                        m_MusicAudioVolume.value = value;                   
                    }
                };
            }       
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

                m_ButtonSortAscending.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
                {
                        m_SortAscending = true;
                        RebuildList();
                };
            }
            {
                m_ButtonSortDescending = AddUIComponent<UIButton>();
                m_ButtonSortDescending.width = 36;
                m_ButtonSortDescending.height = 36;
                m_ButtonSortDescending.relativePosition = new Vector3(130 + 40 * 2, 10);
                m_ButtonSortDescending.normalBgSprite = "GenericPanel";
                m_ButtonSortDescending.tooltip = "Sort ascending";

                m_ButtonSortDescending.atlas = m_IconAtlas;
                m_ButtonSortDescending.hoveredBgSprite = "OptionBaseFocused";
                m_ButtonSortDescending.pressedBgSprite = "OptionBasePressed";
                m_ButtonSortDescending.normalFgSprite = "SortDescending";

                m_ButtonSortDescending.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
                {
                        m_SortAscending = false;
                        RebuildList();
                };
            }
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
            var radiopanel = Resources.FindObjectsOfTypeAll<RadioPanel>().FirstOrDefault();
            radiopanel.HideRadio();
        }

        private void InitializeMusicList()
        {
            var panel = m_MusicList = AddUIComponent<UIListBox>();

            panel.width = width - 34;
            panel.height = height - 60 - 20;
            panel.relativePosition = new Vector3(10, 60 + 10);
            panel.textColor = new Color32(150, 150, 150, 255);
            panel.itemHover = "SubcategoriesPanel";
            panel.itemHeight = 32;
            panel.itemPadding = new RectOffset(0, 0, 4, 4);
            panel.tooltip = "";
         
            panel.Show();

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

                m_MusicList.scrollbar = scroller;

                scroller.isVisible = true;
            }

            //UpdateMusicList();

            panel.eventItemClicked += musicEntrySelected;
            panel.eventItemDoubleClicked += musicEntryEnableDisable;
           
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
