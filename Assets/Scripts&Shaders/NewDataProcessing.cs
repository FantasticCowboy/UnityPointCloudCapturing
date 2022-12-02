using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class NewDataProcessing : MonoBehaviour
{
    bool ran;

    int timesRun = 0;
    public int maxTimesRun = 1;
    int resolutionX;
    int resolutionY;
    int uid;

    

    static DiskWriter ds = new DiskWriter(1920, 1080);

    public ComputeShader deltaShader;

    public RenderTexture oldTexture;

    HashSet<AsyncGPUReadbackRequest> pendingGpuRequests; 

    void Start() {
        uid = (int) (UnityEngine.Random.Range(0,int.MaxValue));
        //Debug.Log(uid);        
        resolutionX = Screen.width;
        resolutionY = Screen.height;
        StatsCollector.initialize();
        DiskWriter.initialize();
        ran = false;

        oldTexture = new RenderTexture(resolutionX, resolutionY, 0);
    }

    // Update is called once per frame
    void Update(){
        resolutionX = Screen.width;
        resolutionY = Screen.height;

        if(timesRun < maxTimesRun){
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SendDepthFrameToDisk();
            StatsCollector.writeStatistic<long>("Total Pipeline Time", uid, sw.ElapsedMilliseconds);
            timesRun++;
            ran = true;
        }
    }

    void gpuDataCoroutine(){
        foreach(var req in pendingGpuRequests){
            if(req.done){
                ds.SaveDepthFramePipelineNaive(req.GetData<byte>().ToArray());
            }
            pendingGpuRequests.Remove(req);
        }
    }


    void printIfOldTextureIsCreated(string tag){
        if(oldTexture != null){
            UnityEngine.Debug.Log("Old Texture created?" + tag + " =" + oldTexture.IsCreated());
        }
    }



    void SendDepthFrameToDisk(){
        RenderTexture newTexture = RenderColorArray();
        
        Stopwatch sw = new Stopwatch();
        sw.Start();

        RenderTexture result = new RenderTexture(
            resolutionX, 
            resolutionY, 
            0, 
            RenderTextureFormat.ARGB32, 
            RenderTextureReadWrite.sRGB
        );



        result.enableRandomWrite = true;
        result.Create();
        
        if(oldTexture != null){
            int kernel = deltaShader.FindKernel("CSMain");
            deltaShader.SetTexture(0,"NewTexture", newTexture);
            deltaShader.SetTexture(0,"OldTexture", oldTexture);
            deltaShader.SetTexture(0,"Result", result);

            deltaShader.Dispatch(kernel, newTexture.width / 8, newTexture.height / 8, 1);

            RenderTexture.active = result;        
        }else{
            RenderTexture.active = newTexture;
        }

        StatsCollector.writeStatistic<long>("Dispatch Time", uid, sw.ElapsedMilliseconds);
        sw.Restart();

        Texture2D tex = new Texture2D(result.width, result.height);

        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        StatsCollector.writeStatistic<long>("Read Pixels Time", uid, sw.ElapsedMilliseconds);
        ds.SaveDepthFramePipelineNaiveRenderTexture(result);

        // Cleanup render texture
        RenderTexture.active = null;
        result.Release();
        if(oldTexture != null){
            oldTexture.Release();
        }

        // Swap for next iteration
        Graphics.CopyTexture(newTexture, oldTexture);
    }

    RenderTexture RenderColorArray(){
        RenderTexture renderTex = new RenderTexture(resolutionX, resolutionY, 0);
        renderTex.Create();        
        GetComponent<Camera>().targetTexture = renderTex;
        GetComponent<Camera>().Render();  
        GetComponent<Camera>().targetTexture = null;
        return renderTex;    
    }



    void comparePixels(Color[] arr1, Color[] arr2){
        for(int i = 0; i < arr1.Length; i++){
            if(arr1[i].r != arr2[i].r){
                UnityEngine.Debug.Log("Pixel arrays not equal at index " + i.ToString() + "on time run=" + timesRun + "for camera " + gameObject.name);
                break;
            }
        }
        UnityEngine.Debug.Log("Pixel arrays equal on time run=" + timesRun + "for camera " + gameObject.name);        
    }    
}