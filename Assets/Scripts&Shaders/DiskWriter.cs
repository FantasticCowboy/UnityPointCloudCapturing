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
using System.Text;
using UnityEngine.Rendering;



// Probably do not need the DiskWriter writer part of this, I can delete it later on
struct writeInformation{
    public DiskWriter writer;

    public byte[] data;    

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
    

    byte[] encodePixels(Color[] pixels){
        string tmp = "";
        tmp += pixels[518400].r;
        return Encoding.ASCII.GetBytes(tmp);
    }

    // need to make a deepcopy of pixels because it will get overwritten next frame
    //public void SaveDepthFramePipelineNaiveString(Color[] pixels){
    //    writeInformation info = new writeInformation(encodePixels(pixels), this);
    //    collection.Add(info);
    //}

    //// need to make a deepcopy of pixels because it will get overwritten next frame
    //public void SaveDepthFramePipelineNaiveRenderTexture(RenderTexture texture){
    //    AsyncGPUReadbackRequest req = AsyncGPUReadback.Request(texture);
    //    collection.Add(new writeInformation(req, this));
    //}    

    public void SaveDepthFrameNaive(byte[] pixels){
        Stopwatch sw = new Stopwatch();
        sw.Start();                
        File.WriteAllBytes("test_file", pixels);
    }
}
