using System;
using System.Linq;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLMusicMod
{
	/// <summary>
	/// Used for detours of RadioPanel. See Detours class for the detour code.
	/// </summary>
	public class CustomRadioPanel : RadioPanel
    {
        public CustomRadioPanel()
        {
        }

        /// <summary>
        /// Radio station buttons in vanilla game have multiple sprites (one for normal state,
        /// another one for if the button is pressed, ...). Custom stations only have a thumbnail.
        /// This detour method overwrites the vanilla behavior and makes is possible for
        /// music pack creators to only provide a thumbnail.
        /// </summary>
        /// <param name="button">Button.</param>
        /// <param name="station">Station.</param>
        private void CustomAssignStationToButton(UIButton button, RadioChannelInfo station)
        {       
            UserRadioCollection collection = LoadingExtension.UserRadioContainer;
            button.atlas = station.m_Atlas;


            if(collection != null && collection.m_Stations.ContainsKey(station.name))
            {                
                button.normalFgSprite = station.m_Thumbnail;
                button.hoveredFgSprite = station.m_Thumbnail;
                button.pressedFgSprite = station.m_Thumbnail;
                button.focusedFgSprite = station.m_Thumbnail;
                button.disabledFgSprite = station.m_Thumbnail;
                button.color = new Color32(225, 225, 225, 255);
                button.hoveredColor = new Color32(255, 255, 255, 255);
                button.tooltip = station.GetLocalizedTitle();
                button.BringToFront();
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
                button.color = new Color32(255, 255, 255, 255);
                button.hoveredColor = new Color32(255, 255, 255, 255);
                button.tooltip = station.GetLocalizedTitle();
                button.BringToFront();
            }

            if(button.parent != null && button.parent.name == "StationsList")
            {
                if(ModOptions.Instance.DisabledRadioStations.Contains(station.name))
                {
                    button.isVisible = false;
                }
                else
                {
                    button.isVisible = true;
                }
            }
        }
    }
}

