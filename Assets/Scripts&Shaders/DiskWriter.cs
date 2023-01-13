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


    public string filename;
    public byte[] data;    
    public writeInformation(byte[] dataIn, DiskWriter writerIn, string filenameIn){
        filename = filenameIn;
        data = dataIn;
        writer = writerIn;
    }
}

public class DiskWriter{

    static string directoryName = "delta_encoding_raw_data/";

    static readonly int NUM_THREADS = 8;

    static bool initialized = false;
    // Order not guaranteed
    static BlockingCollection<writeInformation> collection = new BlockingCollection<writeInformation>();

    static Stopwatch timeSinceStart = new();

    static void countCompressionRatio(object obj){

    }


    string formatFilepath(string name){
        return directoryName + name;
    }


    static void writeThread(){
        Stopwatch sw = new Stopwatch();
        Directory.CreateDirectory(directoryName);
        while(true){
            writeInformation info = collection.Take(); 
            info.writer.SaveDepthFrameNaive(info.data, info.filename);                
            //writeCompressionRatio(info.data);
            StatsCollector.writeStatistic<long>("write time", 0,  sw.ElapsedMilliseconds);
            
            if(sw.ElapsedMilliseconds > 200){
                StatsCollector.writeStatistic<long>("Anomally at", 0, timeSinceStart.ElapsedMilliseconds / 1000);
            }
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
        reusedByteArray = new byte[width * height * sizeof(float)];
        timeSinceStart.Start();
    }


    public void SaveDepthFramePipelineNaive(byte[] pixels, string filename){
        writeInformation info = new writeInformation(pixels, this, filename);
        collection.Add(info);
    }
    
    public void SaveDepthFramePipelineNaive(byte[] pixels){
        writeInformation info = new writeInformation(pixels, this, "test_file");
        collection.Add(info);
    }

    byte[] encodePixels(Color[] pixels){
        string tmp = "";
        tmp += pixels[518400].r;
        return Encoding.ASCII.GetBytes(tmp);
    }

    public void SaveDepthFrameNaive(byte[] pixels){
        SaveDepthFrameNaive(pixels, "test_file");
    }
    public void SaveDepthFrameNaive(byte[] pixels, string filename){
        Stopwatch sw = new Stopwatch();
        sw.Start();                
        File.WriteAllBytes(formatFilepath(filename), pixels);
    }


    // Used to calculate and log what the compression ratio is for a specifc frame
    static void writeCompressionRatio(byte[] arr){
        Stopwatch sw = new();
        sw.Start();
        float count = 0.0F;

        double chunkSize = Math.Ceiling(arr.Length / ((NUM_THREADS) * 1.0f));
        foreach(var b in arr){
            if(!b.Equals(0)){
                count+=1;
            }
        }
        StatsCollector.writeStatistic<float>("Compression Ratio", 0, count / (arr.Length * 1.0f));
        UnityEngine.Debug.Log(count );
        UnityEngine.Debug.Log(arr.Length);        
        UnityEngine.Debug.Log(count / arr.Length);
        StatsCollector.writeStatistic<long>("Time to Iterate Through Array", 0, sw.ElapsedMilliseconds);
    }


}
