using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativePlugin.HealthData;

// 총 수면 시간
public enum SleepAmount
{
    Zero, // 데이터가 없음 (Watch를 차고 자지 않거나 잠을 자지 않음)
    VeryInadequateBad, // 3시간 미만 나쁜 수면
    Inadequate,
    AdequateGood, // 6~8시간 좋은 수면
    Excessive,
}

// 기상 시간의 오차
public enum SleepRisingTimeVariance
{
    SmallGood,
    LargeBad,
}

// daytime에 낮잠 여부
public enum SleepDaytimeNap
{
    YesBad,
    NoGood,
}

public readonly struct SleepReport
{
    public SleepAmount sleepAmount;
    public SleepRisingTimeVariance sleepRisingTimeVariance;
    public SleepDaytimeNap sleepDaytimeNap;

    public SleepReport(SleepAmount sleepAmount, SleepRisingTimeVariance sleepRisingTimeVariance, SleepDaytimeNap sleepDaytimeNap)
    {
        this.sleepAmount = sleepAmount;
        this.sleepRisingTimeVariance = sleepRisingTimeVariance;
        this.sleepDaytimeNap = sleepDaytimeNap;
    }
}

public readonly struct ActivityReport
{
    public double sctiveEnergyBurnedGoalAchieved;
    public double exerciseTimeGoalAchieved;
    public double standHoursGoalAchieved;

    public ActivityReport(double activeEnergyBurnedGoalAchieved, double exerciseTimeGoalAchieved, double standHoursGoalAchieved)
    {
        this.activeEnergyBurnedGoalAchieved = activeEnergyBurnedGoalAchieved;
        this.exerciseTimeGoalAchieved = exerciseTimeGoalAchieved;
        this.standHoursGoalAchieved = standHoursGoalAchieved;
    }
}

public readonly struct HealthReport
{
    public SleepReport sleepReport;
    public ActivityReport activityReport;

    public HealthReport(SleepReport sleepReport, ActivityReport activityReport)
    {
        this.sleepReport = sleepReport;
        this.activityReport = activityReport;
    }
}

public static class HealthDataAnalyzer
{
    public static HealthReport GetDailyReport(DateTime startDate, int days)
    {
        HealthDataStore.Load(); // 밖에서 해야 하나?
        if (!HealthDataStore.Loaded())
        {
            Debug.Log("HealthDataStore is not loaded");
            return new HealthReport(new SleepReport(SleepAmount.Zero, SleepRisingTimeVariance.LargeBad, SleepDaytimeNap.YesBad), new ActivityReport(0, 0, 0));
        }
        endDate = startDate.AddDays(days);
        SleepReport sleepReport = GetSleepReport(startDate, endDate);
        ActivityReport activityReport = GetActivityReport(startDate, endDate);
        return new HealthReport(sleepReport, activityReport);
    }

