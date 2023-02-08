using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaputreFPS : MonoBehaviour
{
    static float getCurrentFPS(){
        return 1.0f / Time.unscaledDeltaTime;
    }

    public static void recordFPS(){
        StatsCollector.writeStatistic<float>(Screen.height + " " + Screen.width + " FPS", 1, getCurrentFPS());
    }



    // Update is called once per frame
    void Update(){
    }
}
