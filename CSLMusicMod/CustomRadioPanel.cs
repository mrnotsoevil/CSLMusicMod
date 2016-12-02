using System;
using System.Linq;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLMusicMod
{
    public class CustomRadioPanel : RadioPanel
    {
        public CustomRadioPanel()
        {
        }

        private void CustomAssignStationToButton(UIButton button, RadioChannelInfo station)
        {
            UserRadioCollection collection = Resources.FindObjectsOfTypeAll<UserRadioCollection>().FirstOrDefault();

            button.atlas = station.m_Atlas;

            if(collection != null && collection.m_Stations.ContainsKey(station.name))
            {                
                button.normalFgSprite = station.m_Thumbnail;
                button.hoveredFgSprite = station.m_Thumbnail;
                button.pressedFgSprite = station.m_Thumbnail;
                button.focusedFgSprite = station.m_Thumbnail;
                button.disabledFgSprite = station.m_Thumbnail;
                button.tooltip = station.GetLocalizedTitle();
            }
            else
            {
                if (station.m_Thumbnail == null)
                {
                    Debug.LogError("Station " + station.GetLocalizedTitle() + " has no thumbnail assigned.");
                }
                button.normalFgSprite = station.m_Thumbnail;
                button.hoveredFgSprite = station.m_Thumbnail + "Hovered";
                button.pressedFgSprite = station.m_Thumbnail + "Pressed";
                button.focusedFgSprite = station.m_Thumbnail;
                button.disabledFgSprite = station.m_Thumbnail + "Disabled";
                button.tooltip = station.GetLocalizedTitle();
            }
        }
    }
}

