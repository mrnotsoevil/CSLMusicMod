using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneSmallCity : MusicEntryTag
    {
        public TagMilestoneSmallCity() : base("milestone07", "#milestone07 - Small City")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone7", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

