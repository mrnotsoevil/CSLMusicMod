using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneTinyTown : MusicEntryTag
    {
        public TagMilestoneTinyTown() : base("milestone03", "#milestone03 - Tiny Town")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone3", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

