using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using NativePlugin.SleepDetection.IOS;

namespace NativePlugin.SleepDetection
{
    public enum SleepState : int
    {
        Unknown = -1,
        Awake = 0,
        Asleep = 1,
    }

    public struct SleepDetectionResult
    {
        public readonly bool IsStationary;
        public readonly double AccelerationMagnitudeInG;
        public readonly double HeartRateStandardDeviationInBpm;
        public readonly double HeartRateAverageInBpm;
        public readonly double HeartRateIntervalStandardDeviationInSeconds;
        public readonly double HeartRateIntervalAverageInSeconds;
        public readonly double NetworkOutput;
        public readonly SleepState SleepState;

        public SleepDetectionResult(
            bool isStationary,
            double accelerationMagnitudeInG,
            double heartRateStandardDeviationInBpm,
            double heartRateAverageInBpm,
            double heartRateIntervalStandardDeviationInSeconds,
            double heartRateIntervalAverageInSeconds,
            double networkOutput,
            SleepState sleepState
        )
        {
            IsStationary = isStationary;
            AccelerationMagnitudeInG = accelerationMagnitudeInG;
            HeartRateStandardDeviationInBpm = heartRateStandardDeviationInBpm;
            HeartRateAverageInBpm = heartRateAverageInBpm;
            HeartRateIntervalStandardDeviationInSeconds =
                heartRateIntervalStandardDeviationInSeconds;
            HeartRateIntervalAverageInSeconds = heartRateIntervalAverageInSeconds;
            NetworkOutput = networkOutput;
            SleepState = sleepState;
        }
    }

    public static class SleepDetection
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern bool iOS_sleepDetectionIsAvailable();

        [DllImport("__Internal")]
        private static extern AppleSleepDetectionResult iOS_sleepDetectionDetectSleep();
#endif

        public static bool IsAvailable()
        {
#if UNITY_IOS
            return iOS_sleepDetectionIsAvailable();
#else
            Debug.Log("SleepDetection.IsAvailable: Unsupported Platform");
            return true;
#endif
        }

        public static SleepDetectionResult DetectSleep()
        {
#if UNITY_IOS
            AppleSleepDetectionResult result = iOS_sleepDetectionDetectSleep();
            return new SleepDetectionResult(
                result.isStationary,
                result.accelerationMagnitudeInG,
                result.heartRateStandardDeviationInBpm,
                result.heartRateAverageInBpm,
                result.heartRateIntervalStandardDeviationInSeconds,
                result.heartRateIntervalAverageInSeconds,
                result.networkOutput,
                (SleepState)result.sleepState
            );
#else
            Debug.Log("SleepDetection.DetectSleep: Unsupported Platform");
            return new SleepDetectionResult(false, 0, 0, 0, 0, 0, 0, SleepState.Awake);
#endif
        }
    }
}
