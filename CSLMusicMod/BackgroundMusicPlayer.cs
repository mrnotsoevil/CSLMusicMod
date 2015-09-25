using System;
using UnityEngine;
using ColossalFramework;
using System.IO;
using System.Collections;

namespace CSLMusicMod
{
    public class BackgroundMusicPlayer : MonoBehaviour
    {
        public const float VolumeModifier_Switch = 0.06f;
        public const float VolumeModifier_Crossover = 0.02f;

        private SavedFloat _MasterAudioVolume = new SavedFloat(Settings.musicAudioVolume, Settings.gameSettingsFile, DefaultSettings.musicAudioVolume, true);
        private SavedFloat _MusicAudioVolume = new SavedFloat(Settings.musicAudioVolume, Settings.gameSettingsFile, DefaultSettings.musicAudioVolume, true);

        private float FinalVolume
        {
            get
            {
                if (Singleton<AudioManager>.instance.MuteAll)
                    return 0f;

                return _MasterAudioVolume.value * _MusicAudioVolume.value;
            }
        }

        private AudioSource _mainAudioSource;
        private AudioSource _helperAudioSource;

        private String _currentFile;
        private AudioClip _currentClip;
        private AudioClip _requestedClip;

        public CSLAudioWatcher AudioWatcher { get; set; }

        public State CurrentState { get; private set; }

