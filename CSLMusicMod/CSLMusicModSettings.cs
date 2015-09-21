using System;
using System.Collections.Generic;
using System.IO;
using ColossalFramework;
using UnityEngine;
using System.Collections;
using ColossalFramework.Plugins;

namespace CSLMusicMod
{
    public static class CSLMusicModSettings_old
    {
        
        #region Helpers
        

        

        
        #endregion
        #region Adding unknown music
        
        #endregion
        #region Settings Files
        

        
        #endregion
        #region Music Packs    
        private static void RemoveUnsubscribedConvertedModpackMusic()
        {
            Debug.Log("[CSLMusic] Removing unsubscribed converted music files ...");

            List<String> dirstoremove = new List<string>();

            if (Directory.Exists(ConvertedMusicPackMusicFolder))
            {
                dirstoremove.AddRange(Directory.GetDirectories(ConvertedMusicPackMusicFolder));
            }

            //Look through folders and look if pluginid exists
            foreach (String folder in dirstoremove.ToArray())
            {
                String foldername = Path.GetFileName(folder);
                String modid = foldername.TrimStart('_');

                if (PluginIdExists(modid))
                {
                    dirstoremove.Remove(folder);
                }
            }

            //Delete all which are left
            foreach (String folder in dirstoremove)
            {
                Debug.Log("[CSLMusic] ... deleting " + folder);
                Directory.Delete(folder, true);
            }
        }

        private static bool PluginIdExists(String id)
        {
            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (info.publishedFileID.AsUInt64.ToString() == id)
                    return true;
            }

            return false;
        }

        private static void AddModpackMusicFolders()
        {
            ModdedMusicSourceFolders.Clear();

            //If music packs are disabled, just add no folders.
            if (!EnableMusicPacks)
                return;

            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
                if (info.isEnabled)
                {
                    String path = Path.Combine(info.modPath, CustomMusicDefaultFolder);

                    if (Directory.Exists(path))
                    {
                        Debug.Log("[CSLMusic] Adding music pack @ " + path);

                        if (!ModdedMusicSourceFolders.Contains(path))
                        {
                            ModdedMusicSourceFolders.Add(path);
                        }
                    }
                }
            }
        }

        
        #endregion
        #region Folders
       

        public static void CreateFolders()
        {
            CreateMusicFolder();
        }
        #endregion
    }
}

