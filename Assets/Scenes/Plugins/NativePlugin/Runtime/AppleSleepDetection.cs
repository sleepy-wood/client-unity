using System;
using System.Runtime.InteropServices;

namespace NativePlugin.SleepDetection.IOS
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AppleSleepDetectionResult
    {
        public readonly bool isStationary;
        public readonly double accelerationMagnitudeInG;
        public readonly double heartRateStandardDeviationInBpm;
        public readonly double heartRateAverageInBpm;
        public readonly double heartRateIntervalStandardDeviationInSeconds;
        public readonly double heartRateIntervalAverageInSeconds;
        public readonly double networkOutput;
        public readonly int sleepState;
    }
}