    private static SleepReport GetSleepReport(DateTime startDate, DateTime endDate)
    {
        SleepSample[] sleepSamples = HealthDataStore.GetSleepSamples(startDate, endDate);
        if (sleepSamples is null || sleepSamples.Length == 0)
        {
            Debug.Log("SleepSamples is null or empty");
            return new SleepReport(SleepAmount.Zero, SleepRisingTimeVariance.LargeBad, SleepDaytimeNap.YesBad);
        }

        List<(DateTime, DataTime)> consecutiveSleeps = new List<(DateTime, DataTime)>();
        TimeSpan padSpan = TimeSpan.FromMinutes(30) / 2;

        foreach (SleepSample sleepSample in sleepSamples)
        {
            if (sleepSample.Type == SleepType.InBed || sleepSample.Type == SleepType.Awake)
            {
                continue;
            }
            bool found = false;
            for (int i = 0; i < consecutiveSleeps.Count; i++)
            {
                (DateTime, DateTime) (sDate, eDate) = consecutiveSleeps[i];
                if CheckOverlap(sDate - padSpan, eDate + padSpan, sleepSample.startDate - padSpan, sleepSample.endDate + padSpan)
                {
                    consecutiveSleeps[i] = (sDate < sleepSample.startDate ? sDate : sleepSample.startDate, eDate > sleepSample.endDate ? eDate : sleepSample.endDate);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                consecutiveSleeps.Add((sleepSample.startDate, sleepSample.endDate));
            }
        }

        List<(DateTime, DataTime)> latestConsecutiveSleeps = new List<(DateTime, DataTime)>();
        foreach ((DateTime, DateTime) (sDate, eDate) in consecutiveSleeps)
        {
            if (sDate > endDate - TimeSpan.FromDays(1))
            {
                latestConsecutiveSleeps.Add((sDate, eDate));
            }
        }

        int latestLongestSleepIdx = 0;
        TimeSpan longestSpan = TimeSpan.Zero;
        for (int i = 0; i < latestConsecutiveSleeps.Count; i++)
        {
            (DateTime, DateTime) (sDate, eDate) = latestConsecutiveSleeps[i];
            TimeSpan span = eDate - sDate;
            if (span > longestSpan)
            {
                longestSpan = span;
                latestLongestSleepIdx = i;
            }
        }

        foreach ((DateTime, DateTime) (sDate, eDate) in latestConsecutiveSleeps)
        {
            TimeSpan span = eDate - sDate;
            if (span > longestSpan)
            {
                longestSpan = span;
                latestLongestSleep = (sDate, eDate);
            }
        }
        foreach ((DateTime, DateTime) (sDate, eDate) in consecutiveSleeps)
        {
            if (sDate > endDate - TimeSpan.FromDays(1))
            {
                latestTotalSleepTime += (eDate - sDate).TotalHours;
            }
        }

        SleepAmount sleepAmount = GetSleepAmount(totalSleepTime);
        SleepRisingTimeVariance sleepRisingTimeVariance = GetSleepRisingTimeVariance(sleepSamples);
        SleepDaytimeNap sleepDaytimeNap = GetSleepDaytimeNap(sleepSamples);
        return new SleepReport(sleepAmount, sleepRisingTimeVariance, sleepDaytimeNap);
    }

    private static SleepAmount GetSleepAmount(double totalSleepTime)
    {
        if (totalSleepTime == 0)
        {
            return SleepAmount.Zero;
        }
        else if (totalSleepTime < 3)
        {
            return SleepAmount.VeryInadequateBad;
        }
        else if (totalSleepTime < 6)
        {
            return SleepAmount.Inadequate;
        }
        else if (totalSleepTime < 8)
        {
            return SleepAmount.AdequateGood;
        }
        else
        {
            return SleepAmount.Excessive;
        }
    }

    private static SleepRisingTimeVariance GetSleepRisingTimeVariance(SleepSample[] sleepSamples)
    {
        double totalRisingTimeVariance = 0;
        foreach (SleepSample sleepSample in sleepSamples)
        {
            totalRisingTimeVariance += (sleepSample.endDate - sleepSample.startDate).TotalHours;
        }
        if (totalRisingTimeVariance == 0)
        {
            return SleepRisingTimeVariance.LargeBad;
        }
        else
        {
            return SleepRisingTimeVariance.SmallGood;
        }
    }

    private static bool CheckOverlap(DateTime min1, DateTime max1, DateTime min2, DateTime max2)
    {
        return min1 <= min2 && min2 <= max1;
    }

    private static ActivityReport GetActivityReport(DateTime startDate, DateTime endDate)
    {
        ActivitySample[] activitySamples = HealthDataStore.GetActivitySamples(startDate, endDate);
        if (activitySamples is null || activitySamples.Length == 0)
        {
            Debug.Log("ActivitySamples is null or empty");
            return new ActivityReport(0, 0, 0);
        }

        int latestIdx = 0;
        for (int i = 0; i < activitySamples.Length; i++)
        {
            if (activitySamples[i].Date > activitySamples[latestIdx].Date)
            {
                latestIdx = i;
            }
        }

        ActivitySample latestSample = activitySamples[latestIdx];
        double activeEnergyBurnedGoalAchieved = latestSample.ActiveEnergyBurnedGoalInKcal != 0 ? latestSample.ActiveEnergyBurnedInKcal / latestSample.ActiveEnergyBurnedGoalInKcal : 0;
        double exerciseTimeGoalAchieved = latestSample.ExerciseTimeGoalInMinutes != 0 ? latestSample.ExerciseTimeInMinutes / latestSample.ExerciseTimeGoalInMinutes : 0;
        double standHoursGoalAchieved = latestSample.StandHoursGoal != 0 ? latestSample.StandHours / latestSample.StandHoursGoal : 0;

        return new ActivityReport(activeEnergyBurnedGoalAchieved, exerciseTimeGoalAchieved, standHoursGoalAchieved);
    }
}