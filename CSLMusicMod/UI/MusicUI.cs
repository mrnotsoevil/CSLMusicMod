using System;
using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace CSLMusicMod.UI
{
    public class MusicUI : MonoBehaviour
    {
        //Texture atlas
        private UITextureAtlas _atlas;

        private bool _key_NextTrack_IsDown = false;
        private bool _key_MusicSettings_IsDown = false;
        private UIMusicListPanel _current_Settings_Panel;
        private UICheckButton _toolbar_Button;

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
            InitAtlases();

            //Create ui
            UIView v = UIView.GetAView();
            AddListPanel(v);
            AddToolbarButton(v);
        }

        private void AddToolbarButton(UIView  view)
        {
            _toolbar_Button = (UICheckButton)view.AddUIComponent(typeof(UICheckButton));
            _toolbar_Button.atlas = _atlas;
            _toolbar_Button.normalBgSprite = "OptionBase";
            _toolbar_Button.hoveredBgSprite = "OptionBaseFocused";
            _toolbar_Button.pressedBgSprite = "OptionBasePressed";
            _toolbar_Button.normalFgSprite = "Music";
            _toolbar_Button.width = 36;
            _toolbar_Button.height = 36;
            Vector2 screenResolution = view.GetScreenResolution();
            _toolbar_Button.relativePosition = new Vector3(screenResolution.x - _toolbar_Button.width - 10 - 20, screenResolution.y - _toolbar_Button.height / 2 - 120 + 7);
        
            _toolbar_Button.eventClick += delegate
            {
                    _current_Settings_Panel.isVisible = !_current_Settings_Panel.isVisible;
            };
        }

        private void AddListPanel(UIView view)
        {
            _current_Settings_Panel = (UIMusicListPanel)view.AddUIComponent(typeof(UIMusicListPanel));           
            _current_Settings_Panel.Hide();

            _current_Settings_Panel.AudioWatcher = AudioWatcher;
            _current_Settings_Panel.SettingsManager = gameObject.GetComponent<SettingsManager>();
            _current_Settings_Panel.MusicManager = gameObject.GetComponent<MusicManager>();
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
                        "SortDescending"
                    });  

            }
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

            //Settings panel <-> toobar button
            if (_toolbar_Button != null && _current_Settings_Panel != null)
            {
                _toolbar_Button.isChecked = _current_Settings_Panel.isVisible;
            }

            //Toolbar button visibility
            _toolbar_Button.isVisible = ModOptions.ShowToolbarButton;
        }

        public void OnDestroy()
        {
            MonoBehaviour.Destroy(_current_Settings_Panel);
        }       
    }
}

