using System;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework;

namespace CSLMusicMod
{
    public class UIMusicSettingsPanel : UIPanel
    {
        public CSLAudioWatcher AudioWatcher { get; set; }

        private bool _initialized;
        private UICheckButton _enable_Sky;
        private UICheckButton _enable_Bad;
        //private UICheckButton _enable_Chirpy;
        //private UICheckButton _enable_MusicWhileLoading;
        private UICheckButton _enable_Playlist_random;
        //private UIKeyBindingButton _nextTrackBinding;
        //private UIKeyBindingButton _openPanelBinding;
        //+Feature: Adjust music volume
        private SavedFloat _MusicAudioVolume = new SavedFloat(Settings.musicAudioVolume, Settings.gameSettingsFile, DefaultSettings.musicAudioVolume, true);
        private UISlider _MusicVolumeSlider;

        private SettingsManager SettingsManager
        {
            get
            {
                return gameObject.GetComponent<SettingsManager>();
            }
        }

        public UIMusicSettingsPanel()
        {
        }

        public override void Start()
        {
            base.Start();

            backgroundSprite = "MenuPanel2";
            wrapLayout = true;

            //+Feature: add volume slider
            {
                _MusicVolumeSlider = AddUIComponent<UISlider>();
                _MusicVolumeSlider.relativePosition = new Vector3(15, 24 / 2 + 10 / 2);
                _MusicVolumeSlider.width = width - _MusicVolumeSlider.relativePosition.x * 2;
                _MusicVolumeSlider.height = 10;
                _MusicVolumeSlider.backgroundSprite = "TextFieldPanel";
                _MusicVolumeSlider.minValue = 0;
                _MusicVolumeSlider.maxValue = 100;
                _MusicVolumeSlider.tooltip = "Drag to change the music volume";

                UIPanel thumb = _MusicVolumeSlider.AddUIComponent<UIPanel>();
                thumb.width = 15;
                thumb.height = 15;
                thumb.backgroundSprite = "TextFieldPanelHovered";

                UIPanel fill = _MusicVolumeSlider.AddUIComponent<UIPanel>();
                fill.backgroundSprite = "TextFieldPanel";
                fill.color = new Color32(79, 210, 233, 255);
                _MusicVolumeSlider.fillIndicatorObject = fill;

                _MusicVolumeSlider.thumbObject = thumb;
            }

            //Add checkbuttons and more          
            mkLabel("Select music by", 10, 50 + 5);
            _enable_Sky = mkCheckBox("Height", 30 + 120 + 10, 50 + 34, 120);
            _enable_Bad = mkCheckBox("Mood", 30, 50 + 34, 120);
            mkLabel("Playlist", 10, 50 + 34 * 2 + 5);
            _enable_Playlist_random = mkCheckBox("Play tracks randomly", 30, 50 + 34 * 3);
            mkLabel("Tweaks", 10, 50 + 34 * 4 + 5);
            //_enable_Chirpy = mkCheckBox("Use Chirpy", 30, 50 + 34 * 5);
            //_enable_MusicWhileLoading = mkCheckBox("Music while loading", 30, 50 + 34 * 6);

            //Key bindings
            //mkLabel("Key bindings", 10, 50 + 34 * 7 + 5);

            //mkLabel("Next track", 30, 50 + 34 * 8 + 5);
            //_nextTrackBinding = mkKeyBindButton(150, 50 + 34 * 8, width - 150 - 100);

            //mkLabel("Key bindings", 30, 50 + 34 * 9 + 5);
            //_openPanelBinding = mkKeyBindButton(150, 50 + 34 * 9, width - 150 - 100);


            //Add tooltips
            _enable_Sky.tooltip = "Change music if you float high enough above your city";
            _enable_Bad.tooltip = "Change music depending on your popularity";
            //_enable_Chirpy.tooltip = "Great leader Chirpy will tell you which music is playing";
           // _enable_MusicWhileLoading.tooltip = "Play menu music while loading. Useful if music stutters while loading";
            _enable_Playlist_random.tooltip = "Select tracks to play randomly";

            _initialized = true;

            //Add events
            _enable_Sky.eventCheckStateChanged += delegate(UICheckButton sender, bool state)
            {
                if (SettingsManager.ModOptions.HeightDependentMusic != state)
                {
                    SettingsManager.ModOptions.HeightDependentMusic = state;
                    SettingsManager.SaveModSettings();
                }
            };
            _enable_Bad.eventCheckStateChanged += delegate(UICheckButton sender, bool state)
            {
                if (SettingsManager.ModOptions.MoodDependentMusic != state)
                {
                    SettingsManager.ModOptions.MoodDependentMusic = state;
                    SettingsManager.SaveModSettings();
                }
            };
            _enable_Playlist_random.eventCheckStateChanged += delegate(UICheckButton sender, bool state)
            {
                if (SettingsManager.ModOptions.RandomTrackSelection != state)
                {
                    SettingsManager.ModOptions.RandomTrackSelection = state;
                    SettingsManager.SaveModSettings();
                }
            };
            /*_enable_Chirpy.eventCheckStateChanged += delegate(UICheckButton sender, bool state)
            {
                if (CSLMusicModSettings.EnableChirper != state)
                {
                    CSLMusicModSettings.EnableChirper = state;
                    CSLMusicModSettings.SaveModSettings();
                }
            };*/
            /*_enable_MusicWhileLoading.eventCheckStateChanged += delegate(UICheckButton sender, bool state)
            {
                if (SettingsManager.ModOptions.MusicWhileLoading != state)
                {
                    SettingsManager.ModOptions.MusicWhileLoading = state;
                    SettingsManager.SaveModSettings();
                }
            };*/
            /*_nextTrackBinding.eventAssignedKeyChanged += delegate(UIKeyBindingButton sender, KeyCode assignedKey)
            {
                if (SettingsManager.ModOptions.Key_NextTrack != assignedKey)
                {
                    SettingsManager.ModOptions.Key_NextTrack = assignedKey;
                    SettingsManager.SaveModSettings();
                }
            };*/
            /*_openPanelBinding.eventAssignedKeyChanged += delegate(UIKeyBindingButton sender, KeyCode assignedKey)
            {
                if (SettingsManager.ModOptions.Key_Settings != assignedKey)
                {
                    SettingsManager.ModOptions.Key_Settings = assignedKey;
                    SettingsManager.SaveModSettings();
                }
            };*/
       
            _MusicVolumeSlider.eventValueChanged += delegate(UIComponent component, float value)
            {
                //I use x100 because it failed with 0..1?
                value = value / 100f;

                if (_MusicAudioVolume.value != value)
                {
                    _MusicAudioVolume.value = value;                   
                }
            };
        }

