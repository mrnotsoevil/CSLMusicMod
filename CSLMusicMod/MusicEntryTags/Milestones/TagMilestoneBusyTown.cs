using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneBusyTown : MusicEntryTag
    {
        public TagMilestoneBusyTown() : base("milestone05", "#milestone05 - Busy Town")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone5", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

