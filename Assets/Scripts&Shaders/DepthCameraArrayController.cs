using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;
using System.Threading;

public class DepthCameraArrayController : MonoBehaviour
{
    public struct RenderCameraJob : IJob
    {
        NewDataProcessing dataProcessor;
        public void Execute()
        {
            dataProcessor.run();
        }

        public RenderCameraJob(NewDataProcessing dataProcessorIn){
            dataProcessor = dataProcessorIn;
        }

    }    
    // Update is called once per frame
    void Update()
    {
        Stopwatch sw = new();
        sw.Start();
        List<Thread> threads = new();
        foreach( var dataProcessor in gameObject.GetComponentsInChildren<NewDataProcessing>()){
            dataProcessor.run();
        }        
        //foreach( var dataProcessor in gameObject.GetComponentsInChildren<NewDataProcessing>()){
        //    threads.Add(new Thread(dataProcessor.ServePendingGpuRequests()));
        //            dataProcessor.run();
        //}        

        

        StatsCollector.writeStatistic<long>("Concurrent Pipeline Time", 232, sw.ElapsedMilliseconds);
    }
}
