using System;
using ColossalFramework;

namespace CSLMusicMod
{
	public class TagVanillaSnow: MusicEntryTag
	{       
		public TagVanillaSnow() : base("snow", "#snow - Music when the map is a snow map")
		{

		}

		public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
		{
			// Make it a little 'mightier' than default game: Winter == if environment is winter or it is cold (temp <= 2°C)
			bool vanilla_winter = (Singleton<SimulationManager>.instance.m_metaData.m_environment == "Winter");
			bool cslmm_winter = (Singleton<WeatherManager>.instance.m_currentTemperature <= 2.0);

			return vanilla_winter || cslmm_winter;
		}

	}
}

