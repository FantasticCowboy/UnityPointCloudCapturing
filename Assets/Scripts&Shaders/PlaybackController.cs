using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackController : MonoBehaviour
{

    bool slowed = false;
    public int targetFrameRate;
    public float timeScale;
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        Application.targetFrameRate = targetFrameRate;  
        Time.timeScale = timeScale;

    }
}
