using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class BenchmarkGpuToCpuTransferTimes : MonoBehaviour
{
    public ComputeShader dummyShader;

    // Update is called once per frame

    void Start(){
        StatsCollector.initialize();
    }

    void  TestGPUtransferSpeed(int width, int height){
        Stopwatch sw = new();
        sw.Start();
        ComputeBuffer buf = new ComputeBuffer(width * height, 4 * 4);
        float[] result = new float[width * height * 4];
        dummyShader.SetBuffer(0, "Result", buf);
        buf.GetData(result);
        StatsCollector.writeStatistic<long>(
            "Get Data From GPU Time " + height.ToString() + "x" + width.ToString(),
            1 ,sw.ElapsedMilliseconds);
        buf.Dispose();
    }

    void Update()
    {
        TestGPUtransferSpeed(720,1280);
        TestGPUtransferSpeed(1080,1920);
        TestGPUtransferSpeed(1440,2560);
        TestGPUtransferSpeed(2160,3840);
    }
}
