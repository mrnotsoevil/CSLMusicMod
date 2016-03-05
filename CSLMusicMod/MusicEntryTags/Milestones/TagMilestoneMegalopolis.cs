using System;
using ColossalFramework;

namespace CSLMusicMod
{
    public class TagMilestoneMegalopolis : MusicEntryTag
    {
        public TagMilestoneMegalopolis() : base("milestone13", "#milestone13 - Megalopolis")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone13", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

