using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativePlugin.HealthData;

public static class HealthDataStore
{
    public static SleepSample[] sleepSamples;
    public static ActivitySample[] activitySamples;
    private static bool sleepLoaded;
    private static bool activityLoaded;

    // 앱 시작 시 호출
    public static void Init()
    {
        Debug.Log("HealthDataIsAvailable: " + HealthData.IsAvailable().ToString());

        HealthData.RequestAuthCompleted += OnRequestAuthCompleted;
        HealthData.QuerySleepSamplesCompleted += OnQuerySleepSamplesCompleted;
        HealthData.QueryActivitySamplesCompleted += OnQueryActivitySamplesCompleted;

        if (HealthData.IsAvailable())
        {
            HealthData.RequestAuth();
        }
    }

    // 앱 시작 제외 24시간 마다 호출
    public static void Load()
    {
        HealthData.QuerySleepSamples(
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
            DateTime.Now,
            10000
        );
        HealthData.QueryActivitySamples(
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
            DateTime.Now
        );
    }

    public static bool Loaded()
    {
        return sleepLoaded && activityLoaded;
    }

    static void OnRequestAuthCompleted(bool granted)
    {
        // if authorization is granted, query sleep samples
        if (granted)
        {
            Load();
        }
    }

    static void OnQuerySleepSamplesCompleted(SleepSample[] samples)
    {
        // if there was error, samples will be null
        if (samples != null)
        {
            sleepSamples = samples;
            sleepLoaded = true;
        }
    }

    static void OnQueryActivitySamplesCompleted(ActivitySample[] samples)
    {
        // if there was error, samples will be null
        if (samples != null)
        {
            activitySamples = samples;
            activityLoaded = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate">startDate + 5</param>
    /// <returns></returns>
    static SleepSample[] GetSleepSamples(DateTime startDate, DateTime endDate)
    {
        if (sleepSamples == null)
        {
            return null;
        }
        else
        {
            List<SleepSample> sleepSamplesList = new List<SleepSample>();
            foreach (var sample in sleepSamples)
            {
                if (sample.StartDate > startDate && sample.EndDate < endDate)
                {
                    sleepSamplesList.Add(sample);
                }
            }
            return sleepSamplesList.ToArray();
        }
    }

    static ActivitySample[] GetActivitySamples(DateTime startDate, DateTime endDate)
    {
        if (activitySamples == null)
        {
            return null;
        }
        else
        {
            List<ActivitySample> activitySamplesList = new List<ActivitySample>();
            foreach (var sample in activitySamples)
            {
                if (sample.Date > startDate && sample.Date < endDate)
                {
                    activitySamplesList.Add(sample);
                }
            }
            return activitySamplesList.ToArray();
        }
    }
}