        public BackgroundMusicPlayer()
        {
            CurrentState = State.Stopped;
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void ensureAudioSources()
        {
            /*if (_mainAudioSource == null)
                _mainAudioSource = gameObject.AddComponent<AudioSource>();
            if (_helperAudioSource == null)
                _helperAudioSource = gameObject.AddComponent<AudioSource>();*/
            if (_mainAudioSource == null)
                _mainAudioSource = Singleton<AudioManager>.instance.ObtainPlayer((AudioClip)null).m_source;                        
            if (_helperAudioSource == null)
                _helperAudioSource = Singleton<AudioManager>.instance.ObtainPlayer((AudioClip)null).m_source;
        }

        public void Start()
        {
            ensureAudioSources();
        }

        public void ImmediatelyStopPlayback()
        {
            ensureAudioSources();

            if (_mainAudioSource.isPlaying)
                _mainAudioSource.Stop();
            if (_helperAudioSource.isPlaying)
                _helperAudioSource.Stop();

            CurrentState = State.Stopped;

        }

        public void StopPlayback()
        {
            if (CurrentState != State.Stopped)
            {
                ensureAudioSources();

                _currentFile = null;
                _requestedClip = null;

                CurrentState = State.Stopping;
                _helperAudioSource.Stop(); //to be sure: stop helper audio
            }
        }

        public void Playback(String file)
        {
            if (_currentFile == file)
                return;

            ensureAudioSources();

            _currentFile = file;

            Debug.Log("[CSLMusicMod] BackgroundMusicPlayer got " + file);

            _requestedClip = null;
            StartCoroutine(_GetAudioClip(file, new Action<AudioClip>((clip) =>
                        {
                            Debug.Log("... " + clip.samples + " samples are in AudioClip");

                            _requestedClip = null;

                            if (_currentClip == null || Math.Abs(_currentClip.samples - clip.samples) > 1024)
                            {
                                //New track
                                StopAndPlay(clip);
                                //CrossfadeTo(clip, true);
                            }
                            else
                            {
                                //Xover track
                                CrossfadeTo(clip, true);
                            }
                        })));

            /*var clip = GetAudioClip(file);

            Debug.Log("... " + clip.samples + " samples are in AudioClip");

            _requestedClip = null;

            if (_currentClip == null || Math.Abs(_currentClip.samples - clip.samples) > 1024)
            {
                //New track
                StopAndPlay(clip);
                //CrossfadeTo(clip, true);
            }
            else
            {
                //Xover track
                CrossfadeTo(clip, true);
            }*/
        }

        private void StopAndPlay(AudioClip clip)
        {
            if (CurrentState == State.Stopped)
            {
                Debug.Log("[CSLMusicMod] Player is stopped. Just playing.");

                _mainAudioSource.clip = clip;               
                _currentClip = clip;
                _requestedClip = null;
                _mainAudioSource.Play();
                CurrentState = State.Playing;

                _mainAudioSource.volume = 0;
            }
            else
            {
                Debug.Log("[CSLMusicMod] Player is busy. Stopping current playback.");

                CurrentState = State.Stopping;
                _requestedClip = clip;

                _helperAudioSource.Stop(); //to be sure: stop helper audio
            }
        }

        private void CrossfadeTo(AudioClip clip, bool preserveTime)
        {
            Debug.Log("[CSLMusicMod] Crossing over to new music.");

            CurrentState = State.Crossfading;

            var time = _mainAudioSource.timeSamples;

            TransformMainToHelper();
            _mainAudioSource.volume = 0;
            _mainAudioSource.clip = clip;
            _mainAudioSource.Play();

            if (preserveTime)
                _mainAudioSource.timeSamples = time;

            _helperAudioSource.volume = FinalVolume;

            _currentClip = clip;
        }

        private IEnumerator _GetAudioClip(String file, Action<AudioClip> action)
        {
            file = Path.GetFullPath(file);

            if (Application.platform == RuntimePlatform.WindowsPlayer)
                file = "file:///" + file;
            else
                file = "file://" + file;

            Debug.Log("[CSLMusicMod] Loading clip from " + file);

            WWW fs = new WWW(file);
            var clip = fs.GetAudioClip(false, false);   

            while (!clip.isReadyToPlay)
            {
                yield return new WaitForSeconds(0.1f);
            }

            action(clip);
        }

        /*private AudioClip GetAudioClip(String file)
        {
            file = Path.GetFullPath(file);

            if (Application.platform == RuntimePlatform.WindowsPlayer)
                file = "file:///" + file;
            else
                file = "file://" + file;

            Debug.Log("[CSLMusicMod] Loading clip from " + file);

            WWW fs = new WWW(file);
            var clip = fs.GetAudioClip(false, false);   

            while (!clip.isReadyToPlay)
            {
            }

            return clip;
        }*/

        /**
         * Stops main audio stream and puts it into helper stream
         * */
        private void TransformMainToHelper()
        {
            _helperAudioSource.Stop();
            _helperAudioSource.clip = _mainAudioSource.clip;
            _helperAudioSource.timeSamples = _mainAudioSource.timeSamples;
            _mainAudioSource.Stop();
            _helperAudioSource.Play();

            Debug.Log("[CSLMusicMod] Main audio successully tranformed to helper audio.");
        }

        public void Update()
        {
            if (_mainAudioSource == null || _helperAudioSource == null)
                return;

            switch (CurrentState)
            {
                case State.Playing:
                    // Increase or decrease volume until it is the same
                    if (_mainAudioSource.volume < FinalVolume)
                    {
                        var volume2 = Math.Min(FinalVolume, _mainAudioSource.volume + VolumeModifier_Switch);
                        _mainAudioSource.volume = volume2;

                        if (volume2 < FinalVolume)
                            Debug.Log("[CSLMusicMod] Increasing volume of Playing state to " + volume2);
                    }
                    else
                    {
                        var volume2 = Math.Max(FinalVolume, _mainAudioSource.volume - VolumeModifier_Switch);
                        _mainAudioSource.volume = volume2;

                        if (volume2 < FinalVolume)
                            Debug.Log("[CSLMusicMod] Decreasing volume of Playing state to " + volume2);
                    }

                    //Is the music finished?
                    if (!_mainAudioSource.isPlaying)
                    {
                        Debug.Log("[CSLMusicMod] Playback finished.");
                        ImmediatelyStopPlayback();
                        AudioWatcher.RequestSwitchMusic();
                    }

                    break;
                case State.Stopping:

                    // Decrease main audio volume and go to "stopped" if volume is 0
                    var volume = Math.Max(0f, _mainAudioSource.volume - VolumeModifier_Switch);

                    _mainAudioSource.volume = volume;

                    if (volume <= 0)
                    {
                        _currentClip = null;
                        CurrentState = State.Stopped;
                        Debug.Log("[CSLMusicMod] Stopping finished.");
                    }

                    break;
                case State.Stopped:

                    // If stopping has ended, play the requested clip
                    if (_requestedClip != null)
                    {
                        StopAndPlay(_requestedClip);
                    }

                    break;

                case State.Crossfading:

                    // Decrease main audio volume and go to "stopped" if volume is 0
                    {
                        var volume1 = Math.Max(0f, _helperAudioSource.volume - VolumeModifier_Crossover);
                        _helperAudioSource.volume = volume1;
                        var volume2 = Math.Min(FinalVolume, _mainAudioSource.volume + VolumeModifier_Crossover);
                        _mainAudioSource.volume = volume2;

                        //x-fade into "Playing" state
                        if (volume1 <= 0 && volume2 >= FinalVolume)
                        {
                            CurrentState = State.Playing;
                            Debug.Log("[CSLMusicMod] Crossfading finished.");
                        }
                    }
                    break;
            }
        }

        public enum State
        {
            Playing,
            Stopping,
            Stopped,
            Crossfading
        }
    }
}

