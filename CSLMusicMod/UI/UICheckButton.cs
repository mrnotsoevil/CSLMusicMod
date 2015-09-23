using System;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLMusicMod.UI
{
    public class UICheckButton : UIButton
    {
        private bool _checked;

        public delegate void CheckStateHandler(UICheckButton sender, bool state);
        public event CheckStateHandler eventCheckStateChanged;

        public bool isChecked
        {
            get
            {
                return _checked;
            }
            set
            {
                _checked = value;

                if (eventCheckStateChanged != null)
                    eventCheckStateChanged(this, value);
            }
        }

        public Color32 checkedColor { get; set; }

        public Color32 uncheckedColor { get; set; }

        public Color32 checkedHoverColor { get; set; }

        public Color32 uncheckedHoverColor { get; set; }

        public UICheckButton()
        {
            checkedColor = new Color32(0, 100, 180, 255);
            uncheckedColor = new Color32(128, 128, 128, 128);
            checkedHoverColor = new Color32(0, 128, 208, 255);
            uncheckedHoverColor = new Color32(200, 200, 200, 255);

            this.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
            {
                isChecked = !isChecked;
            };
        }

        public override void Update()
        {
            base.Update();

            if (_checked)
            {
                color = checkedColor;
                hoveredColor = checkedHoverColor;
                focusedColor = checkedColor;
            }
            else
            {
                color = uncheckedColor;
                hoveredColor = uncheckedHoverColor;
                focusedColor = uncheckedColor;
            }
        }
    }
}

