using System;
using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace CSLMusicMod
{
    public class MusicUI : MonoBehaviour
    {
        private bool _key_NextTrack_IsDown = false;
        private bool _key_MusicSettings_IsDown = false;
        private MusicListPanel _current_Settings_Panel;

        private static CSLMusicChirperMessage _last_Music_Switch_Message;

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
            UIView v = UIView.GetAView();
            _current_Settings_Panel = (MusicListPanel)v.AddUIComponent(typeof(MusicListPanel));
            _current_Settings_Panel.AudioWatcher = AudioWatcher;
            _current_Settings_Panel.Hide();
        }

        public void Update()
        {
            //While setting key bindings, do nothing
            if (UIKeyBindingButton.CurrentListeningButton != null)
            {
                _key_MusicSettings_IsDown = false;
                _key_NextTrack_IsDown = false;
                return;
            }

            //Next track
            if (CSLMusicModSettings.Key_NextTrack != KeyCode.None)
            {
                if (Input.GetKeyDown(CSLMusicModSettings.Key_NextTrack))
                {
                    _key_NextTrack_IsDown = true;
                }
                else if (Input.GetKeyUp(CSLMusicModSettings.Key_NextTrack) && _key_NextTrack_IsDown)
                {
                    _key_NextTrack_IsDown = false;

                    AudioWatcher.RequestSwitchMusic(true);
                }

               
            }

            //Settings panel
            if (CSLMusicModSettings.Key_Settings != KeyCode.None)
            {
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
        }

        public void OnDestroy()
        {
            MonoBehaviour.Destroy(_current_Settings_Panel);
        }

        /**
         * Dequeue a chrip using reflection
         * */
        public static void DequeueChirp(MessageBase msg)
        {
            //I don't trust it (o .o ) ( o. o) ( ^.^ )
            try
            {
                if(MessageManager.instance.m_properties != null)
                {
                    Queue<MessageBase> q = ReflectionHelper.GetPrivateField<Queue<MessageBase>>(MessageManager.instance, "m_messageQueue");

                    //Inplace requeuing
                    int c = q.Count;
                    for(int i = 0; i < c; i++)
                    {
                        MessageBase m = q.Dequeue();

                        if(m != msg)
                        {
                            q.Enqueue(m);
                        }
                    }
                }
            }
            catch(Exception)
            {
            }
        }

        public static void ChirpNowPlaying(CSLCustomMusicEntry music)
        {
            if (CSLMusicModSettings.EnableChirper)
            {
                CSLMusicChirperMessage msg = CSLMusicChirperMessage.CreateNowPlayingMessage(music);

                MessageManager.instance.QueueMessage(msg);

                //Remove old message
                if (_last_Music_Switch_Message != null)
                {
                    DequeueChirp(_last_Music_Switch_Message);
                }

                _last_Music_Switch_Message = msg;

            }
        }

        public static void ChirpWelcome()
        {
            if (CSLMusicModSettings.EnableChirper)
            {
                MessageManager.instance.QueueMessage(CSLMusicChirperMessage.CreateWelcomeMessage());
            }
        }

        public static void ChirpConverterError()
        {
            if (CSLMusicModSettings.EnableChirper)
            {
                MessageBase msg = CSLMusicChirperMessage.CreateConverterErrorMessage();

                if (msg != null)
                {
                    Debug.Log("[CSLMusic][Chirpy] Sending conversion error report");
                    MessageManager.instance.QueueMessage(msg);
                }
                else
                {
                    Debug.Log("[CSLMusic][Chirpy] No conversion error reported");
                }
            }
        }
    }
}

