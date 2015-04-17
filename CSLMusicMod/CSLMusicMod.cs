using System;
using ICities;
using UnityEngine;
using System.Collections;
using System.IO;

namespace CSLMusicMod
{
    public class CSLMusicMod : LoadingExtensionBase, IUserMod
    {
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

        public CSLMusicMod()
        {

        }

        private void ensureGameObject()
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
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            //Create folders if not available
            CSLMusicModSettings.CreateFolders();

            //Load settings 
            CSLMusicModSettings.LoadModSettings();

            ensureGameObject();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            //ensure it!
            ensureGameObject();

            //Create ui
            if (_ui == null)
                _ui = _gameObject.AddComponent<MusicUI>();

            MusicUI.ChirpWelcome();
            MusicUI.ChirpConverterError();
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

