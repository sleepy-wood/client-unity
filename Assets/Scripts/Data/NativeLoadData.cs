using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativePlugin.HealthData;
using System;

public class NativeLoadData
{
    public void LoadNativeData()
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
            List<SleepDataStruct> sleepDataArray = new List<SleepDataStruct>();
            foreach (var sample in samples)
            {
                Debug.Log("Start:QuerySleepSamplesCompleted: " + sample.ToString());
                SleepDataStruct sleepDataStruct = new SleepDataStruct();
                sleepDataStruct.StartDate = sample.StartDate;
                sleepDataStruct.EndDate = sample.EndDate;
                sleepDataStruct.Type = sample.Type;
                sleepDataArray.Add(sleepDataStruct);
            }

            DataTemporary.samples = samples;
            DataTemporary.ArraySleepData.arraySleepData = sleepDataArray;
        }
    }
}
