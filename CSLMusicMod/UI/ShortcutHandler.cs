using System;
using System.Linq;
using UnityEngine;
using CSLMusicMod.Helpers;
using ColossalFramework.UI;

namespace CSLMusicMod
{
    public class ShortcutHandler : MonoBehaviour
    {
        // Shortcut key variables
        private bool m_OpenPanelKey_IsDown = false;
        private bool m_NextTrackKey_IsDown = false;
        private bool m_NextStationKey_IsDown = false;

        private bool m_ModifierCtrl = false;
        private bool m_ModifierShift = false;
        private bool m_ModiferAlt = false;

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

        public void Start()
        {
            Debug.Log(ModOptions.Instance.ShortcutNextTrack);
            Debug.Log(ModOptions.Instance.ShortcutNextStation);
            Debug.Log(ModOptions.Instance.ShortcutOpenRadioPanel);
        }

        private bool ShortcutDown(ModOptions.Shortcut shortcut)
        {
            if (shortcut.Key == KeyCode.None)
                return false;
            
            return (Input.GetKeyDown(shortcut.Key) &&
            (shortcut.ModifierControl == m_ModifierCtrl) &&
            (shortcut.ModifierShift == m_ModifierShift) &&
            (shortcut.ModifierAlt == m_ModiferAlt));
        }

        private bool ShortcutUp(ModOptions.Shortcut shortcut)
        {
            if (shortcut.Key == KeyCode.None)
                return true;

            return Input.GetKeyUp(shortcut.Key);
        }

        public void Update()
        {
            // Check if some other UI has the focus
            if(UIView.HasInputFocus())
            {
                m_NextTrackKey_IsDown = false;
                m_OpenPanelKey_IsDown = false;
                m_NextStationKey_IsDown = false;
                return;
            }

            m_ModifierCtrl = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            m_ModifierShift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            m_ModiferAlt = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));         

            //Next track
            if(ShortcutDown(ModOptions.Instance.ShortcutNextTrack))
            {
                m_NextTrackKey_IsDown = true;
            }
            else if(m_NextTrackKey_IsDown && ShortcutUp(ModOptions.Instance.ShortcutNextTrack))
            {
                m_NextTrackKey_IsDown = false;
                Debug.Log("[CSLMusic] Pressed shortcut for next track");
                AudioManagerHelper.NextTrack();
            }

            //Next station
            if(ShortcutDown(ModOptions.Instance.ShortcutNextStation))
            {
                m_NextStationKey_IsDown = true;
            }
            else if(m_NextStationKey_IsDown && ShortcutUp(ModOptions.Instance.ShortcutNextStation))
            {
                Debug.Log("[CSLMusic] Pressed shortcut for next station");
                m_NextStationKey_IsDown = false;
                AudioManagerHelper.NextStation();
            }

            //Panel
            if(ShortcutDown(ModOptions.Instance.ShortcutOpenRadioPanel))
            {
                m_OpenPanelKey_IsDown = true;
            }
            else if(m_OpenPanelKey_IsDown && ShortcutUp(ModOptions.Instance.ShortcutOpenRadioPanel))
            {                
                m_OpenPanelKey_IsDown = false;
                Debug.Log("[CSLMusic] Pressed shortcut for hide/show panel");

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

