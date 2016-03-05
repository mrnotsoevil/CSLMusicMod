using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneBigTown : MusicEntryTag
    {
        public TagMilestoneBigTown() : base("milestone06", "#milestone06 - Big Town")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone6", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

