using System;
using System.IO;

namespace CSLMusicMod
{
    public class CSLMusicChirperMessage : MessageBase
    {
        public enum MusicMessageType
        {
            NowPlaying,
            Welcome
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
            return MessageManager.instance.GetRandomResidentID();
        }

       public override string GetText()
        {
            switch (_type)
            {
                case MusicMessageType.NowPlaying:
                    return String.Format("Now playing {0} #music{1}", _parameters);
                case MusicMessageType.Welcome:
                    return String.Format("Music mod is now #online. " +
                                         "Use [{0}] key for #switching to the next track. Press [{1}] key for #more.",
                                         CSLMusicModSettings.Key_NextTrack.ToString(),
                                         CSLMusicModSettings.Key_Settings.ToString());
            }

            return "#crazy";
        }

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
    }
}

