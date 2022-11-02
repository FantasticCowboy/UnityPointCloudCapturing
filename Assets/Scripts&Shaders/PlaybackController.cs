using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackController : MonoBehaviour
{

    bool slowed = false;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 1;        
    }

    // Update is called once per frame
    void Update()
    {
        //if (!slowed && Input.GetMouseButtonDown(0)){
        //    Time.timeScale = 1/72f;
        //    slowed = true;
        //}else if(slowed && Input.GetMouseButtonDown(0)){
        //    Time.timeScale = 1f;            
        //    slowed = false;
        //}
    }
}
