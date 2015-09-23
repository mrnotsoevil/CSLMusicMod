using System;
using UnityEngine;
using ColossalFramework;

namespace CSLMusicMod
{
    public class BackgroundMusicPlayer : MonoBehaviour
    {
        private SavedFloat _MusicAudioVolume = new SavedFloat(Settings.musicAudioVolume, Settings.gameSettingsFile, DefaultSettings.musicAudioVolume, true);
        private AudioSource _mainAudioSource;
        private AudioSource _helperAudioSource;

        public BackgroundMusicPlayer()
        {
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void ensureAudioSources()
        {
            if (_mainAudioSource == null)
                _mainAudioSource = GetComponent<AudioSource>();
            if (_helperAudioSource == null)
                _helperAudioSource = GetComponent<AudioSource>();
        }

        public void Start()
        {
            //ensureAudioSources();
        }

        public void ImmediatelyStopPlayback()
        {
            /*ensureAudioSources();

            if (_mainAudioSource.isPlaying)
                _mainAudioSource.Stop();
            if (_helperAudioSource.isPlaying)
                _helperAudioSource.Stop();*/
        }

        public void StopPlayback()
        {
            //ImmediatelyStopPlayback();
        }

        public void Playback(String file)
        {
            /*if (Application.platform == RuntimePlatform.WindowsPlayer)
                file = "file:///" + file;
            else
                file = "file://" + file;

            WWW fs = new WWW(file);

            _mainAudioSource.clip = fs.audioClip;
            _mainAudioSource.Play();*/
        }

        public void Update()
        {
            //_mainAudioSource.volume = _MusicAudioVolume.value;
        }
    }
}

