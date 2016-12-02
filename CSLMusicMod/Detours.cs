using System;
using UnityEngine;
using System.Reflection;
using ColossalFramework.IO;
using System.IO;

namespace CSLMusicMod
{
    public class Detours : MonoBehaviour
    {
        private RedirectCallsState m_RedirectObtainMusicClip;
        private RedirectCallsState m_RedirectStationName;
        private RedirectCallsState m_RedirectRadioPanelButtonGeneration;

        public Detours()
        {
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void OnEnable()
        {
            Install();
        }

        public void OnDisable()
        {
            Uninstall();
        }

        public void Install()
        {
            Debug.Log("[CSLMusic] Installing detours ...");
            m_RedirectObtainMusicClip = RedirectionHelper.RedirectCalls(typeof(RadioContentInfo).GetMethod("ObtainClip", BindingFlags.Instance | BindingFlags.Public),
                typeof(CustomRadioContentInfo).GetMethod("CustomObtainClip", BindingFlags.Instance | BindingFlags.Public));
            m_RedirectStationName = RedirectionHelper.RedirectCalls(typeof(RadioChannelInfo).GetMethod("GetLocalizedTitle", BindingFlags.Instance | BindingFlags.Public),
                typeof(CustomRadioChannelInfo).GetMethod("CustomGetLocalizedTitle", BindingFlags.Instance | BindingFlags.Public));
            m_RedirectRadioPanelButtonGeneration = RedirectionHelper.RedirectCalls(typeof(RadioPanel).GetMethod("AssignStationToButton", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomRadioPanel).GetMethod("CustomAssignStationToButton", BindingFlags.Instance | BindingFlags.NonPublic));
        }

        public void Uninstall()
        {
            Debug.Log("[CSLMusic] Uninstalling detours ...");
            RedirectionHelper.RevertRedirect(m_RedirectObtainMusicClip);
            RedirectionHelper.RevertRedirect(m_RedirectStationName);
            RedirectionHelper.RevertRedirect(m_RedirectRadioPanelButtonGeneration);
        }
    }
}

