using System;
using UnityEngine;
using ColossalFramework.UI;

namespace CSLMusicMod
{
    public class MusicUI : MonoBehaviour
    {
        private bool _key_NextTrack_IsDown = false;
        private bool _key_MusicSettings_IsDown = false;
        private MusicSettingsPanel _current_Settings_Panel;

        private CSLAudioWatcher AudioWatcher
        {
            get
            {
                return this.gameObject.GetComponent<MusicInjector>().AudioWatcher;
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
            UIView v = UIView.GetAView ();
            _current_Settings_Panel = (MusicSettingsPanel)v.AddUIComponent (typeof(MusicSettingsPanel));
            _current_Settings_Panel.AudioWatcher = AudioWatcher;
            _current_Settings_Panel.Hide();
        }

        public void Update()
        {
            //Next track
            if (Input.GetKeyDown(CSLMusicModSettings.Key_NextTrack))
            {
                _key_NextTrack_IsDown = true;
            }
            else if (Input.GetKeyUp(CSLMusicModSettings.Key_NextTrack) && _key_NextTrack_IsDown)
            {
                _key_NextTrack_IsDown = false;

                AudioWatcher.RequestSwitchMusic();
            }

            //Settings panel
            if (Input.GetKeyDown(CSLMusicModSettings.Key_Settings))
            {
                _key_MusicSettings_IsDown = true;
            }
            else if (Input.GetKeyUp(CSLMusicModSettings.Key_Settings) && _key_MusicSettings_IsDown)
            {
                _key_MusicSettings_IsDown = false;

                if (_current_Settings_Panel.isVisible)
                    _current_Settings_Panel.Hide();
                else
                    _current_Settings_Panel.Show();
            }
        }

        public void OnDestroy()
        {
            MonoBehaviour.Destroy(_current_Settings_Panel);
        }

        public static void ChirpNowPlaying(CSLCustomMusicEntry music)
        {
            if (CSLMusicModSettings.EnableChirper)
            {
                MessageManager.instance.QueueMessage(CSLMusicChirperMessage.CreateNowPlayingMessage(music));
            }
        }

        public static void ChirpWelcome()
        {
            if (CSLMusicModSettings.EnableChirper)
            {
                MessageManager.instance.QueueMessage(CSLMusicChirperMessage.CreateWelcomeMessage());
            }
        }
    }
}

