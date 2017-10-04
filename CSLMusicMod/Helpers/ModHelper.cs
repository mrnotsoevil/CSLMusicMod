using System;
using System.IO;
using ColossalFramework.Plugins;

namespace CSLMusicMod.Helpers
{
    /// <summary>
    /// Helper methods for interacting with other mods
    /// </summary>
	public static class ModHelper
	{
		/// <summary>
		/// Gets the mod by the specified folder
		/// </summary>
		/// <returns>The PluginInfo of the mod in folder.</returns>
		/// <param name="folder">Folder of the mod</param>
		public static PluginManager.PluginInfo GetSourceModFromFolder(String folder)
		{
			foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
			{
				if (folder.StartsWith(info.modPath))
					return info;
			}

			return null;
		}

		/// <summary>
        /// Checks if a mod with given ID exists
        /// </summary>
        /// <returns><c>true</c>, if the mod exists, <c>false</c> otherwise.</returns>
        /// <param name="id">ID of the mod</param>
		public static bool PluginIdExists(String id)
		{
			foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
			{
				if (info.publishedFileID.AsUInt64.ToString() == id)
					return true;
			}

			return false;
		}

		/**
         * 
         * Gets the mod by the specified id string
         * Only returns mod with CSLMusicMod_Music folder
         * 
         * */
		public static PluginManager.PluginInfo GetSourceModFromId(String id)
		{
			foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
			{
				if (info.publishedFileID.AsUInt64.ToString() == id)
				{
                    if (Directory.Exists(Path.Combine(info.modPath, "CSLMusicMod_Music")))
						return info;
				}
			}

			return null;
		}
	}
}

