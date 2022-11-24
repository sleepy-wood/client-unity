using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using NativePlugin.IOS;
using NativePlugin.HealthData.IOS;
using NativePlugin.HealthData.Mock;

namespace NativePlugin.HealthData
{
    public enum SleepType : int
    {
        InBed = 0,
        AsleepUnspecified = 1,
        Awake = 2,
        AsleepCore = 3,
        AsleepDeep = 4,
        AsleepREM = 5,
    }

    public struct SleepSample
    {
        public readonly DateTime StartDate;
        public readonly DateTime EndDate;
        public readonly SleepType Type;

        public SleepSample(DateTime startDate, DateTime endDate, SleepType type)
        {
            StartDate = startDate;
            EndDate = endDate;
            Type = type;
        }
    }

    public struct ActivitySample
    {
        public readonly DateTime Date;

        // public readonly bool IsMoveMode;
        // public readonly double MoveTimeInMinutes;
        // public readonly double MoveTimeGoalInMinutes;
        public readonly double ActiveEnergyBurnedInKcal;
        public readonly double ActiveEnergyBurnedGoalInKcal;
        public readonly int ExerciseTimeInMinutes;
        public readonly int ExerciseTimeGoalInMinutes;
        public readonly int StandHours;
        public readonly int StandHoursGoal;

        public ActivitySample(
            DateTime date,
            // bool isMoveMode,
            // double moveTimeInMinutes,
            // double moveTimeGoalInMinutes,
            double activeEnergyBurnedInKcal,
            double activeEnergyBurnedGoalInKcal,
            int exerciseTimeInMinutes,
            int exerciseTimeGoalInMinutes,
            int standHours,
            int standHoursGoal
        )
        {
            Date = date;
            // IsMoveMode = isMoveMode;
            // MoveTimeInMinutes = moveTimeInMinutes;
            // MoveTimeGoalInMinutes = moveTimeGoalInMinutes;
            ActiveEnergyBurnedInKcal = activeEnergyBurnedInKcal;
            ActiveEnergyBurnedGoalInKcal = activeEnergyBurnedGoalInKcal;
            ExerciseTimeInMinutes = exerciseTimeInMinutes;
            ExerciseTimeGoalInMinutes = exerciseTimeGoalInMinutes;
            StandHours = standHours;
            StandHoursGoal = standHoursGoal;
        }
    }

    public static class HealthData
    {
        public delegate void RequestAuthCompletedHandler(bool granted);
        public static event RequestAuthCompletedHandler RequestAuthCompleted;

        public delegate void QuerySleepSamplesCompletedHandler(SleepSample[] samples);
        public static event QuerySleepSamplesCompletedHandler QuerySleepSamplesCompleted;

        public delegate void QueryActivitySamplesCompletedHandler(ActivitySample[] samples);
        public static event QueryActivitySamplesCompletedHandler QueryActivitySamplesCompleted;

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern bool iOS_healthDataIsAvailable();

        [DllImport("__Internal")]
        private static extern void iOS_healthDataRequestAuth(
            AppleSuccessCallback<bool> onSuccess,
            AppleErrorCallback onError
        );

        [DllImport("__Internal")]
        private static extern void iOS_healthDataQuerySleepSamples(
            double startDateInSeconds,
            double endDateInSeconds,
            int maxNumSamples,
            AppleSuccessCallback<bool> onSuccess,
            AppleErrorCallback onError
        );

        [DllImport("__Internal")]
        private static extern int iOS_healthDataGetSleepSamplesCount();

        [DllImport("__Internal")]
        private static extern AppleSleepSample iOS_healthDataGetSleepSampleAtIndex(int index);

        [DllImport("__Internal")]
        private static extern void iOS_healthDataQueryActivitySamples(
            double startDateInSeconds,
            double endDateInSeconds,
            AppleSuccessCallback<bool> onSuccess,
            AppleErrorCallback onError
        );

        [DllImport("__Internal")]
        private static extern int iOS_healthDataGetActivitySamplesCount();

        [DllImport("__Internal")]
        private static extern AppleActivitySample iOS_healthDataGetActivitySampleAtIndex(int index);
#endif

        public static bool IsAvailable()
        {
#if UNITY_IOS
            return iOS_healthDataIsAvailable();
#else
            Debug.Log("IsAvailable: Unsupported Platform");
            return MockHealthData.IsAvailable();
#endif
        }

        public static void RequestAuth()
        {
#if UNITY_IOS
            iOS_healthDataRequestAuth(AppleOnRequestAuthCompleted, AppleOnRequestAuthFailed);
#else
            Debug.Log("RequestAuth: Unsupported Platform");
            RequestAuthCompleted?.Invoke(MockHealthData.RequestAuth());
#endif
        }

#if UNITY_IOS
        [MonoPInvokeCallback(typeof(AppleSuccessCallback<bool>))]
        private static void AppleOnRequestAuthCompleted(bool granted)
        {
            Debug.Log("AppleOnRequestAuthCompleted: " + granted);
            RequestAuthCompleted?.Invoke(granted);
        }

