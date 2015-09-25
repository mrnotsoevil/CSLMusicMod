#CSL Music Mod

![Logo](./Readme_Resources/Logo_800.png)

With CSL Music Mod you can add custom music to Cities: Skylines.

Steam Workshop Link: http://steamcommunity.com/sharedfiles/filedetails/?id=422934383

##Changelog

###Update 4
- Major code restructuring
- Direct playback of *.ogg files
- Next/previous music button
- UI overhaul
- Moved some settings into mod options
- New tag behaviour (now you don't need a "default music")
- More?
- Removed chirper (Message class still existing due to compatibility reasons)

###Update 3.3
- Bugfix: Sky/Bad music is not enabled
- Bugfix: Retry assign sky/bad music if destination entry was not found
- Music list now won't list the full names of the files anymore if not needed (you can disable this)
- Better music list filename display for 'old' mode (if you disable the new one listed above)
- Added scrollbar (you can disable this, too)

###Update 3.2
- Fix crash if loading a map ingame (Menu->Load / Quickload) [Reported by 'Bastard from the Skies']
- Fix crash if Chirper cannot find random residential id

###Update 3.1
- Fixed not saving any settings (enabling/disabling/order)

###Update 3
- Added support for music packs (see [wiki](https://github.com/rumangerst/CSLMusicMod/wiki/Create-a-music-pack) how to create one yourself)
- Added support for additional music folder paths

###Update 2
- Added setting for non random playback
- Added resorting of music list (drag items to resort them)
- Some UI changes

###Update 1
- Added keybindings to settings panel
- Added volume slider to settings panel
- Added chirp if \*.ogg file could not be converted
- Improved Chirper: Only chirp the current song after you switched tracks fast
- Added more settings for configuration file: Height for 'sky' music changeable, mood for 'bad mood' changeable
- Better algorithm to select next random music track (Should prevent too many repeats)
- Bugfix: Cannot rename objects (houses, persons, ...) because panel pops up

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
With Update 4 I finally managed to create an audio player, but Unity does not support mp3 audio in the final game.

###Why are you including NVorbis in your source?
Because I want to prevent conflicts with other mods, which may use NVorbis. Also I want to achieve that C:S can compile the source files, which is not possible if NVorbis is not distributed as source.

###What changes did you make in NVorbis?
I Changed the namespaces to prevent conflicts

##Acknowledgements

- Cities: Skylines developers and publisher for this great game
- NVorbis \- I hope you are not mad that I include your code
- KDE Breeze project - Great icons !
- Add more

 
