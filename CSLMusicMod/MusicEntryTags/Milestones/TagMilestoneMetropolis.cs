using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneMetropolis : MusicEntryTag
    {
        public TagMilestoneMetropolis() : base("milestone12", "#milestone12 - Metropolis")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone12", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

