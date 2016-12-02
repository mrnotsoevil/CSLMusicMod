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
        private UIView m_UIView;       
        private UIMusicListPanel m_ListPanel;

        private bool m_Initialized = false;

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
            m_UIView = v;
            AddListPanel(v);

            m_Initialized = true;
        }

        public void Update()
        {
            if(m_Initialized)
            {
                var radiopanel = Resources.FindObjectsOfTypeAll<RadioPanel>().FirstOrDefault();

                if(radiopanel != null && m_ListPanel != null)
                {
                    m_ListPanel.isVisible = ReflectionHelper.GetPrivateField<bool>(radiopanel, "m_isVisible");
                }
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

        public void OnDestroy()
        {
            if(m_ListPanel != null)
                MonoBehaviour.Destroy(m_ListPanel);
        }
    }
}