        private UILabel mkLabel(String text, int x, int y)
        {
            var label = AddUIComponent<UILabel>();
            label.textColor = new Color32(200, 200, 200, 255);
            label.width = width - x - 10;
            label.height = 32;
            label.relativePosition = new Vector3(x, y);
            label.isVisible = true;
            label.text = text;
            label.verticalAlignment = UIVerticalAlignment.Middle;

            return label;
        }

        private UICheckButton mkCheckBox(String text, int x, int y, int width)
        {
            var button = AddUIComponent<UICheckButton>();
            button.relativePosition = new Vector3(x, y);
            button.text = text;
            button.width = width;
            button.height = 32;
            button.normalBgSprite = "SubcategoriesPanel";

            button.isVisible = true;
            return button;
        }

        private UICheckButton mkCheckBox(String text, int x, int y)
        {
            var button = AddUIComponent<UICheckButton>();
            button.relativePosition = new Vector3(x, y);
            button.text = text;
            button.width = width - x - 10;
            button.height = 32;
            button.normalBgSprite = "SubcategoriesPanel";
           
            button.isVisible = true;
            return button;
        }

        private UIKeyBindingButton mkKeyBindButton(int x, int y, float width)
        {
            var button = AddUIComponent<UIKeyBindingButton>();
            button.relativePosition = new Vector3(x, y);
            button.width = width;
            button.height = 32;
            button.normalBgSprite = "SubcategoriesPanel";
            button.hoveredColor = new Color32(128, 128, 128, 255);
            button.focusedColor = new Color32(0, 100, 180, 255);

            button.isVisible = true;
            return button;
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            UpdateValues();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            //To be sure disable this!
            UIKeyBindingButton.CurrentListeningButton = null;
        }

        public override void Update()
        {
            base.Update();

            if (isVisible)
            {
                //Makes the slider a little bit jitterish, but successfuly synchronizes volumes from outer (eg the options menu)
                if (_initialized)
                {
                    if (_MusicVolumeSlider.value / 100f != _MusicAudioVolume.value)
                    {
                        _MusicVolumeSlider.value = _MusicAudioVolume.value * 100f;
                    }
                }
            }
        }

        private void UpdateValues()
        {
            if (_initialized)
            {
                _enable_Sky.isChecked = SettingsManager.ModOptions.HeightDependentMusic;
                _enable_Bad.isChecked = SettingsManager.ModOptions.MoodDependentMusic;
                _enable_Playlist_random.isChecked = SettingsManager.ModOptions.RandomTrackSelection;
                //_enable_Chirpy.isChecked = SettingsManager.ModOptions.EnableChirper;
                //_enable_MusicWhileLoading.isChecked = SettingsManager.ModOptions.MusicWhileLoading;
                //_nextTrackBinding.AssignedKey = SettingsManager.ModOptions.Key_NextTrack;
                //_openPanelBinding.AssignedKey = SettingsManager.ModOptions.Key_Settings;
                _MusicVolumeSlider.value = _MusicAudioVolume.value * 100f; //I use x100 because it failed with 0..1?

                //Debug.Log("********** vol:" + _MusicAudioVolume.value);
            }
        }
    }
}

