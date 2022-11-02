using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestRenderImage : MonoBehaviour
{
    #region Variables
    public Shader curShader;
    public float grayScaleAmount = 1.0f;
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
        grayScaleAmount = Mathf.Clamp(grayScaleAmount, 0.0f, 1.0f);        
    }

    void OnDisable() {
        if(curMaterial){
            DestroyImmediate(curMaterial);
        }    
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if(curShader != null){
            material.SetFloat("_LuminosityAmounnt", grayScaleAmount);
            Graphics.Blit(src, dest, material);
        }else{
            Graphics.Blit(src, dest);
        }
    }

}
