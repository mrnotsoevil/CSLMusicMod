using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneWorthyVillage : MusicEntryTag
    {
        public TagMilestoneWorthyVillage() : base("milestone02", "#milestone02 - Worthy Village")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone2", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

