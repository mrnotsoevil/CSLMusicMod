using System;
using System.IO;
using UnityEngine;

namespace CSLMusicMod
{
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
            switch (_type)
            {
                case MusicMessageType.NowPlaying:
                    return String.Format("Now playing {0} #music{1}", _parameters);
                case MusicMessageType.Welcome:
                    return String.Format("Music mod is now #online. " +
                        "Use [{0}] key for #switching to the next track. Press [{1}] key for #settings and #track_list.",
                                         CSLMusicModSettings.Key_NextTrack.ToString(),
                                         CSLMusicModSettings.Key_Settings.ToString());
                case MusicMessageType.ConvertError:
                    return String.Format("Could not convert {0} #music #sad", _parameters);
            }

            return "#crazy";
        }
        /*public override bool IsSimilarMessage(MessageBase other)
        {
            if (other is CSLMusicChirperMessage)
            {
                CSLMusicChirperMessage _other = other as CSLMusicChirperMessage;

                //double check ftw
                if (_other != null)
                {
                    return _other._type == _type;
                }

                return base.IsSimilarMessage(other);
            }
            else
            {
                return base.IsSimilarMessage(other);
            }
        }*/
        public static CSLMusicChirperMessage CreateWelcomeMessage()
        {
            return new CSLMusicChirperMessage(MusicMessageType.Welcome);
        }

        public static CSLMusicChirperMessage CreateNowPlayingMessage(CSLCustomMusicEntry entry)
        {
            String mainname = Path.GetFileNameWithoutExtension(entry.GoodMusic);
            String hashtags = "";

            if (entry.EnableBadMusic && !String.IsNullOrEmpty(entry.BadMusic))
                hashtags += " #moody";
            if (entry.EnableSkyMusic && !String.IsNullOrEmpty(entry.SkyMusic))
                hashtags += " #sky";

            return new CSLMusicChirperMessage(MusicMessageType.NowPlaying, mainname, hashtags);
        }

        public static CSLMusicChirperMessage CreateConverterErrorMessage()
        {
            if (CSLMusicModSettings.Info_NonConvertedFiles.m_size == 0)
                return null;

            String p = "";

            foreach (String file in CSLMusicModSettings.Info_NonConvertedFiles)
            {
                p += Path.GetFileName(file) + ", ";
            }

            p = p.Trim(' ', ',');

            return new CSLMusicChirperMessage(MusicMessageType.ConvertError, p);
        }
    }
}

