using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneBoomTown : MusicEntryTag
    {
        public TagMilestoneBoomTown() : base("milestone04", "#milestone04 - Boom Town")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone4", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

