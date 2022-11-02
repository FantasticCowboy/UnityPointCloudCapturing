
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Profiling;
using System.Diagnostics;



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


    void Start() {
        uid = (int) (UnityEngine.Random.Range(0,10000));
        //Debug.Log(uid);        
        resolutionX = Screen.width;
        resolutionY = Screen.height;
        if(target == null){
            target = new RenderTexture(1920, 1080, 0);
        }
        ran = false;
    }

    // Update is called once per frame
    void Update(){
        resolutionX = Screen.width;
        resolutionY = Screen.height;

        if(timesRun < maxTimesRun){
            SendDepthFrameToDisk();
            timesRun++;
            ran = true;
        }
    }
    void SendDepthFrameToDisk(){
        Stopwatch sw = new Stopwatch();

        sw.Start();
        byte[] pixelArray = RenderAndGetColorArray();
        sw.Stop();

        UnityEngine.Debug.Log("RenderAndGetColorArrayTime:" + sw.ElapsedMilliseconds);
        sw.Reset();
        sw.Start();
        ds.SaveDepthFrameReuseByteArray(pixelArray);
        sw.Stop();
        UnityEngine.Debug.Log("SendDepthFrame Time:" + sw.ElapsedMilliseconds);
    }
    string FormatWorldCoordianteAsString(Vector3 coord){
        return "[" + coord.x + "," + coord.y + "," + coord.z + "]";
    }

    void WriteWorldCoordinateArrayToDisk(Vector3[] coordinates){
        Stopwatch sw = new Stopwatch();
        sw.Start();
        WriteDataToFile("[");
        foreach(Vector3 c in coordinates){
            WriteDataToFile(FormatWorldCoordianteAsString(c) + ",");
        }
        WriteDataToFile("]");
        sw.Stop();
        UnityEngine.Debug.Log("Time to write data to file:" + sw.ElapsedMilliseconds);
    }
    void WriteWorldCoordinateArrayToDisk_V2(Vector3[] coordinates){
        string val = "[";
        long totalElapsedTime = 0;
        foreach(Vector3 c in coordinates){
            Stopwatch sw = new Stopwatch();
            sw.Start();
            val += (FormatWorldCoordianteAsString(c) + ",");
            sw.Stop();
            totalElapsedTime += sw.ElapsedTicks;
        }
        val += ("]");
        long averageElapsedTime = totalElapsedTime / coordinates.Length;
        UnityEngine.Debug.Log("Total Elapsed Time:" + totalElapsedTime);
        UnityEngine.Debug.Log("Elements Visited:" + coordinates.Length);
        UnityEngine.Debug.Log("Average Time To Format:" + averageElapsedTime);
    }    

    string FormatColorAsString(Color c){
        return  "[" + c.r.ToString() + "," + c.g.ToString() + "," + c.b.ToString() + "]";
    }

    Vector3 GetXYZCoordinate(int arrayPos, Color pixel){
        int y = arrayPos / resolutionX;
        int x = arrayPos % resolutionX;
        
        float z = pixel[0] * GetComponent<Camera>().farClipPlane;

        return GetComponent<Camera>().ScreenToWorldPoint(new Vector3(x,y,z));
    }

    Vector3[] ConvertPixelsToCoordinates(Color[] pixelArray){
        Vector3[] output = new Vector3[resolutionX * resolutionY];
        int i = 0;
        foreach(Color pixel in pixelArray){
            output[i] = GetXYZCoordinate(i, pixel);
            i++;
        }
        return output;
    }

    string FormatColorArrayAsString(Color[] colors){
        string ret = "[";
        WriteDataToFile("[");
        foreach(Color c in colors){
            WriteDataToFile(FormatColorAsString(c));
        }
        WriteDataToFile("]");
        return ret;
    }


    Color[] TextureToPixelArray(Texture2D tex2d){
        string[] output = new string[resolutionX * resolutionY];
        Color[] pixelInfo = tex2d.GetPixels();
        return pixelInfo;
    }

    byte[] RenderAndGetColorArray(){
        int resolutionX = Screen.width;
        int resolutionY = Screen.height;
    
        //RenderTexture tempRt = target;
        GetComponent<Camera>().targetTexture = target;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        GetComponent<Camera>().Render();
        UnityEngine.Debug.Log("Time to render camera" + sw.ElapsedMilliseconds);
        sw.Restart();

        RenderTexture.active = target;
        Texture2D tex2d = new Texture2D(resolutionX, resolutionY, TextureFormat.ARGB32, false);
        byte[] rawBytes = tex2d.GetRawTextureData();
        UnityEngine.Debug.Log("Time to read pixels into texture" + sw.ElapsedMilliseconds);
        sw.Restart();

        return rawBytes;
    }


    // Creates a render texture that the camera can write to/
    RenderTexture CreateRenderTexture(){
        return new RenderTexture(Screen.width, Screen.height, 0);
    }

    void WriteDataToFile(string message){
        string path = "./testUnitySaveData_" + uid.ToString();
        File.WriteAllText(path, message);

    }
}
