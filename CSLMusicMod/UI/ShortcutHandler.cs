using System;
using System.Linq;
using UnityEngine;
using CSLMusicMod.Helpers;

namespace CSLMusicMod
{
    public class ShortcutHandler : MonoBehaviour
    {
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

        public ShortcutHandler()
        {
        }

        public void Update()
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
    }
}

