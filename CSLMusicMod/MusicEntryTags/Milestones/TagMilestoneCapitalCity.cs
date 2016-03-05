using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneCapitalCity : MusicEntryTag
    {
        public TagMilestoneCapitalCity() : base("milestone10", "#milestone10 - Capital City")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone10", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

