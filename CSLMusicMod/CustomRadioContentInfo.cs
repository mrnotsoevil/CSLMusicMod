using System;
using UnityEngine;
using System.IO;
using ColossalFramework.IO;
using ColossalFramework;

namespace CSLMusicMod
{    
    public class CustomRadioContentInfo : RadioContentInfo
    {
        public WWW CustomObtainClip()
        {
            if(File.Exists(this.m_fileName))
            {
                Debug.Log("Loading Clip from " + this.m_fileName);

                return new WWW("file:///" + WWW.EscapeURL(this.m_fileName));
            }
            else
            {
                string text = Path.Combine(DataLocation.gameContentPath, "Radio");
                text = Path.Combine(text, this.m_contentType.ToString());
                text = Path.Combine(text, this.m_folderName);
                text = Path.Combine(text, this.m_fileName);

                Debug.Log("Loading Clip from " + text);
                return new WWW("file:///" + text);
            }
        }
    }
}

