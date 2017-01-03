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
                var uri = new Uri(this.m_fileName);
                var uristring = uri.AbsoluteUri;
				uristring = uristring.Replace ("%20", " ");
				//var uristring = "file://" + this.m_fileName.Replace("\\","/").Replace("#", "%23");

                Debug.Log("Loading custom clip from " + this.m_fileName + " (" + uristring + ")");

                return new WWW(uristring);
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

