using System;
using ColossalFramework.UI;
using UnityEngine;
using System.Collections.Generic;

namespace CSLMusicMod.UI
{
    public class UIKeyBindingButton : UIButton
    {
        static private List<KeyCode> validKeyCodes = obtainValidKeys();
        public delegate void KeyCodeHandler(UIKeyBindingButton sender,KeyCode assignedKey);

        public event KeyCodeHandler eventAssignedKeyChanged;

        private KeyCode _assignedKey;
        public KeyCode AssignedKey
        {
            get
            {
                return _assignedKey;
            }
            set
            {
                _assignedKey = value;

                if (eventAssignedKeyChanged != null)
                {
                    eventAssignedKeyChanged(this, value);
                }
            }
        }

        public static UIKeyBindingButton CurrentListeningButton = null;

        public UIKeyBindingButton()
        {
        }

        public override void Start()
        {
            base.Start();

            eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
            {
                if (CurrentListeningButton == this)
                    CurrentListeningButton = null;
                else
                    CurrentListeningButton = this;
            };
        }

        public override void Update()
        {
            base.Update();

            if (!isVisible)
            {
                if (CurrentListeningButton == this)
                {
                    CurrentListeningButton = null;
                }

                return;
            }

            if (CurrentListeningButton == this)
            {
                text = "Press key (Click to cancel) ...";
            }
            else
            {
                text = AssignedKey.ToString();
            }

            //This logic is separate from ui logic!
            if (CurrentListeningButton == this)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CurrentListeningButton = null;
                    return;
                }

                foreach (KeyCode key in validKeyCodes)
                {
                    if (Input.GetKeyDown(key))
                    {
                        AssignedKey = key;
                        CurrentListeningButton = null;
                        return;
                    }
                }
            }
        }

        private static List<KeyCode> obtainValidKeys()
        {
            List<KeyCode> result = new List<KeyCode>();

            foreach (KeyCode code in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
            {
                if (code == KeyCode.Escape)
                    continue;
                if (code.ToString().ToLower().Contains("mouse"))
                    continue;

                result.Add(code);
            }

            return result;
        }
    }
}

