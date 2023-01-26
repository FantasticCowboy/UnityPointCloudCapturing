using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Threading;

public class DataProcessing : MonoBehaviour
{
    bool ran;

    int timesRun = 0;
    public int maxTimesRun = 1;
    int resolutionX;
    int resolutionY;
    int uid;
    public float delay = 1;

    struct AsyncRead{

        public AsyncGPUReadbackRequest req;
        public RenderTexture old;

        public RenderTexture newTexture;        
        public RenderTexture result;

        public ComputeBuffer encoding;

        public int reqNum;

        public AsyncRead(AsyncGPUReadbackRequest reqIn, RenderTexture oldIn, RenderTexture resultIn,
            RenderTexture newTextureIn, ComputeBuffer encodingIn, int reqNumIn){

            req = reqIn;
            old = oldIn;
            result = resultIn;
            newTexture = newTextureIn;
            encoding = encodingIn;
            reqNum = reqNumIn;
        }
    }    

    void release(AsyncRead read){
        if(read.newTexture!=null){
            renderTextureToReferences[read.newTexture]-=1;
            if(renderTextureToReferences[read.newTexture]==0){
                read.newTexture.Release();
            }
        }        
        if(read.old!=null){
            renderTextureToReferences[read.old]-=1;
            if(renderTextureToReferences[read.old]==0){
                read.old.Release();
            }            
        }
        if(read.result!=null){
            read.result.Release();
        }        
        if(read.encoding !=null){
            read.encoding.Release();
        }
    }

    DiskWriter dw = new DiskWriter(3840, 2160);

    public ComputeShader deltaShader;

    RenderTexture oldTexture;

    HashSet<AsyncRead> pendingGpuRequests = new(); 

    Dictionary<RenderTexture, int> renderTextureToReferences = new();

    void Start() {
        uid = (int) (UnityEngine.Random.Range(0,int.MaxValue));
        //Debug.Log(uid);        
        resolutionX = Screen.width;
        resolutionY = Screen.height;
        StatsCollector.initialize();
        DiskWriter.initialize();
        ran = false;

        //oldTexture = new RenderTexture(resolutionX, resolutionY, 0);
    }

    // Update is called once per frame


    public void run(){
        resolutionX = Screen.width;
        resolutionY = Screen.height;

        // May cause a floating point overflow!
        delay -= Time.deltaTime;
        if(timesRun < maxTimesRun && delay < 0){
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SendDepthFrameToDisk();
            StatsCollector.writeStatistic<long>("Total Pipeline Time", uid, sw.ElapsedMilliseconds);
            timesRun++;
            ran = true;
        }
        ServePendingGpuRequests();
    }
    
    string formatWriteFileName(int fileNum){
        return uid.ToString() + "_" + fileNum.ToString();
    }

    public void ServePendingGpuRequests(){
        List<AsyncRead> doneRequests = new();
        foreach(var read in pendingGpuRequests){
            if(read.req.hasError){
                UnityEngine.Debug.Log(read.req.hasError);
            }else if(read.req.done){
                Stopwatch sw = new();
                sw.Start();
                dw.SaveDepthFramePipelineNaive(read.req.GetData<byte>().ToArray(), formatWriteFileName(read.reqNum));    
                doneRequests.Add(read);
                StatsCollector.writeStatistic<long>("Get Data Time", uid, sw.ElapsedMilliseconds);
                release(read);
            }
        }
        foreach(var req in doneRequests){
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
        RenderTexture result = new RenderTexture(
            resolutionX, 
            resolutionY, 
            0, 
            RenderTextureFormat.ARGB32, 
            RenderTextureReadWrite.sRGB
        );
        result.enableRandomWrite = true;
        result.Create();



        ComputeBuffer Encoding = new ComputeBuffer(Screen.width * Screen.height / 20 , sizeof(float) * 4 );
        ComputeBuffer Count = new ComputeBuffer(1,4);
        
        if(oldTexture != null){
            int kernel = deltaShader.FindKernel("CSMain");
            deltaShader.SetTexture(0,"NewTexture", newTexture);
            deltaShader.SetTexture(0,"OldTexture", oldTexture);
            deltaShader.SetBuffer(0, "Encoding", Encoding);
            

            Stopwatch sw = new();
            sw.Start();
            deltaShader.Dispatch(kernel, newTexture.width / 8, newTexture.height / 8, 1);

            // For debug only delete later 
            // incrementer.GetData(tmp);
            // StatsCollector.writeStatistic<int>("Number of times incremented", 0, tmp[0]);
            //////////////////////////////
            StatsCollector.writeStatistic<long>("Dispatch Time", 0, sw.ElapsedMilliseconds);
            RenderTexture.active = result;    
            
            int[] newCount = new int[1];

            sw.Restart();
            Count.GetData(newCount);
            UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
            //UnityEngine.Debug.Log(newCount[0]);

            AsyncRead read = new AsyncRead(AsyncGPUReadback.Request(Encoding), oldTexture, result, newTexture, Encoding, timesRun);
            pendingGpuRequests.Add(read);        
        }else{
            // Note probably a small 1 time memory leak of newtexture here
            RenderTexture.active = newTexture;            
            Texture2D tmp = new(Screen.width, Screen.height, TextureFormat.RGBAFloat, false);
            Rect regionToReadFrom = new Rect(0, 0, Screen.width, Screen.height);
            tmp.ReadPixels(regionToReadFrom, 0, 0);

            dw.SaveDepthFramePipelineNaive(tmp.GetRawTextureData(), formatWriteFileName(timesRun));            
        }

        // Cleanup render texture
        RenderTexture.active = null;

        // NOTE. TODO. Figure out if we can release eventhough we have a pending gpu readback!!!!!!!!!!!
        //result.Release();
        //if(oldTexture != null){
        //    oldTexture.Release();
        //}
        //// Swap for next iteration
        //
        //newTexture.Release();
        oldTexture = newTexture;

    }

    RenderTexture RenderColorArray(){
        RenderTexture renderTex = new RenderTexture(resolutionX, resolutionY, 0);
        renderTex.Create();        
        GetComponent<Camera>().targetTexture = renderTex;
        GetComponent<Camera>().Render();  
        GetComponent<Camera>().targetTexture = null;

        renderTextureToReferences[renderTex] = 2;
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