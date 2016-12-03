using System;
using System.Linq;
using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.Reflection;
using CSLMusicMod.Helpers;

namespace CSLMusicMod.UI
{
    public class MusicUI : MonoBehaviour
    {  
        private UIMusicListPanel m_ListPanel;

        private bool m_Initialized = false;

        // Shortcut key variables
        private bool m_OpenPanelKey_IsDown = false;
        private bool m_NextTrackKey_IsDown = false;

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

        public MusicUI()
        {
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Start()
        {

        }
      
        private void AddListPanel(UIView view)
        {
            m_ListPanel = (UIMusicListPanel)view.AddUIComponent(typeof(UIMusicListPanel));           
            m_ListPanel.Hide();
        }

        private void Initialize()
        {
            //Create ui
            UIView v = UIView.GetAView();
            AddListPanel(v);

            m_Initialized = true;
        }

        public void Update()
        {
            if(m_Initialized)
            {
                UpdateShortcuts();
                UpdateListPanelVisibility();
            }
            else
            {
                try
                {
                    Initialize();
                }
                catch(Exception)
                {
                    
                }
            }

        }

        private void UpdateListPanelVisibility()
        {
            if(ModOptions.Instance.EnableCustomUI)
            {
                var radiopanel = CurrentRadioPanel;

                if(radiopanel != null && m_ListPanel != null)
                {
                    m_ListPanel.isVisible = ReflectionHelper.GetPrivateField<bool>(radiopanel, "m_isVisible");
                }
            }
        }

        private void UpdateShortcuts()
        {
            //Next track
            if (ModOptions.Instance.KeyNextTrack != KeyCode.None)
            {
                if (Input.GetKeyDown(ModOptions.Instance.KeyNextTrack))
                {
                    m_NextTrackKey_IsDown = true;
                }
                else if (Input.GetKeyUp(ModOptions.Instance.KeyNextTrack) && m_NextTrackKey_IsDown)
                {
                    m_NextTrackKey_IsDown = false;

                    AudioManagerHelper.NextTrack();
                }
            }

            //Settings panel
            if (ModOptions.Instance.KeyOpenMusicPanel != KeyCode.None)
            {
                if (Input.GetKeyDown(ModOptions.Instance.KeyOpenMusicPanel))
                {
                    m_OpenPanelKey_IsDown = true;
                }
                else if (Input.GetKeyUp(ModOptions.Instance.KeyOpenMusicPanel) && m_OpenPanelKey_IsDown)
                {
                    m_OpenPanelKey_IsDown = false;

                    var radiopanel = CurrentRadioPanel;
                    if(radiopanel != null)
                    {
                        var visible = ReflectionHelper.GetPrivateField<bool>(radiopanel, "m_isVisible");

                        if (visible)
                            radiopanel.HideRadio();
                        else
                            radiopanel.ShowRadio();
                    }
                }
            }
        }

        public void OnDestroy()
        {
            if(m_ListPanel != null)
                MonoBehaviour.Destroy(m_ListPanel);
        }
    }
}

