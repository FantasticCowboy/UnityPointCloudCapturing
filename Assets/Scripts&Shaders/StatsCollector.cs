using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System;
using Unity.Jobs;
using System.IO;
using System.Threading;
using System.Diagnostics;

struct writeJob{
    public string data;
    public string filename;

    public writeJob(string dataIn, string filenameIn){
        data = dataIn;
        filename = filenameIn;
    }
}

public class StatsCollector{

    static BlockingCollection<writeJob> queue = new BlockingCollection<writeJob>();
    static bool initialized = false;

    static Stopwatch sw = new Stopwatch();

    static public void writerThread(){
        while(true){
            writeJob result = queue.Take();
            File.AppendAllText(result.filename, result.data);
        }
    }

    static public void initialize(){
        if(!initialized){
            sw.Start();
            File.AppendAllText("stats.txt", String.Format("Starting New Test\n"));
            ThreadStart start = writerThread;
            Thread t = new Thread(start);
            t.Start();
            initialized = true;
        }
    }

    static public long getUnixTimestamp(){ 
        return sw.ElapsedMilliseconds;
    }

    // Requires that T can be formated in astring
    static public void writeStatistic<T>(string metricName, string filename, T val){
        string valueToWrite  = String.Format("{0}, {1}, {2} \n", metricName, val, getUnixTimestamp());
        writeJob job = new writeJob(valueToWrite, filename);
        queue.Add(job);
    }
    static public void writeStatistic<T>(string metricName, int executionId, T val){
        string valueToWrite  = String.Format("{0}, {1}, {2}, {3} \n", metricName, val, executionId, getUnixTimestamp());
        writeJob job = new writeJob(valueToWrite, "stats.txt");
        queue.Add(job);
    }    
}
