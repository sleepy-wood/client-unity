using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativePlugin.HealthData;

public enum HealthDataStoreStatus
{
    NotLoaded,
    Outdated, // loading한지 24시간이 지남
    LoadingInProgress,
    Loaded,
}

public static class HealthDataStore
{
    public static SleepSample[] SleepSamples;
    public static ActivitySample[] ActivitySamples;
    private static bool SleepLoading = false;
    private static bool ActivityLoading = false;
    private static DateTime SleepLoadedTime;
    private static DateTime ActivityLoadedTime;

    // 앱 시작 시 호출
    public static bool Init()
    {
        HealthData.RequestAuthCompleted += OnRequestAuthCompleted;
        HealthData.QuerySleepSamplesCompleted += OnQuerySleepSamplesCompleted;
        HealthData.QueryActivitySamplesCompleted += OnQueryActivitySamplesCompleted;
        bool isAvailable = HealthData.IsAvailable();
        if (isAvailable)
        {
            HealthData.RequestAuth();
        }
        return isAvailable;
    }

    // 앱 시작 제외 24시간 마다 호출
    public static void Load()
    {
        if (HealthData.IsAvailable())
        {
            SleepLoading = true;
            ActivityLoading = true;
            DateTime time1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            HealthData.QuerySleepSamples(time1970, DateTime.Now, 100000);
            HealthData.QueryActivitySamples(time1970, DateTime.Now);
        }
    }

    public static HealthDataStoreStatus GetStatus()
    {
        if (SleepSamples is null || ActivitySamples is null)
        {
            return HealthDataStoreStatus.NotLoaded;
        }
        else if (SleepLoading || ActivityLoading)
        {
            return HealthDataStoreStatus.LoadingInProgress;
        }
        else if (
            SleepLoadedTime < DateTime.Now.AddDays(-1)
            || ActivityLoadedTime < DateTime.Now.AddDays(-1)
        )
        {
            return HealthDataStoreStatus.Outdated;
        }
        else
        {
            return HealthDataStoreStatus.Loaded;
        }
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
        if (samples is not null)
        {
            SleepSamples = samples;
        }
        else
        {
            SleepSamples = new SleepSample[0];
        }
        SleepLoading = false;
        SleepLoadedTime = DateTime.Now;
    }

    static void OnQueryActivitySamplesCompleted(ActivitySample[] samples)
    {
        // if there was error, samples will be null
        if (samples is not null)
        {
            ActivitySamples = samples;
        }
        else
        {
            ActivitySamples = new ActivitySample[0];
        }
        ActivityLoading = false;
        ActivityLoadedTime = DateTime.Now;
    }

    public static SleepSample[] GetSleepSamples(DateTime startDate, DateTime endDate)
    {
        if (SleepSamples is null)
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

    public static ActivitySample[] GetActivitySamples(DateTime startDate, DateTime endDate)
    {
        if (ActivitySamples is null)
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
