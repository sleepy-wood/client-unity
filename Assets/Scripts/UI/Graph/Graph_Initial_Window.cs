using NativePlugin.HealthData;
using Photon.Pun.Demo.Cockpit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph_Initial_Window : MonoBehaviour
{
    private SleepSample[] sleepsData;
    private DateTime startTime;
    private void Awake()
    {
        sleepsData = DataTemporary.samples;
    }
    void Start()
    {
        for(int  i = sleepsData.Length - 1; i >= 0; i--)
        {
            
        }
    }

}
