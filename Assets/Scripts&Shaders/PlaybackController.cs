using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackController : MonoBehaviour
{

    public static int playbackFPS = 24;
    public static float playbackTimescale = 1.0f;
    public static int startFrameNumber = 0;
    public static int numberOfFramesToRecord = 24;

    public static int frameHeight = 200;
    public static int frameWidth = 200;


    void Start(){
    }

    // Update is called once per frame
    void Update()
    {
        Application.targetFrameRate = playbackFPS;  
        Time.timeScale = playbackTimescale;
    }
}
