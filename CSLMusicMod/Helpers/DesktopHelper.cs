using System;
using System.Diagnostics;

namespace CSLMusicMod.Helpers
{
    public static class DesktopHelper
    {
        /// <summary>
        /// Opens file with external program
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFileExternally(string path)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    Process.Start(path);
                    break;
                case PlatformID.Win32Windows:
                    Process.Start(path);
                    break;
                case PlatformID.Unix:
                    Process.Start("xdg-open", path);
                    break;
                case PlatformID.MacOSX:
                    Process.Start("open", path);
                    break;
            }
        }
    }
}