using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using NativePlugin.HealthData;

public class NativeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("HealthDataIsAvailable: " + HealthData.IsAvailable().ToString());
        HealthData.RequestAuthCompleted += OnRequestAuthCompleted;
        HealthData.QuerySleepSamplesCompleted += OnQuerySleepSamplesCompleted;
        if (HealthData.IsAvailable())
        {
            HealthData.RequestAuth();
        }
    }
    
    void OnRequestAuthCompleted(bool granted)
    {
        Debug.Log("Start:RequestAuthCompleted: " + granted.ToString());
        // if authorization is granted, query sleep samples
        if (granted)
        {
            HealthData.QuerySleepSamples(
                new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                DateTime.Now,
                100
            );
        }
    }

    void OnQuerySleepSamplesCompleted(SleepSample[] samples)
    {
        // if there was error, samples will be null
        if (samples != null)
        {
            Debug.Log("Start:QuerySleepSamplesCompleted: " + samples.Length.ToString());
            foreach (var sample in samples)
            {
                Debug.Log("Start:QuerySleepSamplesCompleted: " + sample.ToString());
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
