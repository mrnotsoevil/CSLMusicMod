using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneGrandCity : MusicEntryTag
    {
        public TagMilestoneGrandCity() : base("milestone09", "#milestone09 - Grand City")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone9", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

