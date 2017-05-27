using System;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLMusicMod
{
    public class UIMusicListEntry : UIPanel
    {
        private UISprite m_Icon;
        private UILabel m_Name;
        private UIButton m_MoveUp;
        private UIButton m_MoveDown;
        private UIButton m_Disable;

        private bool m_ContentEnabled = true;
        private bool m_IsHovered = false;

        public UITextureAtlas m_IconAtlas;

        private string m_valueName;
        private string m_valueIcon;

        private bool m_ButtonsInitialized = false;

        public UIMusicListEntry()
        {
        }

        public override void Start()
        {
            this.backgroundSprite = "TextFieldPanel";
            this.color = new Color32(60, 60, 60, 255);
            this.canFocus = false;
            initializeIcon();
            initializeSongLabel();
            UpdateColor();

            if(m_valueName != null)
            {
                m_Name.text = StringHelper.CutoffAfter(m_valueName, m_Name.ObtainRenderer(), width - 36 * 5);
            }
            if(m_valueIcon != null)
            {
                m_Icon.spriteName = m_valueIcon;
            }
        }

        private void initializeButtonsIfNeeded()
        {
            if(!m_ButtonsInitialized)
            {
                initializeMoveUpButton();
                initializeMoveDownButton();
                initializeDisableButton();
                m_ButtonsInitialized = true;
            }
        }

        public void SetName(string name)
        {
            if (m_Name == null)
                m_valueName = name;
            else
                m_Name.text = StringHelper.CutoffAfter(name, m_Name.ObtainRenderer(), width);
        }

        public void SetContentType(RadioContentInfo.ContentType type)
        {
            switch(type)
            {
                case RadioContentInfo.ContentType.Music:
                    m_valueIcon = "Music";
                    break;
                case RadioContentInfo.ContentType.Blurb:
                    m_valueIcon = "Blurb";
                    break;
                case RadioContentInfo.ContentType.Broadcast:
                    m_valueIcon = "Broadcast";
                    break;
                case RadioContentInfo.ContentType.Commercial:
                    m_valueIcon = "Commercial";
                    break;
                case RadioContentInfo.ContentType.Talk:
                    m_valueIcon = "Talk";
                    break;
            }

            if(m_Icon != null)
            {
                m_Icon.spriteName = m_valueIcon;
            }
        }

        public void SetContentEnabled(bool enabled)
        {
            m_ContentEnabled = enabled;
            UpdateColor();
        }

        private void UpdateColor()
        {
            if(m_IsHovered)
            {
                if(m_ContentEnabled)
                {
                    this.color = new Color32(100, 100, 100, 255);
                }
                else
                {
                    this.color = new Color32(100, 100, 100, 200);
                }
            }
            else
            {
                if (m_ContentEnabled)
                {
                    this.color = new Color32(60, 60, 60, 255);
                }
                else
                {
                    this.color = new Color32(60, 60, 60, 200);
                }
            }
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            m_IsHovered = true;
            UpdateColor();
            initializeButtonsIfNeeded();
            this.m_MoveUp.Show();
            this.m_MoveDown.Show();
            this.m_Disable.Show();
        }

        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            base.OnMouseLeave(p);
            m_IsHovered = false;
            UpdateColor();
            this.m_MoveUp.Hide();
            this.m_MoveDown.Hide();
            this.m_Disable.Hide();
        }

        private void initializeIcon()
        {
            m_Icon = AddUIComponent<UISprite>();
            m_Icon.width = m_Icon.height = Math.Min(34, height);
            m_Icon.relativePosition = new Vector3(10, height / 2.0f - m_Icon.height / 2.0f);
            m_Icon.atlas = m_IconAtlas;
            m_Icon.spriteName = "Music";
            m_Icon.Show();
        }

        private void initializeSongLabel()
        {
            m_Name = AddUIComponent<UILabel>();
            m_Name.relativePosition = new Vector3(50, height / 2.0f - (12.0f * 0.75f) / 2.0f );
            m_Name.width = width - 34;
            m_Name.textScale = 0.75f;
            m_Name.height = height;
            m_Name.textColor = new Color32(150, 150, 150, 255);
            m_Name.text = "Radio station test";
            m_Name.wordWrap = false;
            m_Name.Show();
        }

        private void initializeDisableButton()
        {
            m_Disable = AddUIComponent<UIButton>();
            m_Disable.width = m_Disable.height = Math.Min(34, height);
            m_Disable.relativePosition = new Vector3(width - m_Disable.width, height / 2.0f - m_Disable.height / 2.0f);
            m_Disable.atlas = m_IconAtlas;
            m_Disable.hoveredBgSprite = "OptionBaseFocused";
            m_Disable.pressedBgSprite = "OptionBasePressed";
            m_Disable.normalFgSprite = "Disable";
            m_Disable.Hide();
        }

        private void initializeMoveDownButton()
        {
            m_MoveDown = AddUIComponent<UIButton>();
            m_MoveDown.width = m_MoveDown.height = Math.Min(34, height);
            m_MoveDown.relativePosition = new Vector3(width - m_MoveDown.width * 2, height / 2.0f - m_MoveDown.height / 2.0f);
            m_MoveDown.atlas = m_IconAtlas;
            m_MoveDown.hoveredBgSprite = "OptionBaseFocused";
            m_MoveDown.pressedBgSprite = "OptionBasePressed";
            m_MoveDown.normalFgSprite = "MoveDown";
            m_MoveDown.Hide();
        }

        private void initializeMoveUpButton()
        {
            m_MoveUp = AddUIComponent<UIButton>();
            m_MoveUp.width = m_MoveUp.height = Math.Min(34, height);
            m_MoveUp.relativePosition = new Vector3(width - m_MoveUp.width * 3, height / 2.0f - m_MoveUp.height / 2.0f);
            m_MoveUp.atlas = m_IconAtlas;
            m_MoveUp.hoveredBgSprite = "OptionBaseFocused";
            m_MoveUp.pressedBgSprite = "OptionBasePressed";
            m_MoveUp.normalFgSprite = "MoveUp";
            m_MoveUp.Hide();
        }
    }
}
