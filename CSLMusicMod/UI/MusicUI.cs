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

        private UIView _uiview;
        private bool _key_NextTrack_IsDown = false;
        private bool _key_MusicSettings_IsDown = false;
        private UIMusicListPanel _current_Settings_Panel;
        private UICheckButton _toolbar_Button;

        private bool _toolbar_Button_dragging = false;
        private Vector3 _toolbar_Button_dragging_pos = Vector2.zero;
        private int _toolbar_Button_MouseDown_Timer = -1;

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
            _uiview = v;
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
            _toolbar_Button.relativePosition = new Vector3(screenResolution.x - _toolbar_Button.width - 10 - 20 - 40, screenResolution.y - _toolbar_Button.height / 2 - 120 + 7);
        
            _toolbar_Button.eventClick += delegate
            {
                _toolbar_Button_MouseDown_Timer = -1;
                _current_Settings_Panel.isVisible = !_current_Settings_Panel.isVisible;
            };

            //Drag,drop
            _toolbar_Button.eventMouseDown += (component, eventParam) =>
            {
                _toolbar_Button_MouseDown_Timer = 60;
            };
            _toolbar_Button.eventMouseUp += (component, eventParam) =>
            {
                if (_toolbar_Button_dragging)
                {                       
                    gameObject.GetComponent<SettingsManager>().SaveModSettings();
                    _toolbar_Button_dragging = false;
                }

                _toolbar_Button_dragging = false;
                _toolbar_Button_MouseDown_Timer = -1;
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

        private Vector3 ObtainMousePosition()
        {
            var mousepos = Input.mousePosition;
            var x = mousepos.x;
            var y = mousepos.y;

            //transform
            var screen = _uiview.GetScreenResolution();
                     
            x = screen.x * (x / Screen.width);
            y = screen.y * ((Screen.height - y) / Screen.height);

            var pos = new Vector2(x, y);

            Debug.Log("[CSLMusicMod] pp" + pos);

            return pos;
        }

        private void UpdateToolbarButton()
        {
            var visible = _toolbar_Button.isVisible = ModOptions.ShowToolbarButton;

            if (visible)
            {
                //Color while dragging
                if (_toolbar_Button_dragging)
                    _toolbar_Button.pressedColor = Color.red;
                else
                    _toolbar_Button.pressedColor = Color.white;

                //Deactivate dragging in certain situations
                if ((_toolbar_Button_MouseDown_Timer != -1 || _toolbar_Button_dragging) && (Input.GetKeyDown(KeyCode.Escape) || !_toolbar_Button.containsMouse))
                {
                    Debug.Log("[CSLMusic:ToolbarButton] Disabling all dragging ...");

                    gameObject.GetComponent<SettingsManager>().SaveModSettings();
                    _toolbar_Button_MouseDown_Timer = -1;
                    _toolbar_Button_dragging = false;
                }

                if (!ModOptions.FixateToolbarButton)
                {
                    // Activate dragging after certain mouse down time
                    if (_toolbar_Button_MouseDown_Timer != -1)
                    {
                        Debug.Log("[CSLMusic:ToolbarButton] Timer: " + _toolbar_Button_MouseDown_Timer);

                        if (_toolbar_Button_MouseDown_Timer > 0)
                            _toolbar_Button_MouseDown_Timer--;
                        else
                        {
                            //Find correct "grabbing pixel"
                            Vector3 mouse = ObtainMousePosition();
                            Vector3 pos = _toolbar_Button.relativePosition;

                            _toolbar_Button_dragging_pos = mouse - pos;

                            //todo
                            //_toolbar_Button_dragging_pos = Vector3.zero;

                            _toolbar_Button_dragging = true;

                            _toolbar_Button_MouseDown_Timer = -1;

                            Debug.Log("[CSLMusic:ToolbarButton] Dragging pos: " + _toolbar_Button_dragging_pos);
                        }
                    }

                    //While dragging move button
                    if (_toolbar_Button_dragging)
                    {
                        var mouse = ObtainMousePosition();

                        ModOptions.ToolbarButtonX = (mouse.x - _toolbar_Button_dragging_pos.x);
                        ModOptions.ToolbarButtonY = (mouse.y - _toolbar_Button_dragging_pos.y);

                        Debug.Log("[CSLMusic:ToolbarButton] Dragging: " + ModOptions.ToolbarButtonX + " # " + ModOptions.ToolbarButtonY);
                    }
                }
            }

            if (visible)
            {

                var x = ModOptions.ToolbarButtonX;
                var y = ModOptions.ToolbarButtonY;

                if (x <= -1 || y <= -1)
                {
                    var screenResolution = _uiview.GetScreenResolution();
                    _toolbar_Button.relativePosition = new Vector3(screenResolution.x - _toolbar_Button.width - 10 - 20 - 40, screenResolution.y - _toolbar_Button.height / 2 - 120 + 7);
                
                    //Debug.Log("[CSLMusic:ToolbarButton] Toggle back to " + _toolbar_Button.relativePosition);
                }
                else
                {
                    _toolbar_Button.relativePosition = new Vector3(x, y);
                }
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

            UpdateToolbarButton();
        }

        public void OnDestroy()
        {
            _toolbar_Button_dragging = false;
            MonoBehaviour.Destroy(_current_Settings_Panel);
        }
    }
}

