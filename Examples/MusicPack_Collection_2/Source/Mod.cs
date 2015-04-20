using System;
using ICities;
using UnityEngine;
using System.Collections;
using System.IO;

namespace CSLMusicPack_mrnotsoevil_Collection2
{
    public class CSLMusicPack : IUserMod
    {
        public string Name
        {
            get
            {
                return "Music pack: mrnotsoevil's Experimental Collection 2";
            }
        }

        public string Description
        {
            get
            {
                return "Add more music by Airtone, Blue Dot Session, Chris Zabriskie, Fri.events Orchestra and Sergey Kovchik to the game. Needs CSLMusicMod";
            }
        }
    }
}

