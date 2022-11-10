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
public enum SleepRiseTimeVariance
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
    public SleepRiseTimeVariance sleepRiseTimeVariance;
    public SleepDaytimeNap sleepDaytimeNap;

    public SleepReport(SleepAmount sleepAmount, SleepRiseTimeVariance sleepRiseTimeVariance, SleepDaytimeNap sleepDaytimeNap)
    {
        this.sleepAmount = sleepAmount;
        this.sleepRiseTimeVariance = sleepRiseTimeVariance;
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
            return new HealthReport(new SleepReport(SleepAmount.Zero, SleepRiseTimeVariance.LargeBad, SleepDaytimeNap.YesBad), new ActivityReport(0, 0, 0));
        }
        SleepReport sleepReport = GetSleepReport(startDate, days);
        ActivityReport activityReport = GetActivityReport(startDate, days);
        return new HealthReport(sleepReport, activityReport);
    }

    private static SleepReport GetSleepReport(DateTime startDate, int days)
    {
        DateTime endDate = startDate.AddDays(days);
        SleepSample[] sleepSamples = HealthDataStore.GetSleepSamples(startDate, endDate);
        if (sleepSamples is null || sleepSamples.Length == 0)
        {
            Debug.Log("SleepSamples is null or empty");
            return new SleepReport(SleepAmount.Zero, SleepRiseTimeVariance.LargeBad, SleepDaytimeNap.YesBad);
        }

        List<(DateTime, DataTime)> consecutiveSleeps = new List<(DateTime, DataTime)>();
        TimeSpan padSpan = TimeSpan.FromMinutes(30) / 2; // 30분 환승 룰

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

        double totalSleepTime = 0;
        foreach ((DateTime, DateTime) (sDate, eDate) in consecutiveSleeps)
        {
            totalSleepTime += (eDate - sDate).TotalHours;
        }

        List<(DateTime, DataTime)>[] consecutiveSleepsByDay = new List<(DateTime, DataTime)>[days];
        for (int i = 0; i < days; i++)
        {
            consecutiveSleepsByDay[i] = new List<(DateTime, DataTime)>();
        }
        foreach ((DateTime, DataTime) (sDate, eDate) in consecutiveSleeps)
        {
            DateTime mDate = sDate + (eDate - sDate) / 2;
            for (int i = 0; i < days; i++)
            {
                // if (CheckOverlap(startDate.AddDays(i), startDate.AddDays(i + 1), sDate, eDate))
                if (mDate > startDate.AddDays(i) && mDate < startDate.AddDays(i + 1))
                {
                    consecutiveSleepsByDay[i].Add((sDate, eDate));
                    break;
                }
            }
        }

        int[] longestConsecutiveSleepIdxsByDay = new int[days];
        for (int i = 0; i < days; i++)
        {
            int longestIdx = 0;
            for (int j = 1; j < consecutiveSleepsByDay[i].Count; j++)
            {
                (DateTime, DataTime) (sDate, eDate) = consecutiveSleepsByDay[i][j];
                (DateTime, DataTime) (longestSDate, longestEDate) = consecutiveSleepsByDay[i][longestIdx];
                if (eDate - sDate > longestEDate - longestSDate)
                {
                    longestIdx = j;
                }
            }
            if (consecutiveSleepsByDay[i].Count > 0)
            {
                longestConsecutiveSleepIdxsByDay[i] = longestIdx;
            }
            else // 어떤 날에는 잠이 없을 수도
            {
                longestConsecutiveSleepIdxsByDay[i] = -1;
            }
        }

        List<(DateTime, DateTime)> longestConsecutiveSleeps = new List<(DateTime, DateTime)>();
        for (int i = 0; i < days; i++)
        {
            if (longestConsecutiveSleepIdxsByDay[i] != -1)
            {
                longestConsecutiveSleeps.Add(consecutiveSleepsByDay[i][longestConsecutiveSleepIdxsByDay[i]]);
            }
        }

        SleepAmount sleepAmount = GetSleepAmount(totalSleepTime);
        SleepRiseTimeVariance sleepRiseTimeVariance = GetSleepRiseTimeVariance(longestConsecutiveSleeps, days);
        SleepDaytimeNap sleepDaytimeNap = GetSleepDaytimeNap(consecutiveSleepsByDay[days - 1], longestConsecutiveSleepIdxsByDay[days - 1]);
        return new SleepReport(sleepAmount, sleepRiseTimeVariance, sleepDaytimeNap);
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

    private static SleepRiseTimeVariance GetSleepRiseTimeVariance(List<(DateTime, DateTime)> longestConsecutiveSleeps, int days)
    {
        if (longestConsecutiveSleeps.Count == 0)
        {
            return SleepRiseTimeVariance.LargeBad; // 잠을 아예 자지 않은 경우
        }
        else if (longestConsecutiveSleeps.Count == 1)
        {
            if (days == 1) // 첫날에는 데이터가 하나만 있어도 OK
            {
                return SleepRiseTimeVariance.SmallGood;
            }
            else
            {
                return SleepRiseTimeVariance.LargeBad;
            }
        }
        else
        {
            List<double> riseTimes = new List<double>();
            foreach ((DateTime, DateTime) (sDate, eDate) in longestConsecutiveSleeps)
            {
                riseTimes.Add(eDate.TimeOfDay.TotalHours);
            }
            double totalVariance = 0;
            for (int i = 0; i < riseTimes.Count - 1; i++)
            {
                double difference = Math.Abs(riseTimes[i] - riseTimes[i + 1]);
                if (difference > 12)
                {
                    difference = 24 - difference;
                }
                totalVariance += difference;
            }
            double averageVariance = totalVariance / (riseTimes.Count - 1);
            if (averageVariance < 1.0) // 1시간 이내의 variation
            {
                return SleepRiseTimeVariance.SmallGood;
            }
            else
            {
                return SleepRiseTimeVariance.LargeBad;
            }
        }
    }

    private static SleepDaytimeNap GetSleepDaytimeNap(List<(DateTime, DateTime)> sleeps, int longestSleepIdx)
    {
        if (sleeps.Count == 0 || longestSleepIdx == -1)
        {
            return SleepDaytimeNap.YesBad;
        }
        else
        {
            (DateTime, DateTime) (sDate, eDate) = sleeps[longestSleepIdx];
            (DateTime, DateTime) dayTime = (sDate - TimeSpan.FromHours(8), sDate);
            double totalDaytimeNap = 0;
            for (int i = 0; i < sleeps.Count; i++)
            {
                if (i == longestSleepIdx)
                {
                    continue;
                }
                (DateTime, DateTime) (sDate, eDate) = sleeps[i];
                if (CheckOverlap(dayTime.Item1, dayTime.Item2, sDate, eDate))
                {
                    totalDaytimeNap += CalcOverlap(dayTime.Item1, dayTime.Item2, sDate, eDate).TotalHours;
                }
            }
            if (totalDaytimeNap > 0.5) // 30분 이상 daytime 수면
            {
                return SleepDaytimeNap.YesBad;
            }
            return SleepDaytimeNap.NoGood;
        }
    }

    private static bool CheckOverlap(DateTime min1, DateTime max1, DateTime min2, DateTime max2)
    {
        return min1 <= min2 && min2 <= max1;
    }

    private static TimeSpan CalcOverlap(DateTime min1, DateTime max1, DateTime min2, DateTime max2)
    {
        DateTime maxMin = min1 > min2 ? min1 : min2;
        DateTime minMax = max1 < max2 ? max1 : max2;
        return minMax - maxMin > TimeSpan.Zero ? minMax - maxMin : TimeSpan.Zero;
    }

    private static ActivityReport GetActivityReport(DateTime startDate, int days)
    {
        DateTime endDate = startDate.AddDays(days);
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