using System;
using ColossalFramework;
using UnityEngine;

namespace CSLMusicMod
{
    public class TagMilestoneLittleHamlet : MusicEntryTag
    {
        public TagMilestoneLittleHamlet() : base("milestone01", "#milestone01 - Little Hamlet")
        {
        }

        public override bool TagApplies(UnityEngine.GameObject gameObject, AudioManager.ListenerInfo info)
        {
            MilestoneInfo mi = null;

            if(Singleton<UnlockManager>.instance.m_allMilestones.TryGetValue("Milestone1", out mi))
            {
                return mi.IsPassed();
            }

            return false;
        }
    }
}