        [MonoPInvokeCallback(typeof(AppleErrorCallback))]
        private static void AppleOnRequestAuthFailed(AppleInteropError error)
        {
            // TODO: Handle error?
        }
#endif

        public static void QuerySleepSamples(
            DateTime startDate,
            DateTime endDate,
            int maxNumSamples
        )
        {
            double startDateInSeconds = HealthUtils.ConvertToUnixTimestamp(startDate);
            double endDateInSeconds = HealthUtils.ConvertToUnixTimestamp(endDate);
#if UNITY_IOS
            iOS_healthDataQuerySleepSamples(
                startDateInSeconds,
                endDateInSeconds,
                maxNumSamples,
                AppleOnQuerySleepSamplesCompleted,
                AppleOnQuerySleepSamplesFailed
            );
#else
            Debug.Log("QuerySleepSamples: Unsupported Platform");
            QuerySleepSamplesCompleted?.Invoke(
                MockHealthData.QuerySleepSamples(startDate, endDate, maxNumSamples)
            );
#endif
        }

#if UNITY_IOS
        [MonoPInvokeCallback(typeof(AppleSuccessCallback<bool>))]
        private static void AppleOnQuerySleepSamplesCompleted(bool success)
        {
            Debug.Log("AppleOnQuerySleepSamplesCompleted: " + success);
            if (success)
            {
                int numSamples = iOS_healthDataGetSleepSamplesCount();
                SleepSample[] res = new SleepSample[numSamples];
                for (int i = 0; i < numSamples; i++)
                {
                    AppleSleepSample sample = iOS_healthDataGetSleepSampleAtIndex(i);
                    DateTime startDate = HealthUtils.ConvertFromUnixTimestamp(
                        sample.startDateInSeconds
                    );
                    DateTime endDate = HealthUtils.ConvertFromUnixTimestamp(
                        sample.endDateInSeconds
                    );
                    SleepType type = (SleepType)sample.value;
                    // Debug.Log(
                    //     "AppleOnQuerySleepSamplesCompleted: "
                    //         + startDate
                    //         + " - "
                    //         + endDate
                    //         + " - "
                    //         + type
                    // );
                    res[i] = new SleepSample(startDate, endDate, type);
                }
                QuerySleepSamplesCompleted?.Invoke(res);
            }
            else
            {
                QuerySleepSamplesCompleted?.Invoke(null);
            }
        }

        [MonoPInvokeCallback(typeof(AppleErrorCallback))]
        private static void AppleOnQuerySleepSamplesFailed(AppleInteropError error)
        {
            // TODO: Handle error?
        }
#endif

        public static void QueryActivitySamples(DateTime startDate, DateTime endDate)
        {
            double startDateInSeconds = HealthUtils.ConvertToUnixTimestamp(startDate);
            double endDateInSeconds = HealthUtils.ConvertToUnixTimestamp(endDate);
#if UNITY_IOS
            iOS_healthDataQueryActivitySamples(
                startDateInSeconds,
                endDateInSeconds,
                AppleOnQueryActivitySamplesCompleted,
                AppleOnQueryActivitySamplesFailed
            );
#else
            Debug.Log("QueryActivitySamples: Unsupported Platform");
            QueryActivitySamplesCompleted?.Invoke(
                MockHealthData.QueryActivitySamples(startDate, endDate)
            );
#endif
        }

#if UNITY_IOS
        [MonoPInvokeCallback(typeof(AppleSuccessCallback<bool>))]
        private static void AppleOnQueryActivitySamplesCompleted(bool success)
        {
            Debug.Log("AppleOnQueryActivitySamplesCompleted: " + success);
            if (success)
            {
                int numSamples = iOS_healthDataGetActivitySamplesCount();
                ActivitySample[] res = new ActivitySample[numSamples];
                for (int i = 0; i < numSamples; i++)
                {
                    AppleActivitySample sample = iOS_healthDataGetActivitySampleAtIndex(i);
                    DateTime date = HealthUtils.ConvertFromUnixTimestamp(sample.dateInSeconds);
                    // Debug.Log(
                    //     "AppleOnQueryActivitySamplesCompleted: "
                    //         + date
                    //         + " - "
                    //         + sample.standHours
                    //         + " - "
                    //         + sample.standHoursGoal
                    // );
                    res[i] = new ActivitySample(
                        date,
                        sample.activeEnergyBurnedInKcal,
                        sample.activeEnergyBurnedGoalInKcal,
                        sample.exerciseTimeInMinutes,
                        sample.exerciseTimeGoalInMinutes,
                        sample.standHours,
                        sample.standHoursGoal
                    );
                }
                QueryActivitySamplesCompleted?.Invoke(res);
            }
            else
            {
                QueryActivitySamplesCompleted?.Invoke(null);
            }
        }

        [MonoPInvokeCallback(typeof(AppleErrorCallback))]
        private static void AppleOnQueryActivitySamplesFailed(AppleInteropError error)
        {
            // TODO: Handle error?
        }
#endif
    }

    public static class HealthUtils
    {
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp).ToLocalTime();
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return diff.TotalSeconds;
        }
    }
}
