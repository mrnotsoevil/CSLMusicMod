using System;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLMusicMod
{
    public class MusicSettingsPanel : UIPanel
    {
        public CSLAudioWatcher AudioWatcher { get; set; }

        private bool _initialized;
        private UICheckButton _enable_Sky;
        private UICheckButton _enable_Bad;
        private UICheckButton _enable_Chirpy;
        private UICheckButton _enable_MusicWhileLoading;

        public MusicSettingsPanel()
        {
        }

        public override void Start()
        {
            base.Start();

            backgroundSprite = "MenuPanel2";
            wrapLayout = true;
          
            mkLabel("Music selection", 10, 50 + 5);
            _enable_Sky = mkCheckBox("Height dependent music", 30, 50 + 34);
            _enable_Bad = mkCheckBox("Mood dependent music", 30, 50 + 34 * 2);
            mkLabel("Tweaks", 10, 50 + 34 * 3 + 5);
            _enable_Chirpy = mkCheckBox("Use Chirpy", 30, 50 + 34 * 4);
            _enable_MusicWhileLoading = mkCheckBox("Music while loading", 30, 50 + 34 * 5);

            _initialized = true;

            //Add events
            _enable_Sky.checkStateChanged += delegate(UICheckButton sender, bool state)
            {
                if (CSLMusicModSettings.HeightDependentMusic != state)
                {
                    CSLMusicModSettings.HeightDependentMusic = state;
                    CSLMusicModSettings.SaveModSettings();
                }
            };
            _enable_Bad.checkStateChanged += delegate(UICheckButton sender, bool state)
            {
                if (CSLMusicModSettings.MoodDependentMusic != state)
                {
                    CSLMusicModSettings.MoodDependentMusic = state;
                    CSLMusicModSettings.SaveModSettings();
                }
            };
            _enable_Chirpy.checkStateChanged += delegate(UICheckButton sender, bool state)
            {
                if (CSLMusicModSettings.EnableChirper != state)
                {
                    CSLMusicModSettings.EnableChirper = state;
                    CSLMusicModSettings.SaveModSettings();
                }
            };
            _enable_MusicWhileLoading.checkStateChanged += delegate(UICheckButton sender, bool state)
            {
                if (CSLMusicModSettings.MusicWhileLoading != state)
                {
                    CSLMusicModSettings.MusicWhileLoading = state;
                    CSLMusicModSettings.SaveModSettings();
                }
            };
        }

        private UILabel mkLabel(String text, int x, int y)
        {
            var label = AddUIComponent<UILabel>();
            label.textColor = new Color32(200, 200, 200, 255);
            label.width = width;
            label.height = 32;
            label.relativePosition = new Vector3(x, y);
            label.isVisible = true;
            label.text = text;
            label.verticalAlignment = UIVerticalAlignment.Middle;

            return label;
        }

        private UICheckButton mkCheckBox(String text, int x, int y)
        {
            var button = AddUIComponent<UICheckButton>();
            button.relativePosition = new Vector3(x, y);
            button.text = text;
            button.width = width - 20 - 20;
            button.height = 32;
            button.normalBgSprite = "SubcategoriesPanel";
           
            button.isVisible = true;
            return button;
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            UpdateValues();
        }

        private void UpdateValues()
        {
            if (_initialized)
            {
                _enable_Sky.isChecked = CSLMusicModSettings.HeightDependentMusic;
                _enable_Bad.isChecked = CSLMusicModSettings.MoodDependentMusic;
                _enable_Chirpy.isChecked = CSLMusicModSettings.EnableChirper;
                _enable_MusicWhileLoading.isChecked = CSLMusicModSettings.MusicWhileLoading;
            }
        }
    }
}

