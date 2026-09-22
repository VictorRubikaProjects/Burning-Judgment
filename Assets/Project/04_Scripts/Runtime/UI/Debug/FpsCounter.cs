using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    public int MaxFrames = 60; 

    private static float lastFPSCalculated = 0f;
    private List<float> frameTimes = new();
    
    [SerializeField] private TMP_Text m_text;

    void Start () {
        lastFPSCalculated = 0f;
        frameTimes.Clear();
    }
 
    void Update () {
        addFrame();
        lastFPSCalculated = calculateFPS();
        m_text.text = "FPS: " + lastFPSCalculated.ToString("F2");
    }

    private void addFrame()
    {
        frameTimes.Add(Time.deltaTime);
        if (frameTimes.Count > MaxFrames)
        {
            frameTimes.RemoveAt(0);
        }
    }

    private float calculateFPS()
    {
        float newFPS = 0f;

        float totalTimeOfAllFrames = 0f;
        foreach (float frame in frameTimes)
        {
            totalTimeOfAllFrames += frame;
        }
        newFPS = ((float)(frameTimes.Count)) / totalTimeOfAllFrames;

        return newFPS;
    }

    public static float GetCurrentFPS()
    {
        return lastFPSCalculated;
    }
}
