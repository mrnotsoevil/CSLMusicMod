#CSL Music Mod
With CSL music mod, you can add custom music to Cities: Skylines.

##Features
- Plays your music randomly
- You can enable or disable tracks
- (For now) very "lightweight" UI to select/disable/enable tracks
- Support for "Mood" and "Height": Game will switch to other songs when camera is height or your citicens are not very happy
- Uses chirps to show which track is playing (Only if you select the track manually)
- ~~Chirps cannot be disabled because Chirpy is not holding a gun on my head~~
- You can enable or disable Chirpy, "Mood" and "Height" music, ... in a configuration file
- Automatically puts together matching music files to a track with "Mood"/"Height" if you wish

##Adding music files
The mod gets its music files from _CSLMusicMod_Music_ folder (location depends by platform, you can find the folder where many other mods are storing their data).

Just put \*.ogg or \*.raw files into that folder and if you start the game, the music will be used.

###\*.raw files
You can create \*.raw files manually if you want. Just export raw audio data in an audio editor (e.g. Audacity) with following settings:
* Signed 16 bit PCM
* Little Endian byte order
* 2 Channels (Stereo)
* Frequency 44100Hz

##Music files for 'bad mood' and 'sky'
If enabled (see later), the mod can automatically determine the 'type' of a music file, so you don't have to mess with the configuration file.

To determine the type, add #hashtag at the end of the filename (but do not change the extension!).

- \#sky for 'sky'/'hovering above your city' music
- \#bad for 'bad mood' music
 
### Example
Let **MyMusicFile.ogg** (or \*.raw) be your main music file for 'normal' ('good mood') music.

Rename your other music files like this:

- **MyMusicFile #sky.ogg** or **MyMusicFile#sky.ogg** for sky music
- **MyMusicFile #bad.ogg** or **MyMusicFile#bad.ogg** for bad mood music
 

__Note: You must provide a music file for 'normal'/'good' condition. You cannot have songs only for 'bad' and 'sky'__

##Ingame

The mod will be active when you load a city or create a new one. If you keep the default settings, press [N] to switch to the next track and [M] to open the music list.

##Configuration

###Configuration file
You can configure the mod using *CSLMusicMod_Settings.ini*

```
[Music Selection]
# Enable changing music when you are heigh enough above your city
HeightDependentMusic=True
# Enable changing music when your city is not running well
MoodDependentMusic=True
[Tweaks]
# Enable music while loading. Use this if you have a stuttering problem while loading
MusicWhileLoading=True
[Chirper]
# Enable our beloved leader Chirpy
EnableChirper=True
[Keys]
# Key for switching track
NextTrack=N
# Key for opening the settings/music list
ShowSettings=M
[Music Library]
# Enable automatically determining custom music types using #hashtags
AutoAddMusicTypesForCustomMusic=True
```

###Music Configuration
The mod usually does everything automatically. But you can configure your own list of tracks in *CSLMusicMod_MusicFiles.csv*.

This file is a table, you can open in spreadsheet programs like LibreOffice or Excel. Use tabulator/tab as only delimiter.

Each row in the table is a 'music entry', a collection of music files. You can enable or disable an entry (equivalent to enabling/disabling in ingame user interface). Also you can set a music file for each music type (sky, bad mood) and enable or disable this particular song.

Please don't forget that you have to insert \*.raw files into the table. 

__Note: You must provide a music file for 'normal'/'good' condition. You cannot have songs only for 'bad' and 'sky'__

##Troubleshooting

###Some \*.ogg files won't convert
This is usually caused by too large ID3 tags. NVorbis (http://nvorbis.codeplex.com/), which is used for decoding \*.ogg files cannot handle too large tags.

Try to remove ID3 tags by using Audacity or programs like EasyTag.

###My game is loading too long
This happens because \*.ogg files have to be converted to \*.raw files. This only happens once, so get a cup of tea and wait until it's finished :)

##Planned features

- Add ingame settings panel
- Add ingame music list reload

##Development stuff

###How does this mod work?
You cannot replace the AudioManager without breaking everything, but I found a way to overwrite the music:
AudioManager contains a static field m_audibles, containing IAudibleManagers. These managers are called when a sound is played (```PlayAudio()``` method).

AudioManager (the vanilla one) is an IAudibleManager and redirects PlayAudio the the base class (```PlayAudio_Impl()```) which is overriden by AudioManager. PlayAudio_Impl of AudioManager contains UpdateMusic, which sets m_MusicFile (this is how music is set!).

I register a custom IAudibleManager (```CSLAudioWatcher```) after the AudioManager and overwrite m_MusicFile using .NET Reflection.
The mod also deletes all music files from AudioManager to prevent a "fight" between vanilla music and custom music.

When CSLAudioWatcher is destroyed, the old music files are restored.

###Why no \*.mp3, ... support?
Because I want to ensure that this mod runs on every platform. When I looked for an audio library, I stumbled upon the reason, why .NET did not kill Java: .NET libraries are not really platform independent.

NVorbis is a library written completely in C#, implementing an open standard (Ogg Vorbis), so it should work **everywhere** (at least I hope that it will work everywhere).

###Why are you using NVorbis? Unity3D can read audio, too!
Because I spent hours trying to get Unity3D to load my files correctly (Did not work - just loaded max. 2min of an audio file).

I cannot believe that such a popular engine cannot load an audio file from file system properly.

 
