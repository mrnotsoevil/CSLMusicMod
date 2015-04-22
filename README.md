#CSL Music Mod

![Logo](./Readme_Resources/Logo_800.png)

With CSL Music Mod you can add custom music to Cities: Skylines.

Steam Workshop Link: http://steamcommunity.com/sharedfiles/filedetails/?id=422934383

##Features
- Add custom music to your game
- Converts *.ogg files to Cities: Skylines music files
- Play them randomly or in defined order
- Enable or disable tracks quickly to skip them
- Easy to use ingame panels (Open them with ingame-configurable hotkeys)
- Switch to the next track using a hotkey
- Ingame resorting of tracks (just drag the items in the list)
- Supports vanilla game mood/height music (e.g. if you zoom out, the music changes)
- Add mood/height music just by corretly renaming the music file - mod will configure everything automatically
- Chirpy support - Shows you which track is playing, how to open the panels and music file conversion errors
- ~~Chirps cannot be disabled because Chirpy is not putting a gun to my head~~
- Many features such as automatic music configuration, Chirpy, mood/height music can be easily disabled

![Yay Chirpy](./Readme_Resources/C:S Music Mod_Chirpy.png)

##Changelog

###Update 3.3
- Bugfix: Sky/Bad music is not enabled
- Music list now won't list the full names of the files anymore if not needed (you can disable this)
- Better music list filename display for 'old' mode (if you disable the new one listed above)
- Added scrollbar (you can disable this, too)
- 
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

##Adding music from music packs
Just subscribe a music pack and activate CSL Music Mod and the music pack in the content manager.

If you want to test out this feature, download my little example music pack: http://steamcommunity.com/sharedfiles/filedetails/?id=425299246

##Adding custom music files
The mod gets its music files from _CSLMusicMod_Music_ folder (location depends by platform, you can find the folder where many other mods are storing their data).

Just put \*.ogg or \*.raw files into that folder and if you start the game, the music will be used.

![Example of folder with music files](./Readme_Resources/C:S Music Mod_Example_Filenames.png)

Visit the [wiki](https://github.com/rumangerst/CSLMusicMod/wiki/Adding-music) for more information about adding music.
You also can find here information how to [convert](https://github.com/rumangerst/CSLMusicMod/wiki/How-to-convert-audio-files) your music files.

##Ingame

The mod will be active when you load a city or create a new one. If you keep the default settings, press [N] to switch to the next track and [M] to open the music list.

##Configuration

###Ingame configuration
If you press [M] key (default setting), the music list opens. If you click on "Settings", you can access some settings.

![Music list with settings](./Readme_Resources/C:S Music Mod_Settings_List.png)

Everything you set there will be written into the configuration file.
Visit the [wiki](https://github.com/rumangerst/CSLMusicMod/wiki/Configuration) for more information about configuration using the configuration files. There you also can find how to manually configure the music list.

##Troubleshooting

Please visit the [wiki](https://github.com/rumangerst/CSLMusicMod/wiki/Troubleshooting) for solutions for your problems. You also can comment to the Steam Workshop page and i'll try to help.

##FAQ

###How does mood/height based music work?
It's working like in the vanilla game. If you are above 1400 height, the 'sky' music will play. If the general happiness is below 40, the 'bad' mood music will play.

###Can I delete the \*.ogg files after conversion to \*.raw?
Yes.

###How to convert to \*.ogg?
Use an audio conversion software or website. Here is a list of free and open source audio conversion software:

- http://soundconverter.org Soundconverter (Linux)
- http://audacity.sourceforge.net Audacity (All platforms, ¹)
- http://www.freac.org free:ac (All platforms)
- http://winlame.sourceforge.net/ WinLAME (Windows; suggested by eharper256)
- Suggest me some and I'll add it to the list

Visit the [wiki](https://github.com/rumangerst/CSLMusicMod/wiki/How-to-convert-audio-files) for more information about converting your files.


##Planned features

- ~~Add ingame settings panel~~
- Add ingame music list reload
- More settings and tweaks
- ~~Get music from multiple folders~~
- ~~Support for music pack mods~~

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

###Why are you including NVorbis in your source?
Because I want to prevent conflicts with other mods, which may use NVorbis. Also I want to achieve that C:S can compile the source files, which is not possible if NVorbis is not distributed as source.

###What changes did you make in NVorbis?
I Changed the namespaces to prevent conflicts

##Acknowledgements

- Cities: Skylines developers and publisher for this great game
- NVorbis \- I hope you are not mad that I include your code
- Add more

 
