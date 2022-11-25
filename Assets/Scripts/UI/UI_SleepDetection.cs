using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativePlugin.SleepDetection;
using UnityEngine.UI;

public class UI_SleepDetection : MonoBehaviour
{
    [SerializeField] private Text resultText;

    private float curTime = 0;
    private float showTime = 1;
    private SleepDetectionResult sleepDetectionResult;

    private void Start()
    {
        sleepDetectionResult = SleepDetection.DetectSleep();
        if(sleepDetectionResult.SleepState == SleepState.Awake)
        {
            string t = "움직임 여부: " + sleepDetectionResult.IsStationary;
            t += "\n가속도계 측정값: " + sleepDetectionResult.AccelerationMagnitudeInG;
            t += "\n심장박동 표준 편차: " + sleepDetectionResult.HeartRateStandardDeviationInBpm;
            t += "\n네트워크 출력: "  + (-Mathf.Abs((float)sleepDetectionResult.NetworkOutput));

            resultText.text = t;
        }
    }
    private void Update()
    {
        curTime += Time.deltaTime;
        if(curTime >= showTime)
        {
            curTime = 0;
            string t = "움직임 여부: " + sleepDetectionResult.IsStationary;
            t += "\n가속도계 측정값: " + sleepDetectionResult.AccelerationMagnitudeInG;
            t += "\n심장박동 표준 편차: " + sleepDetectionResult.HeartRateStandardDeviationInBpm;
            t += "\n네트워크 출력: " + (-Mathf.Abs((float)sleepDetectionResult.NetworkOutput));

            resultText.text = t;
        }
    }
}
