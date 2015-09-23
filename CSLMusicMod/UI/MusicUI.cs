using System;
using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace CSLMusicMod.UI
{
    public class MusicUI : MonoBehaviour
    {
        private bool _key_NextTrack_IsDown = false;
        private bool _key_MusicSettings_IsDown = false;
        private UIMusicListPanel _current_Settings_Panel;

        //private static CSLMusicChirperMessage _last_Music_Switch_Message;

        private CSLAudioWatcher AudioWatcher
        {
            get
            {
                return this.gameObject.GetComponent<MusicInjector>().AudioWatcher;
            }
        }       

        public SettingsManager.Options ModOptions
        {
            get
            {
                return gameObject.GetComponent<SettingsManager>().ModOptions;
            }
        }

        public MusicUI()
        {
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            //Create ui
            UIView v = UIView.GetAView();
            _current_Settings_Panel = (UIMusicListPanel)v.AddUIComponent(typeof(UIMusicListPanel));           
            _current_Settings_Panel.Hide();

            _current_Settings_Panel.AudioWatcher = AudioWatcher;
            _current_Settings_Panel.SettingsManager = gameObject.GetComponent<SettingsManager>();
            _current_Settings_Panel.MusicManager = gameObject.GetComponent<MusicManager>();
        }

        public void Update()
        {
            //While setting key bindings, do nothing
            //If colossal ui has focus do nothing
            if (UIKeyBindingButton.CurrentListeningButton != null || UIView.HasInputFocus())
            {
                _key_MusicSettings_IsDown = false;
                _key_NextTrack_IsDown = false;
                return;
            }

            //Next track
            if (ModOptions.Key_NextTrack != KeyCode.None)
            {
                if (Input.GetKeyDown(ModOptions.Key_NextTrack))
                {
                    _key_NextTrack_IsDown = true;
                }
                else if (Input.GetKeyUp(ModOptions.Key_NextTrack) && _key_NextTrack_IsDown)
                {
                    _key_NextTrack_IsDown = false;

                    AudioWatcher.RequestSwitchMusic();
                }

               
            }

            //Settings panel
            if (ModOptions.Key_Settings != KeyCode.None)
            {
                if (Input.GetKeyDown(ModOptions.Key_Settings))
                {
                    _key_MusicSettings_IsDown = true;
                }
                else if (Input.GetKeyUp(ModOptions.Key_Settings) && _key_MusicSettings_IsDown)
                {
                    _key_MusicSettings_IsDown = false;

                    if (_current_Settings_Panel.isVisible)
                        _current_Settings_Panel.Hide();
                    else
                        _current_Settings_Panel.Show();
                }
            }
        }

        public void OnDestroy()
        {
            MonoBehaviour.Destroy(_current_Settings_Panel);
        }       
    }
}

