using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Unity.Jobs;
using System.IO.Compression;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;


public class DiskWriter{


    byte[] reusedByteArray;
    public DiskWriter(int width, int height){
        UnityEngine.Debug.Log("Width=" + width);
        UnityEngine.Debug.Log("Height=" + height);
        reusedByteArray = new byte[width * height * sizeof(float)];
        // asd
    }

    public void SaveDepthFrameReuseByteArray(Color[] pixelArray){
        using FileStream compressedArray = File.Open("test_file.gz", FileMode.CreateNew);
        using GZipStream compressor = new GZipStream(compressedArray, CompressionMode.Compress);
        SerializeToMemoryStream(pixelArray, reusedByteArray, compressor);        
    }

    public void SaveDepthFrameReuseByteArray(byte[] pixels){
        using MemoryStream compressedArray = new MemoryStream();
        using GZipStream compressor = new GZipStream(compressedArray, CompressionMode.Compress);
        UnityEngine.Debug.Log("Pixels length=" + pixels.Length);
        compressor.Write(pixels, 0, pixels.Length);
    }

    static void SerializeToMemoryStream(Color[] pixelArray, byte[] bytes, GZipStream compressor){
        Stopwatch sw = new Stopwatch();
        sw.Start();
        {
            UnityEngine.Debug.Log("Time to allocate=" + sw.ElapsedMilliseconds);
            float[] buffer = new float[1];        
            int offset = 0;
            foreach(Color c in pixelArray){            
                buffer[0] = c.r;
                Buffer.BlockCopy(buffer, 0 ,bytes, offset, 1);
                offset += 4;
            }
            compressor.Write(bytes);
        }
        sw.Stop();
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
    }

    public static void SaveDepthFrame(Color[] pixelArray, int width, int height){
        using MemoryStream compressedArray = new MemoryStream();
        using GZipStream compressor = new GZipStream(compressedArray, CompressionMode.Compress);
        byte[] bytes = new byte[width * height * sizeof(float)];
        SerializeToMemoryStream(pixelArray, bytes, compressor);
        //File.WriteAllBytesAsync("test_file.gz", compressedArray.ToArray());    
    }

    public static void StoreColorArray(Color[] pixelArray, int width, int height){
        using MemoryStream compressedArray = new MemoryStream();
        
        using GZipStream compressor = new GZipStream(compressedArray, CompressionMode.Compress);
          
    }    
}
