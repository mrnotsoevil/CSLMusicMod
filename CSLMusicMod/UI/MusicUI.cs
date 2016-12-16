using System;
using System.Linq;
using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.Reflection;

namespace CSLMusicMod.UI
{
    public class MusicUI : MonoBehaviour
    {  
        private UIMusicListPanel m_ListPanel;

        private bool m_Initialized = false;
     
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
            
        }

        private void Initialize()
        {
            //Create ui
            UIView v = UIView.GetAView();
            m_ListPanel = (UIMusicListPanel)v.AddUIComponent(typeof(UIMusicListPanel));           
            m_ListPanel.Hide();

            m_Initialized = true;
        }

        public void Update()
        {
            if (m_Initialized)
            {
                var radiopanel = CurrentRadioPanel;

                if (radiopanel != null && m_ListPanel != null)
                {
                    m_ListPanel.isVisible = ModOptions.Instance.EnableCustomUI && ReflectionHelper.GetPrivateField<bool>(radiopanel, "m_isVisible");
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

