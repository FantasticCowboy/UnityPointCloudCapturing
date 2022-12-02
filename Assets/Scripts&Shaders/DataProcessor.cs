
using UnityEngine;
using System.Diagnostics;
using System;
using System.Collections.Generic;



public class DataProcessor : MonoBehaviour
{
    bool ran;

    int timesRun = 0;
    public int maxTimesRun = 1;
    int resolutionX;
    int resolutionY;
    int uid;
    

    static DiskWriter ds = new DiskWriter(1920, 1080);
    static RenderTexture target;

    static ComputeBuffer newFrameComputeBuffer;
    static ComputeBuffer oldFrameComputeBuffer;

    public ComputeShader deltaShader;
    public ComputeShader newDeltaShader;


    byte[] oldFrame;


    void Start() {
        uid = (int) (UnityEngine.Random.Range(0,int.MaxValue));
        //Debug.Log(uid);        
        resolutionX = Screen.width;
        resolutionY = Screen.height;
        if(target == null){
            target = new RenderTexture(resolutionX, resolutionY, 0);
        }
        
        StatsCollector.initialize();
        DiskWriter.initialize();
        ran = false;
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
    void SendDepthFrameToDisk(){
        Stopwatch sw = new Stopwatch();

        sw.Start();
        byte[] newFrame = RenderAndGetColorArray();
        StatsCollector.writeStatistic<long>("Render and get color array time delta", uid, sw.ElapsedMilliseconds);
        sw.Restart();


        // TODO: we do not need to be allocating this memory every single iteration
        byte[] temp = new byte[newFrame.Length];
        Buffer.BlockCopy(newFrame, 0, temp, 0, newFrame.Length); 
        StatsCollector.writeStatistic<long>("Time to copy new frame into temporary frame", uid, sw.ElapsedMilliseconds);    



        // Note Pixel array is modified
        sw.Restart();
        DeltaEncodingGPU(newFrame);
        oldFrame = temp;

        //StatsCollector.writeStatistic<Double>("Compression", uid, encodedArray.Length / (newFrame.Length * 1.0));    



        StatsCollector.writeStatistic<long>("Delta encoding and remove zeros time", uid, sw.ElapsedMilliseconds);    
        sw.Restart();
 
        byte[] encodedArray = RemoveZeros(newFrame);
        StatsCollector.writeStatistic<long>("Copy array and remove zeros", uid, sw.ElapsedMilliseconds);         
        
        //ds.SaveDepthFramePipelineNaive(encodedArray);
    }

    void NewSendDepthFrameToDisk(){
        RenderTexture tex = RenderColorArray();
        
    }

    // Debug method to see if two byte arrays have the same values
    // Requires that the arrayas have the same length
    bool CompareByteArrayContents(byte[] arr1, byte[] arr2){
        for(int i = 0; i < arr1.Length; i++){
            if(arr1[i] != arr2[i]){
                return false;
            }
        }
        return true;

    }

    void initializeComputeBuffers(int lengthOfByteArray, int stride){
        if(newFrameComputeBuffer == null || oldFrameComputeBuffer == null){
            newFrameComputeBuffer = new(lengthOfByteArray/stride, stride);
            oldFrameComputeBuffer = new(lengthOfByteArray/stride, stride);
        }
    }

    void DeltaEncodingGPU(RenderTexture tex){
            
    }

    // Runs the compute shader on the new frame, updates the new frame
    void DeltaEncodingGPU(byte[] NewFrame){

        Stopwatch sw = new Stopwatch();
        sw.Start();

        if(oldFrame==null){
            return;
        }

        initializeComputeBuffers(NewFrame.Length, 4);

        newFrameComputeBuffer.SetData(NewFrame);        
        oldFrameComputeBuffer.SetData(oldFrame);

        UnityEngine.Debug.Log(CompareByteArrayContents(NewFrame, oldFrame));

        int kernel = deltaShader.FindKernel("CSMain");
        deltaShader.SetBuffer(kernel,"OldFrame", oldFrameComputeBuffer);
        deltaShader.SetBuffer(kernel, "NewFrame", newFrameComputeBuffer);
        

        // I have no clue why I picked this number of threads
        // Sends the buffers to the GPU for processing
        deltaShader.Dispatch(kernel, NewFrame.Length/4/64, 1,1);
        StatsCollector.writeStatistic<long>("GPU dispatch time", uid, sw.ElapsedMilliseconds);
        sw.Restart();

        // Retrieves that data from the GPU, should probably be a nonblocking call but unsure of how it all works
        newFrameComputeBuffer.GetData(NewFrame); 
        StatsCollector.writeStatistic<long>("Get Data from GPU time", uid, sw.ElapsedMilliseconds);
    }

    byte[] DeltaEncodingCPU(byte[] newFrame){
        
        List<byte> encoding = new();

        if(oldFrame == null){
            return encoding.ToArray();
        }

        for(int i = 0; i < newFrame.Length; i++){
            newFrame[i] ^= oldFrame[i];
            if(!newFrame[i].Equals(0) ){
                encoding.Add(newFrame[i]);
            }

        }
        return encoding.ToArray();
    }

    // TODO: this needs to add in each non-zero byte's position in the array
    byte[] RemoveZeros(byte[] frame){
        List<byte> newArray = new List<byte>();
        foreach(byte b in frame){
            if(!b.Equals(0)){
                newArray.Add(b);
            }
        }
        return newArray.ToArray();        
    }


    RenderTexture RenderColorArray(){
        RenderTexture tex = new RenderTexture(resolutionX, resolutionY, 0);
        GetComponent<Camera>().targetTexture = target;
        GetComponent<Camera>().Render();    
        return tex;    
    }
    byte[] RenderAndGetColorArray(){
        int resolutionX = Screen.width;
        int resolutionY = Screen.height;


        GetComponent<Camera>().targetTexture = target;
        GetComponent<Camera>().Render();



        RenderTexture.active = target;
        Texture2D tex2d = new Texture2D(resolutionX, resolutionY, TextureFormat.ARGB32, false);

        // This is required!
        tex2d.ReadPixels(new Rect(0, 0, resolutionX, resolutionY), 0, 0);

        // For some reason when trying to encode the image and then saving it to disk, the saved image
        // is corrupted, i need to look into that
        
        return tex2d.GetRawTextureData();
    }


}