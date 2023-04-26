using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;
using System.Threading;


public class DepthCameraArrayController : MonoBehaviour
{


    void writeConfigs(){
        List<Camera> cams = new();

        List<string> filePrefix = new();
        foreach( var dataProcessor in gameObject.GetComponentsInChildren<DataProcessing>()){
            cams.Add(dataProcessor.GetComponent<Camera>());
            filePrefix.Add((dataProcessor.uid.ToString()));
        }          
        OutputCameraConfigurations.WriteCameraConfigurations("./delta_encoding_raw_data/cameraConfigs.json", cams, filePrefix);        
    }
    void Start(){
        writeConfigs();
    }

    // Update is called once per frame
    void Update()
    {
        Stopwatch sw = new();
        sw.Start();
      
        foreach( var dataProcessor in gameObject.GetComponentsInChildren<DataProcessing>()){
            dataProcessor.run();
        }       


        StatsCollector.writeStatistic<long>("Concurrent Pipeline Time", 232, sw.ElapsedMilliseconds);
    }
}
