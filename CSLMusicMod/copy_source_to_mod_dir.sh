#!/bin/bash

mkdir ~/.local/share/Colossal\ Order/Cities_Skylines/Addons/Mods/CSLMusicMod/Source
rm -rv ~/.local/share/Colossal\ Order/Cities_Skylines/Addons/Mods/CSLMusicMod/Source/*
cp -rv ./* ~/.local/share/Colossal\ Order/Cities_Skylines/Addons/Mods/CSLMusicMod/Source/
rm -rv ~/.local/share/Colossal\ Order/Cities_Skylines/Addons/Mods/CSLMusicMod/Source/bin
rm -v ~/.local/share/Colossal\ Order/Cities_Skylines/Addons/Mods/CSLMusicMod/Source/copy_source_to_mod_dir.sh