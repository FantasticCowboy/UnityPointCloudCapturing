using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TestDeltaEncoding : MonoBehaviour
{

    RenderTexture oldTexture;
    public ComputeShader deltaEncodingShader;
    public Camera cam;

    public Material deltaEncodingMaterial;

    
    void Update()
    {
        RenderTexture cameraTexture = RenderCamera();
        DeltaEncoding(cameraTexture);   

    }

    public void DeltaEncoding(RenderTexture newTexture){
        if(oldTexture != null){
            RenderTexture temp = new RenderTexture(newTexture);
            int kernel = deltaEncodingShader.FindKernel("CSMain");
            deltaEncodingShader.SetTexture(0,"NewTexture", newTexture);
            deltaEncodingShader.SetTexture(0,"OldTexture", oldTexture);
            deltaEncodingShader.Dispatch(kernel, newTexture.width / 8, newTexture.height / 8, 1);
            oldTexture.Release();
            oldTexture = temp;
            
            deltaEncodingMaterial.mainTexture = newTexture;            
        }
    }

    public RenderTexture RenderCamera(){
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        renderTexture.Create();        
        cam.targetTexture = renderTexture;
        cam.Render();  
        return renderTexture;
    }
}
