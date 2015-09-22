using System;
using ICities;
using UnityEngine;
using System.Collections;
using System.IO;

namespace CSLMusicMod
{
    public class CSLMusicMod : LoadingExtensionBase, IUserMod
    {
		public const String VersionName = "Update 4";

        public string Name
        {
            get
            {
                return "Music Mod";
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

        public CSLMusicMod()
        {

        }

		private void ensureInjector()
		{
			Debug.Log("Creating injector game object ...");

			if (_gameObject == null)
			{
				_gameObject = new GameObject();
				_gameObject.name = "CSLMusicMod_GO";
			}           

			if (_injector == null) {
				_injector = _gameObject.AddComponent<MusicInjector> ();       
			}
		}

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
          
			ensureInjector();

			// Create settings
			if (_settings == null)
				_settings = _gameObject.AddComponent<SettingsManager> ();

			// Create the music list
			if (_music == null)
				_music = _gameObject.AddComponent<MusicManager> ();

			// Create the converter
			if (_conversion == null)
				_conversion = _gameObject.AddComponent<ConversionManager> ();

			// Create folders
			_gameObject.GetComponent<MusicManager>().CreateMusicFolder();

			// Load the settings
			_gameObject.GetComponent<SettingsManager> ().LoadModSettings ();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            if (_settingsui == null)
                _settingsui = _gameObject.AddComponent<SettingsUI>();

            _settingsui.InitializeSettingsUI(helper);
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            //ensure it!
			ensureInjector();

            //Create ui
            if (_ui == null)
                _ui = _gameObject.AddComponent<MusicUI>();
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

