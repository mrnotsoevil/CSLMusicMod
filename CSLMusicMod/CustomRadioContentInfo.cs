using System;
using UnityEngine;
using System.IO;
using ColossalFramework.IO;
using ColossalFramework;

namespace CSLMusicMod
{
	/// <summary>
	/// Used for detours of RadioContentInfo. See Detours class for the detour code.
	/// </summary>
	public class CustomRadioContentInfo : RadioContentInfo
    {	
        /// <summary>
        /// The game usually loads its music from its data directories. This is not compatible with
        /// custom music. This detour method loads vanilla music the vanilla way and 
        /// custom music from absolute paths.
        /// </summary>
        /// <returns>The obtained clip.</returns>
        public WWW CustomObtainClip()
        {
            if(File.Exists(this.m_fileName))
            {
                var uri = new Uri(this.m_fileName);
                var uristring = uri.AbsoluteUri;
				uristring = uristring.Replace ("%20", " ");
				//var uristring = "file://" + this.m_fileName.Replace("\\","/").Replace("#", "%23");

                CSLMusicMod.Log("Loading custom clip from " + this.m_fileName + " (" + uristring + ")");

                return new WWW(uristring);
            }
            else
            {
                string text = Path.Combine(DataLocation.gameContentPath, "Radio");
                text = Path.Combine(text, this.m_contentType.ToString());
                text = Path.Combine(text, this.m_folderName);
                text = Path.Combine(text, this.m_fileName);

                CSLMusicMod.Log("Loading Clip from " + text);
                return new WWW("file:///" + text);
            }
        }
    }
}

