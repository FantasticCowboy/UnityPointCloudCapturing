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
using System.Collections.Concurrent;
using System.Threading;



// Probably do not need the DiskWriter writer part of this, I can delete it later on
struct writeInformation{
    public byte[] data;
    public DiskWriter writer;
    public writeInformation(byte[] dataIn, DiskWriter writerIn){
        data = dataIn;
        writer = writerIn;
    }
}

public class DiskWriter{

    static bool initialized = false;

    // Order not guaranteed
    static BlockingCollection<writeInformation> collection = new BlockingCollection<writeInformation>();


    static void writeThread(){
        Stopwatch sw = new Stopwatch();
        while(true){
            sw.Start();
            writeInformation info = collection.Take();
            info.writer.SaveDepthFrameNaive(info.data);            
            StatsCollector.writeStatistic<long>("write time", 0,  sw.ElapsedMilliseconds);
            sw.Restart();
        }
    }

    static public void initialize(){
        if(!initialized){
            ThreadStart start = writeThread;
            Thread t = new Thread(start);
            t.Start();
            initialized = true;
        }
    }

    byte[] reusedByteArray;
    public DiskWriter(int width, int height){
        UnityEngine.Debug.Log("Width=" + width);
        UnityEngine.Debug.Log("Height=" + height);
        reusedByteArray = new byte[width * height * sizeof(float)];
    }

    
    // need to make a deepcopy of pixels because it will get overwritten next frame
    public void SaveDepthFramePipelineNaive(byte[] pixels){
        byte[] cpy = new byte[pixels.Length];
        Buffer.BlockCopy(pixels, 0, cpy, 0, pixels.Length);
        writeInformation info = new writeInformation(cpy, this);
        collection.Add(info);
    }
    

    public void SaveDepthFrameZipFile(byte[] pixels){
        Stopwatch sw = new Stopwatch();
        sw.Start();        
        
        using MemoryStream compressedArray = new MemoryStream();
        UnityEngine.Debug.Log("Time to open file" + sw.ElapsedMilliseconds);
        sw.Restart();
        using GZipStream compressor = new GZipStream(compressedArray, CompressionMode.Compress);
        compressor.Write(pixels, 0, pixels.Length);
        UnityEngine.Debug.Log("Time to compress" + sw.ElapsedMilliseconds);
        sw.Restart();
        
        File.WriteAllBytes("test_file.gz", compressedArray.ToArray());
        UnityEngine.Debug.Log("Time to write to file" + sw.ElapsedMilliseconds);
    }

    public void SaveDepthFrameGzip(byte[] pixels){
        Stopwatch sw = new Stopwatch();
        sw.Start();        
        using MemoryStream compressedArray = new MemoryStream();
        UnityEngine.Debug.Log("Time to open file" + sw.ElapsedMilliseconds);
        sw.Restart();
        using GZipStream compressor = new GZipStream(compressedArray, CompressionMode.Compress);
        compressor.Write(pixels);
        UnityEngine.Debug.Log("Time to compress" + sw.ElapsedMilliseconds);
        sw.Restart();
        File.WriteAllBytes("test_file.gz", compressedArray.ToArray());
        UnityEngine.Debug.Log("Time to write to file" + sw.ElapsedMilliseconds);
    }

    public void SaveDepthFrameGzipDirectlyToFile(byte[] pixels){
        Stopwatch sw = new Stopwatch();
        sw.Start();        
        using FileStream fs = File.Open("test_file.gz", FileMode.OpenOrCreate);
        UnityEngine.Debug.Log("Time to open file" + sw.ElapsedMilliseconds);
        sw.Restart();
        using GZipStream compressor = new GZipStream(fs, System.IO.Compression.CompressionLevel.Fastest);
        compressor.Write(pixels);
        UnityEngine.Debug.Log("Time to compress and write toe file" + sw.ElapsedMilliseconds);
    }

    public void SaveDepthFrameDeflateStream(byte[] pixels){
        Stopwatch sw = new Stopwatch();
        sw.Start();        
        using MemoryStream compressedArray = new MemoryStream();
        UnityEngine.Debug.Log("Time to open file" + sw.ElapsedMilliseconds);
        sw.Restart();
        using DeflateStream compressor = new DeflateStream(compressedArray, CompressionMode.Compress);
        compressor.Write(pixels, 0, pixels.Length);
        UnityEngine.Debug.Log("Time to compress" + sw.ElapsedMilliseconds);
        sw.Restart();
        
        File.WriteAllBytes("test_file.gz", compressedArray.ToArray());
        UnityEngine.Debug.Log("Time to write to file" + sw.ElapsedMilliseconds);
    }

    public void SaveDepthFrameNaive(byte[] pixels){
        Stopwatch sw = new Stopwatch();
        sw.Start();                
        File.WriteAllBytes("test_file.png", pixels);
        UnityEngine.Debug.Log("Time to write to file" + sw.ElapsedMilliseconds);
    }
}
