using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace CSLMusicMod
{
    public class ChannelInitializer : MonoBehaviour
    {
        private bool _isInitialized;
        private Dictionary<string, RadioChannelInfo> _customPrefabs;

        public ChannelInitializer()
        {
        }

        protected void InitializeImpl()
        {
            UserRadioCollection collection = Resources.FindObjectsOfTypeAll<UserRadioCollection>().First();

            foreach(UserRadioChannel channel in collection.m_Stations.Values)
            {
                CreatePrefab(channel.m_Name, "Default", new Action<RadioChannelInfo>((RadioChannelInfo obj) => {                    
                    obj.m_stateChain = channel.m_StateChain;
                    obj.m_Atlas = channel.GetThumbnailAtlas(obj.m_Atlas.material);
                    obj.m_Thumbnail = "thumbnail";
                }));
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
            _customPrefabs = new Dictionary<string, RadioChannelInfo>();
        }

        /*public void OnLevelWasLoaded(int level)
        {
            if (level == 6)
            {
                _customPrefabs.Clear();
                _isInitialized = false;
            }
        }*/

        public void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }
       
        public void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void OnLevelFinishedLoading (Scene scene, LoadSceneMode mode)
        {
            if(mode == LoadSceneMode.Single)
            {
                _customPrefabs.Clear();
                _isInitialized = false;
            }
        }

        public void Update()
        {          
            if (!_isInitialized)
            {
                RadioChannelCollection collection = Resources.FindObjectsOfTypeAll<RadioChannelCollection>().FirstOrDefault();

                if (collection != null && collection.isActiveAndEnabled)
                {
                    Loading.QueueLoadingAction(() =>
                        {
                            InitializeImpl();
                            PrefabCollection<RadioChannelInfo>.InitializePrefabs("CSLMusicChannel ", _customPrefabs.Values.ToArray(), null);
                        });
                    _isInitialized = true;
                }
            }
        }

        protected void CreatePrefab(string newPrefabName, string originalPrefabName, Action<RadioChannelInfo> setupAction)
        {
            var originalPrefab = FindOriginalPrefab(originalPrefabName);

            if (originalPrefab == null)
            {
                Debug.LogErrorFormat("AbstractInitializer#CreatePrefab - Prefab '{0}' not found (required for '{1}')", originalPrefabName, newPrefabName);
                return;
            }
            if (_customPrefabs.ContainsKey(newPrefabName))
            {
                return;
            }
            var newPrefab = ClonePrefab(originalPrefab, newPrefabName, transform);
            if (newPrefab == null)
            {
                Debug.LogErrorFormat("AbstractInitializer#CreatePrefab - Couldn't make prefab '{0}'", newPrefabName);
                return;
            }
            setupAction.Invoke(newPrefab);
            _customPrefabs.Add(newPrefabName, newPrefab);
        }

        protected static RadioChannelInfo ClonePrefab(RadioChannelInfo originalPrefab, string newName, Transform parentTransform)
        {
            var instance = UnityEngine.Object.Instantiate(originalPrefab.gameObject);
            instance.name = newName;
            var newPrefab = instance.GetComponent<RadioChannelInfo>();
            newPrefab.m_Atlas = originalPrefab.m_Atlas;
            newPrefab.m_Thumbnail = originalPrefab.m_Thumbnail;
            instance.SetActive(false);
            newPrefab.m_prefabInitialized = false;
            return newPrefab;
        }

        protected static RadioChannelInfo FindOriginalPrefab(string originalPrefabName)
        {
            RadioChannelInfo foundPrefab;           
            foundPrefab = Resources.FindObjectsOfTypeAll<RadioChannelInfo>().FirstOrDefault(netInfo => netInfo.name == originalPrefabName);
            if (foundPrefab == null)
            {
                return null;
            }
            return foundPrefab;
        }
    }
}

