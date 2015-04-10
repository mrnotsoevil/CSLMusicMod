using System;
using UnityEngine;
using System.Reflection;
using ColossalFramework;
using System.IO;

namespace CSLMusicMod
{
    public class CSLAudioWatcher : IAudibleManager
    {
        public static System.Random RANDOM = new System.Random();

        public String MusicFile
        {
            get
            {
                return ReflectionHelper.GetPrivateField<String>(Singleton<AudioManager>.instance, "m_musicFile");
            }
            private set
            {
                ReflectionHelper.SetPrivateField(Singleton<AudioManager>.instance, "m_musicFile", value);
            }
        }

        public Stream CurrentMusicStream
        {
            get
            {
                return ReflectionHelper.GetPrivateField<Stream>(Singleton<AudioManager>.instance, "m_currentMusicStream"); 
            }
        }

        public Stream PreviousMusicStream
        {
            get
            {
                return ReflectionHelper.GetPrivateField<Stream>(Singleton<AudioManager>.instance, "m_previousMusicStream"); 
            }
        }

        public bool IsShowingCredits
        {
            get
            {
                return ReflectionHelper.GetPrivateField<bool>(Singleton<AudioManager>.instance, "isShowingCredits");
            }
        }

        public String CurrentMusicFile
        {
            get
            {
                return _currentMusic_File;
            }
        }

        public CSLCustomMusicEntry CurrentMusicEntry
        {
            get
            {
                return _currentMusic;
            }
        }

        private bool _switchMusic_Requested;
        private CSLCustomMusicEntry _switchMusic_Requested_Music;
        /**
         * Keep track of the last max. position
         * If the stream restarts, switch the music
         * */
        private Stream _stream;
        private long _streamLastKnownMaxPosition;
        private bool _firstTimeSwitched;
        private CSLCustomMusicEntry _previousMusic;
        private CSLCustomMusicEntry _currentMusic;
        private String _currentMusic_File;

        public CSLAudioWatcher()
        {
            _firstTimeSwitched = false;
            _switchMusic_Requested = false;
        }

        public void RequestSwitchMusic()
        {
            RequestSwitchMusic(null);
        }

        public void RequestSwitchMusic(CSLCustomMusicEntry entry)
        {
            _switchMusic_Requested = true;
            _switchMusic_Requested_Music = entry;
        }

        public void PlayAudio(AudioManager.ListenerInfo listenerInfo)
        {
            //Disable while loading
            if (!Singleton<LoadingManager>.instance.m_loadingComplete)
            {
                //May be annoying (stuttering while loading), so it can be disabled
                if (!CSLMusicModSettings.MusicWhileLoading)
                {
                    SwitchMusicToFile(null);
                }

                return;
            }

            //After loading finished, switch only once
            if (!_firstTimeSwitched)
            {
                Debug.Log("[CSLMusic] Initial Music switch.");

                SwitchMusic(listenerInfo);
                _firstTimeSwitched = true;

                //Yay chirp
                MusicUI.ChirpNowPlaying(_currentMusic);
            }

            //If user requests switch
            if (_switchMusic_Requested)
            {
                Debug.Log("[CSLMusic] User requested switch");
                SwitchMusic(listenerInfo);

                _switchMusic_Requested = false;

                //Yay chirp
                MusicUI.ChirpNowPlaying(_currentMusic);
            }

            /**
                 * CSL usually changes the music by mood and camera height.
                 * Camera height can be easily dealt with (no problem)
                 * 
                 * The problem is that there's no real "randomness"
                 * 
                 * We are watching the current stream if available and if it reaches the end, we will replace 
                 * the current music file
                 * */
         
            UpdateAudioPlayer(listenerInfo);
            UpdateMusic(listenerInfo);
        }

