using System;
using System.IO;

namespace CSLMusicMod
{
    public class UserRadioContent
    {
        public RadioContentInfo.ContentType m_ContentType;

        public String m_Name;

        public String m_DisplayName;

        public String m_Collection;

        public String m_FileName;

        public UserRadioChannel[] m_Channels;

        // Post-launch

        public RadioContentInfo m_VanillaContentInfo;

        public UserRadioContent()
        {
        }

        public UserRadioContent(String collection, String filename)
        {
            m_Name = "CSLMusic" + "/" + collection + "/" + Path.GetFileNameWithoutExtension(filename);
            m_DisplayName = Path.GetFileNameWithoutExtension(filename);
            m_FileName = filename;
            m_Collection = collection;

            String basename = Path.GetFileNameWithoutExtension(filename);

            if(basename.EndsWith("#blurb"))
            {
                m_ContentType = RadioContentInfo.ContentType.Blurb;
            }
            else if(basename.EndsWith("#talk"))
            {
                m_ContentType = RadioContentInfo.ContentType.Talk;
            }
            else if(basename.EndsWith("#commercial"))
            {
                m_ContentType = RadioContentInfo.ContentType.Commercial;
            }
            else if(basename.EndsWith("#broadcast"))
            {
                m_ContentType = RadioContentInfo.ContentType.Broadcast;
            }
            else
            {
                m_ContentType = RadioContentInfo.ContentType.Music;
            }
        }
    }
}

