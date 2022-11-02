using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenEffects : MonoBehaviour
{
    #region Variables
    public Shader curShader;
    public float depthPower = 1.0f;
    private Material curMaterial;
    #endregion

    #region Properties
    Material material{
        get
        {
            if(curMaterial == null){
                curMaterial = new Material(curShader);
                curMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return curMaterial;
        }
    }
    #endregion

    void Start(){
        if(!SystemInfo.supportsImageEffects){
            enabled = false;
            return;
        }
        if(curShader && !curShader.isSupported){
            enabled = false;
        }
    }
    
    void Update()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        depthPower = Mathf.Clamp(depthPower, 0, 5);        
    }

    void OnDisable() {
        if(curMaterial){
            DestroyImmediate(curMaterial);
        }    
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if(curShader != null){
            // Might need to change this a little bit
            material.SetFloat("_DepthPower", depthPower);
            Graphics.Blit(src, dest, material);
        }else{
            Graphics.Blit(src, dest);
        }
    }
}
