using System;
using UnityEngine;
using System.Reflection;
using ColossalFramework;
using System.Collections;

namespace CSLMusicMod
{
    public class MusicInjector : MonoBehaviour
    {
        public CSLAudioWatcher AudioWatcher
        {
            get;
            private set;
        }

        /**
         * Returns the list of audibleManagers
         * */
        private FastList<IAudibleManager> Audibles
        {
            get
            {
                return ReflectionHelper.GetPrivateStaticField<FastList<IAudibleManager>>(typeof(AudioManager), "m_audibles");
            }
        }

        //Store this pointer for later reconstruction of audio manager
        private static string[] _musicFilesPtr;

        public MusicInjector()
        {
            AudioWatcher = new CSLAudioWatcher();
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            Debug.Log("[CSLMusic] Adding CSLAudioWatcher ...");

            //Loading
            StartCoroutine(LoadMusicFiles());

            AudioManager.RegisterAudibleManager(AudioWatcher);

            //Destroy music compontent of AudioManager
            _musicFilesPtr = ReflectionHelper.GetPrivateField<string[]>(Singleton<AudioManager>.instance, "m_musicFiles");
            ReflectionHelper.SetPrivateField(Singleton<AudioManager>.instance, "m_musicFiles", null);

            Debug.Log("[CSLMusic] done.");
        }

        public void OnDestroy()
        {
            Debug.Log("[CSLMusic] Removing CSLAudioWatcher ...");

            ReflectionHelper.SetPrivateField(Singleton<AudioManager>.instance, "m_musicFiles", _musicFilesPtr);
            Audibles.Remove(AudioWatcher);

            Debug.Log("[CSLMusic] done.");
        }

        private IEnumerator LoadMusicFiles()
        {
            yield return new WaitForSeconds(2f);

            //Convert
            yield return StartCoroutine(CSLMusicModSettings.ConvertCustomMusic());

            //Load settings
            CSLMusicModSettings.LoadMusicFiles();
        }
    }
}

