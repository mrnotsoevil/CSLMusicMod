using System;
using UnityEngine;
using System.IO;
using ColossalFramework.Plugins;
using System.Collections.Generic;
using System.Collections;

namespace CSLMusicMod
{
	public class ConversionManager : MonoBehaviour
	{
		public const String ConvertedMusicPackMusicFolder = "CSLMusicMod_Musicpacks_Converted";
        //public static List<String> Info_NonConvertedFiles = new List<string>();

		public SettingsManager.Options ModOptions
		{
			get
			{
				return gameObject.GetComponent<SettingsManager> ().ModOptions;
			}
		}

		public ConversionManager ()
		{

		}

		public void Awake()
		{
			DontDestroyOnLoad(this);
		}

		public IEnumerator ConvertCustomMusic()
		{
			Debug.Log("[CSLMusic] Converting custom and music pack music ...");

			//Collect all to convert
			Dictionary<String, String> conversiontasks = new Dictionary<string, string>();

			foreach (String folder in ModOptions.CustomMusicFolders)
			{
				ConvertCustomMusic_AddConversionTasks(folder, conversiontasks, false);
			}
			foreach (String folder in ModOptions.ModdedMusicSourceFolders)
			{
				ConvertCustomMusic_AddConversionTasks(folder, conversiontasks, true);
			}

			//Convert
			foreach (KeyValuePair<String,String> task in conversiontasks)
			{
				String srcfile = task.Key;
				String dstfile = task.Value;               

				if (Path.GetExtension(srcfile) == ".ogg")
				{
					if (!File.Exists(dstfile))
					{
						Debug.Log("[CSLMusic] To convert: " + srcfile + " -> " + dstfile);
						AudioFormatHelper.ConvertOggToRAW(srcfile, dstfile);

						yield return null;
					}
					else
					{
						Debug.Log("[CSLMusic] Not converting " + srcfile + " to " + dstfile);
					}
				}
			}

			//Set/unset
			RemoveUnsubscribedConvertedModpackMusic();
		}

		private void ConvertCustomMusic_AddConversionTasks(String folder, Dictionary<String, String> conversiontasks, bool mod)
		{
			Debug.Log("[CSLMusic] Conversion looking for files @ " + folder);

			if (Directory.Exists(folder))
			{ 
				PluginManager.PluginInfo modification = null;

				if (mod)
				{
					modification = ModHelper.GetSourceModFromFolder(folder);

					if (modification == null)
					{
						Debug.LogError("[CSLMusic] Cannot add folder " + folder + " as mod! Mod could not be identified");
						return;
					}

					if (!modification.isEnabled)
					{
						//Don't convert if mod is not active
						return;
					}
				}

				//Get music in pack folder and look if the file has a corresponding *.raw file
				//If not, convert the file to raw
				foreach (String file in Directory.GetFiles(folder))
				{ 
					if (Path.GetExtension(file) == ".ogg")
					{
						String srcfile = file;
						String dstfile;

						if (mod)
						{
							//We need to change the folder!
							dstfile = Path.Combine(CreateModConvertedMusicFolder(modification), Path.GetFileNameWithoutExtension(srcfile) + ".raw");
						}
						else
						{
							dstfile = Path.ChangeExtension(file, ".raw"); //We can work in out own folder
						}

						if (!File.Exists(dstfile))
						{
							//Add task
							conversiontasks[srcfile] = dstfile;
						}
					}
				}        
			}
			else
			{
				Debug.LogError("ERROR: " + folder + " is not existing!");
			}
		}

		private void RemoveUnsubscribedConvertedModpackMusic()
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

				if (ModHelper.PluginIdExists(modid))
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

		/**
         * Creates and returns the folder containing the converted files for given mod
         * */
		public String CreateModConvertedMusicFolder(PluginManager.PluginInfo info)
		{
			String destinationpath = Path.Combine(ConvertedMusicPackMusicFolder, info.publishedFileID.AsUInt64.ToString());

			if (!Directory.Exists(destinationpath))
				Directory.CreateDirectory(destinationpath);

			return destinationpath;
		}
	}
}

