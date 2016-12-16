using System;
using System.Threading;

namespace CSLMusicMod
{
    public class CustomAudioManager
    {
        public CustomAudioManager()
        {
        }

        public void CustomQueueBroadcast(RadioContentInfo info)
        {
            if (!ModOptions.Instance.AllowContentBroadcast)
                return;

            var broadcastQueue = ReflectionHelper.GetPrivateField<FastList<RadioContentInfo>>(this, "m_broadcastQueue"); //Why does CO make everything private, so you can't access it ??

            while (!Monitor.TryEnter(broadcastQueue, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                if (broadcastQueue.m_size < 5)
                {
                    for (int i = 0; i < broadcastQueue.m_size; i++)
                    {
                        if (broadcastQueue.m_buffer[i] == info)
                        {
                            return;
                        }
                    }
                    broadcastQueue.Add(info);
                }
            }
            finally
            {
                Monitor.Exit(broadcastQueue);
            }
        }
    }
}

