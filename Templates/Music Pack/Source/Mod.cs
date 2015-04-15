using System;
using ICities;
using UnityEngine;
using System.Collections;
using System.IO;

namespace CSLMusicPack_<User>_<MyMusicPack_Techname>
{
    public class CSLMusicPack : IUserMod
    {
        public string Name
        {
            get
            {
                return "Music pack: <My Music Pack>";
            }
        }

        public string Description
        {
            get
            {
                return "Add more music to the game. Needs CSLMusicMod";
            }
        }
    }
}

