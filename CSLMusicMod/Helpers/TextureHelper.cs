using System;
using ColossalFramework.UI;
using UnityEngine;
using System.IO;

namespace CSLMusicMod
{
    public static class TextureHelper
    {
        /**
         * All credits to Craxy, authour of Toggle Traffic Lights
         * */
        public static UITextureAtlas CreateAtlas(string file, string name, Material baseMaterial, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            var tex = new Texture2D(spriteWidth * spriteNames.Length, spriteHeight, TextureFormat.ARGB32, false)
                {
                    filterMode = FilterMode.Bilinear,
                };

            //load texture
            if(File.Exists(file))
            {
                using (var textureStream = File.Open(file, FileMode.Open))
                {
                    var buf = new byte[textureStream.Length];  //declare arraysize
                    textureStream.Read(buf, 0, buf.Length); // read from stream to byte array
                    tex.LoadImage(buf);
                    tex.Apply(true, false);
                }
            }
            else
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var textureStream = assembly.GetManifestResourceStream("CSLMusicMod.Resources." + file))
                {
                    var buf = new byte[textureStream.Length];  //declare arraysize
                    textureStream.Read(buf, 0, buf.Length); // read from stream to byte array
                    tex.LoadImage(buf);
                    tex.Apply(true, false);
                }
            }


            var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            // Setup atlas
            var material = UnityEngine.Object.Instantiate(baseMaterial);
            material.mainTexture = tex;

            atlas.material = material;
            atlas.name = name;

            //add sprites
            for (var i = 0; i < spriteNames.Length; ++i)
            {
                var uw = 1.0f / spriteNames.Length;

                var spriteInfo = new UITextureAtlas.SpriteInfo
                    {
                        name = spriteNames[i],
                        texture = tex,
                        region = new Rect(i * uw, 0, uw, 1),
                    };

                atlas.AddSprite(spriteInfo);
            }

            return atlas;
        }
    }
}