        private void UpdateAudioPlayer(AudioManager.ListenerInfo info)
        {
            /*if (CurrentMusicStream != null)
            {
                Debug.Log("Stream pos: " + CurrentMusicStream.Position + " / " + CurrentMusicStream.Length);
            }*/

            //Dont care if there is no 'should be' file
            if (String.IsNullOrEmpty(_currentMusic_File))
                return;

            //Check if the requested music file is the current music file
            //Do nothing if this is not applying!
            if (_currentMusic_File != Singleton<AudioManager>.instance.m_currentMusicFile)
                return;

            //Do nothing while crossfading
            if (Singleton<AudioManager>.instance.m_previousMusicFile != null)
                return;           

            //OK, CSLAudioWatcher and AudioManager are now synchronized          
            if (CurrentMusicStream != _stream)
            {
                Debug.Log("[CSLMusic] Streams are different. Applying values.");

                //If the stream changed, apply the known values
                MessageManager.instance.QueueMessage(null);
                _stream = CurrentMusicStream;
                _streamLastKnownMaxPosition = _stream.Position;
            }
            else
            {
                //If the stream did not change, but jumped back, switch to new track
                //Otherwise update current position
                long pos = CurrentMusicStream.Position;

                if (pos < _streamLastKnownMaxPosition)
                {
                    Debug.Log("[CSLMusic] Switch because stream " + pos + "/" + CurrentMusicStream.Length + " lk " + _streamLastKnownMaxPosition + " has restarted"); 
                    SwitchMusic(info);
                }
                else if (pos >= CurrentMusicStream.Length - 65536 * 2)
                {
                    Debug.Log("[CSLMusic] Switch because stream " + pos + "/" + CurrentMusicStream.Length + " lk " + _streamLastKnownMaxPosition + " is ending"); 
                    SwitchMusic(info);
                }
                else
                {
                    _streamLastKnownMaxPosition = pos;
                }

            }
        }

        public void UpdateMusic(AudioManager.ListenerInfo info)
        {
            if (_currentMusic == null)
                return;

            //Determine the actual music file

            String musicFile = _currentMusic.GetMusicFromMood(info);

            if (musicFile != null) //Should not happen - but to be sure test it
            {
                SwitchMusicToFile(musicFile);
            }
            else
            {
                Debug.Log("[CSLMusic] GetMusicFromMood returned NULL? WTF");
            }
        }

        public void SwitchMusicToFile(String file)
        {
            //this file should be the current file
            _currentMusic_File = file;

            if (MusicFile != file)
            {
                Debug.Log("[CSLMusic] Forcing music back to " + file);
                MusicFile = file;

                //*** vanilla music! Stop fighting!
                RemoveVanillaMusicFromAudioManager();
            }
        }

        private void RemoveVanillaMusicFromAudioManager()
        {
            ReflectionHelper.SetPrivateField(Singleton<AudioManager>.instance, "m_musicFiles", null);
        }

        public void SwitchMusic(AudioManager.ListenerInfo info)
        {
            Debug.Log("[CSLMusic] Switching music ...");

            FastList<CSLCustomMusicEntry> entries = CSLMusicModSettings.EnabledMusicEntries;

            if (entries.m_size == 0)
            {
                Debug.Log("... cannot do this! There is no available music!");
                return;
            }

            //Store previous music entry
            if (_currentMusic != null)
            {
                _previousMusic = _currentMusic;
            }

            //Set current music entry
            _currentMusic = _switchMusic_Requested_Music == null ? GetNextRandomMusic(entries) : _switchMusic_Requested_Music;
            _switchMusic_Requested_Music = null; //Reset requested
            _switchMusic_Requested = false;

            UpdateMusic(info);

            Debug.Log("Now always enforcing " + _currentMusic);
        }

        private CSLCustomMusicEntry GetNextRandomMusic(FastList<CSLCustomMusicEntry> entries)
        {
            if (entries.m_size == 0)
            {
                return null;
            }

            //Fetch a random music file until it is not matching with the previous one
            CSLCustomMusicEntry music;

            do
            {
                music = entries[RANDOM.Next(entries.m_size)];
            }
            while(entries.m_size > 1 && music == _previousMusic);

            return music;
        }
    }
}

