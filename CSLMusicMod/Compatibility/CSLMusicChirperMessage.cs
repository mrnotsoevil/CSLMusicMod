using System;
using System.IO;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// Class that exists for compatibility reasons with very old CSLMusicMod versions.
    /// It is not used anywhere.
    /// </summary>
    public class CSLMusicChirperMessage : MessageBase
    {
        public enum MusicMessageType
        {
            NowPlaying,
            Welcome,
            ConvertError
        }

        private MusicMessageType _type;
        private object[] _parameters;

        public CSLMusicChirperMessage(MusicMessageType type, params object[] parameters)
        {
            _type = type;
            _parameters = parameters;
        }

        public override string GetSenderName()
        {
            return "Music Mod";
        }

        public override uint GetSenderID()
        {
            //Sometimes throws an error?

            try
            {
                return MessageManager.instance.GetRandomResidentID();
            }
            catch (Exception)
            {
                Debug.LogError("[CSLMusic] Could not get random resident ID for chirp");

                return 0;
            }
        }

        public override string GetText()
        {
            return "Existing for compatibility reasons";
        }

    }
}

