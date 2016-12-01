using System;
using ColossalFramework.Plugins;
using System.IO;

namespace CSLMusicMod.Helpers
{
	public static class ModHelper
	{
		/**
         * 
         * Gets the mod by the specified folder
         * 
         * */
		public static PluginManager.PluginInfo GetSourceModFromFolder(String folder)
		{
			foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
			{
				if (folder.StartsWith(info.modPath))
					return info;
			}

			return null;
		}

		/**
		 * Does the mod ID exist?
		 * */
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

