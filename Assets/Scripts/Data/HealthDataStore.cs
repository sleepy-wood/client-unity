using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativePlugin.HealthData;

public static class HealthDataStore
{
    public static SleepSample[] SleepSamples;
    public static ActivitySample[] ActivitySamples;
    private static DateTime SleepLoadedTime;
    private static DateTime ActivityLoadedTime;

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
        DateTime now = DateTime.Now;
        bool SleepLoaded = now - SleepLoadedTime < TimeSpan.FromDays(1);
        bool ActivityLoaded = now - ActivityLoadedTime < TimeSpan.FromDays(1);
        return SleepLoaded && ActivityLoaded;
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
            SleepSamples = samples;
            SleepLoadedTime = DateTime.Now;
        }
    }

    static void OnQueryActivitySamplesCompleted(ActivitySample[] samples)
    {
        // if there was error, samples will be null
        if (samples != null)
        {
            ActivitySamples = samples;
            ActivityLoadedTime = DateTime.Now;
        }
    }

    static SleepSample[] GetSleepSamples(DateTime startDate, DateTime endDate)
    {
        if (SleepSamples == null)
        {
            return null;
        }
        else
        {
            List<SleepSample> sleepSamplesList = new List<SleepSample>();
            foreach (var sample in SleepSamples)
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
        if (ActivitySamples == null)
        {
            return null;
        }
        else
        {
            List<ActivitySample> activitySamplesList = new List<ActivitySample>();
            foreach (var sample in ActivitySamples)
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