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
	    private static UITextureAtlas _listAtlas;

	    public static UITextureAtlas ListAtlas
	    {
	        get
	        {
	            if (_listAtlas == null)
	            {
		            _listAtlas = TextureHelper.CreateAtlas("icons.png", "CSLMusicModUI", UIView.Find<UITabstrip>("ToolMode").atlas.material, 31, 31, new string[]
	                {
	                    "OptionBase",
	                    "OptionBaseDisabled",
	                    "OptionBaseFocused",
	                    "OptionBaseHovered",
	                    "OptionBasePressed",
	                    "Music",
	                    "Next",
	                    "Previous",
	                    "Close",
	                    "SortAscending",
	                    "SortDescending",
	                    "Search",
	                    "Clear",
	                    "Talk", 
	                    "Broadcast",
	                    "Commercial", 
	                    "Blurb", 
	                    "ListEntryNormal",  
	                    "ListEntryHover", 
	                    "ContentDisabled",
		                "Open"
	                });  
	            }

		        return _listAtlas;
	        }
	    }

	    public CustomRadioPanel()
        {
        }

        private void AssignStationToButtonInPanel(UIButton button, UISprite iconsprite, RadioChannelInfo station, UserRadioCollection collection)
        {
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

	        button.text = "";
	        button.spritePadding = new RectOffset(0,0,0,0);
        }

        private void AssignStationToButtonInList(UIButton button, UISprite iconsprite, RadioChannelInfo station,
            UserRadioCollection collection)
        {
            
            if(ModOptions.Instance.DisabledRadioStations.Contains(station.name))
            {
                button.isVisible = false;
            }
            else
            {
                button.isVisible = true;
            }
	        
	        ((UIPanel)button.parent).autoLayoutPadding = new RectOffset(0,0,0,0);

	        button.atlas = ListAtlas;
	        button.normalFgSprite = "ListEntryNormal";
	        button.hoveredFgSprite = "ListEntryHover";
	        button.pressedFgSprite = "ListEntryHover";
	        button.focusedFgSprite = "ListEntryNormal";
	        button.disabledFgSprite = "ListEntryNormal";
	        button.color = new Color32(200, 200, 200, 255);
	        button.hoveredColor = new Color32(255, 255, 255, 255);
	        button.tooltip = station.GetLocalizedTitle();
                
            button.text = station.GetLocalizedTitle();
            button.textColor = new Color32(255,255,255,255);
            button.size = new Vector2(200, 20);
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.textPadding = new RectOffset(25, 2, 2, 2);
	        button.textScale = 0.75f;
	        
	        button.BringToFront();
            
            iconsprite.position = new Vector3(0,0);
            iconsprite.size = new Vector2(20, 20);
            iconsprite.atlas = station.m_Atlas;
            iconsprite.spriteName = station.m_Thumbnail;
            iconsprite.isVisible = true;
        }

		private void AddComboboxVisualClue(UIButton button, UISprite iconsprite, RadioChannelInfo station)
		{
			iconsprite.position = new Vector3(23,-23);
			iconsprite.size = new Vector2(16,16);
			iconsprite.atlas = ListAtlas;
			iconsprite.spriteName = "Arrow";
			iconsprite.isVisible = true;
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
	        UISprite iconsprite = button.Find<UISprite>("sprite");
	        
            // Additional sprite on top of the button
            if (iconsprite == null)
            {
                iconsprite = button.AddUIComponent<UISprite>();
	            iconsprite.name = "sprite";
            }

            // Different behavior depending on if the button is displayed in the panel or in the list
            if (button.parent != null && button.parent.name == "StationsList")
            {
	            if(ModOptions.Instance.EnableImprovedRadioStationList)
                	AssignStationToButtonInList(button, iconsprite, station, collection);
	            else 
		            AssignStationToButtonInPanel(button, iconsprite, station, collection);
            }
            else
            {
                AssignStationToButtonInPanel(button, iconsprite, station, collection);
	            
//	            if(ModOptions.Instance.EnableImprovedRadioStationList)
//	            	AddComboboxVisualClue(button, iconsprite, station);
            }
        }
    }
}

