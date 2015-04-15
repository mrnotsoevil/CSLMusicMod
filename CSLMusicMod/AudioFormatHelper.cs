using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace CSLMusicMod
{
    public static class AudioFormatHelper
    {
        /**
         * Reads an OGG audio clip and converts it to *.raw file
         * */
        public static void ConvertOggToRAW(String srcoggfile, String dstrawfile)
        {
            srcoggfile = Path.GetFullPath(srcoggfile);
            dstrawfile = Path.GetFullPath(dstrawfile);

            Debug.Log("[CSLMusic] Convert " + srcoggfile + " to " + dstrawfile);

            try
            {

                using (var vorbis = new NVorbis.VorbisReader(srcoggfile))
                {
                    using (var fileStream = new FileStream(dstrawfile, FileMode.Create))
                    {
                        //NVorbis data
                        int channels = vorbis.Channels;
                        int sampleRate = vorbis.SampleRate;
                        var duration = vorbis.TotalTime;

                        if(channels != 2 || sampleRate != 44100)
                        {
                            Debug.LogError("[CSLMusic] Error: Input file " + srcoggfile + " must have 2 channels and 44100Hz sample rate!");
                        
                            //Add to list
                            CSLMusicModSettings.Info_NonConvertedFiles.Add(srcoggfile);
                            return;
                        }

                        var buffer = new float[16384];
                        int count;

                        //From SavWav
                        Int16[] intData = new Int16[buffer.Length];
                        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

                        Byte[] bytesData = new Byte[buffer.Length * 2];
                        //bytesData array is twice the size of
                        //dataSource array because a float converted in Int16 is 2 bytes.

                        int rescaleFactor = 32767; //to convert float to Int16

                        while ((count = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
                        {
                            // Do stuff with the samples returned...
                            // Sample value range is -0.99999994f to 0.99999994f
                            // Samples are interleaved (chan0, chan1, chan0, chan1, etc.)                       

                            for (int i = 0; i<buffer.Length; i++)
                            {
                                intData[i] = (short)(buffer[i] * rescaleFactor);
                                Byte[] byteArr = new Byte[2];
                                byteArr = BitConverter.GetBytes(intData[i]);
                                byteArr.CopyTo(bytesData, i * 2);
                            }

                            fileStream.Write(bytesData, 0, bytesData.Length);
                        }
                    }
                }

            }
            catch (InvalidDataException ex)
            {
                Debug.LogError("... ERROR: Could not read file! " + ex.Message);

                //Add to list
                CSLMusicModSettings.Info_NonConvertedFiles.Add(srcoggfile);

                return;
            }

            /**
             * Unity3D are you capable of something?!
             * 
             * Problem: Only creates the first  ~2min of output - AudioClip won't load more wtf!?
             * */
            /*WWW wtf = new WWW("file://" + srcoggfile);

            while (!wtf.isDone)
            {
            }

            AudioClip clip = wtf.GetAudioClip(false, false);

            while (!wtf.isDone)
            {
            }

            clip.LoadAudioData();

            while (clip.loadState == AudioDataLoadState.Loading || clip.loadState == AudioDataLoadState.Unloaded)
            {
            }

            Debug.Log("WWW done. Saving now");

            SaveWav.Save(dstrawfile, clip);*/

            Debug.Log("... done.");
        }
    }
}

