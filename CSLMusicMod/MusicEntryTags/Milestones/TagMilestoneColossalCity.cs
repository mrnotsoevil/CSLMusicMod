using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneColossalCity : MusicEntryTag
    {
        public TagMilestoneColossalCity() : base("milestone11", "#milestone11 - Colossal City")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone11", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

