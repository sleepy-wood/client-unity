using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativePlugin.SleepDetection;
using UnityEngine.UI;

public class UI_SleepDetection : MonoBehaviour
{
    [SerializeField] private Text resultText_Move;
    [SerializeField] private Text resultText_Ac;
    [SerializeField] private Text resultText_Heart;
    [SerializeField] private Text resultText_Net;

    private float curTime = 0;
    private float showTime = 1;
    private SleepDetectionResult sleepDetectionResult;

    private void Start()
    {
        sleepDetectionResult = SleepDetection.DetectSleep();
        if(sleepDetectionResult.SleepState == SleepState.Awake)
        {
            resultText_Move.text = "움직임 여부: " + sleepDetectionResult.IsStationary;
            resultText_Ac.text = "가속도계 측정값: " + sleepDetectionResult.AccelerationMagnitudeInG;
            resultText_Heart.text = "심장박동 표준 편차: " + sleepDetectionResult.HeartRateStandardDeviationInBpm;
            resultText_Net.text = "네트워크 출력: "  + (-Mathf.Abs((float)sleepDetectionResult.NetworkOutput));
        }
    }
    private void Update()
    {
        curTime += Time.deltaTime;
        if(curTime >= showTime)
        {
            sleepDetectionResult = SleepDetection.DetectSleep();
            Debug.Log(sleepDetectionResult.SleepState);
            if (sleepDetectionResult.SleepState == SleepState.Awake)
            {
                curTime = 0;
                resultText_Move.text = "움직임 여부: " + sleepDetectionResult.IsStationary;
                resultText_Ac.text = "가속도계 측정값: " + sleepDetectionResult.AccelerationMagnitudeInG;
                resultText_Heart.text = "심장박동 표준 편차: " + sleepDetectionResult.HeartRateStandardDeviationInBpm;
                resultText_Net.text = "네트워크 출력: " + (-Mathf.Abs((float)sleepDetectionResult.NetworkOutput));
            }
        }
    }
}
