using System;
using ICities;
using UnityEngine;
using System.Collections;
using System.IO;
using CSLMusicMod.UI;

namespace CSLMusicMod
{
    public class CSLMusicMod : LoadingExtensionBase, IUserMod
    {
        public const String VersionName = "Update 4";

        public string Name
        {
            get
            {
                return "CSL Music Mod";
            }
        }

        public string Description
        {
            get
            {
                return "Add custom music into the game";
            }
        }

        private GameObject _gameObject;
        private MusicInjector _injector;
        private MusicUI _ui;
        private SettingsUI _settingsui;
        private MusicManager _music;
        private ConversionManager _conversion;
        private SettingsManager _settings;
        private BackgroundMusicPlayer _musicplayer;

        public CSLMusicMod()
        {

        }

        private void ensureComponents()
        {
            Debug.Log("Creating injector game object ...");

            if (_gameObject == null)
            {
                _gameObject = new GameObject();
                _gameObject.name = "CSLMusicMod_GO";
            }           

            if (_injector == null)
            {
                _injector = _gameObject.AddComponent<MusicInjector>();       
            }

            // Create settings
            if (_settings == null)
            {
                _settings = _gameObject.AddComponent<SettingsManager>();               
            }

            // Create the music list
            if (_music == null)
                _music = _gameObject.AddComponent<MusicManager>();

            // Create the converter
            if (_conversion == null)
                _conversion = _gameObject.AddComponent<ConversionManager>();  

            //Create music player
            if (_musicplayer == null)
                _musicplayer = _gameObject.AddComponent<BackgroundMusicPlayer>();

            // Create folders
            _gameObject.GetComponent<MusicManager>().CreateMusicFolder();

            // Load the settings
            _gameObject.GetComponent<SettingsManager>().LoadModSettings();

            //Add audio watcher to player
            _musicplayer.AudioWatcher = _injector.AudioWatcher;          
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
          
            ensureComponents();		
			
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            ensureComponents();

            if (_settingsui == null)
                _settingsui = _gameObject.AddComponent<SettingsUI>();

            _settingsui.Mod = this;
            _settingsui.InitializeSettingsUI(helper);
        }
               

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            //ensure it!
            ensureComponents();

            //Reload music
            _music.LoadMusicFiles();

            //Preload if activated
            if (_settings.ModOptions.PrefetchSongs && _settings.ModOptions.PlayWithoutConvert)
            {
                _musicplayer.PrefetchAudioClips(_music.MusicEntries);
            }

            //Create ui
            if (_ui == null)
                _ui = _gameObject.AddComponent<MusicUI>();
        }

        public void ReloadUI()
        {
            if (_ui != null)
            {
                MonoBehaviour.Destroy(_ui);
                _ui = _gameObject.AddComponent<MusicUI>();
            }
        }

        public override void OnLevelUnloading()
        {
            if (_gameObject != null)
                MonoBehaviour.Destroy(_gameObject);
            if (_injector != null)
                MonoBehaviour.Destroy(_injector);
            if (_ui != null)
                MonoBehaviour.Destroy(_ui);

            _gameObject = null;
            _injector = null;
            _ui = null;

            base.OnLevelUnloading();
        }
    }
}

