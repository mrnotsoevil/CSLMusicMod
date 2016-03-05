using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneBigCity : MusicEntryTag
    {
        public TagMilestoneBigCity() : base("milestone08", "#milestone08 - Big City")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone8", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

