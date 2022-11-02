using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NativePlugin.HealthData;
using System;
using Unity.VisualScripting;

[Serializable]
public struct SleepSampleTest
{
    public int StartYear;
    public int StartMonth;
    public int StartDay;
    public int StartHour;
    public int StartMinute;
    public int StartSecond;

    public int EndYear;
    public int EndMonth;
    public int EndDay;
    public int EndHour;
    public int EndMinute;
    public int EndSecond;
}
public class UI_SleepGraph : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private List<SleepSampleTest> time = new List<SleepSampleTest>();
    [SerializeField] private float upSpeed = 5;
    private List<Slider> sliders = new List<Slider>();
    //NativeLoadData nativeLoad = new NativeLoadData();
    private void Start()
    {
        //nativeLoad.LoadNativeData();

        for (int i = 0; i < content.transform.childCount; i++)
        {
            sliders.Add(content.transform.GetChild(i).GetComponent<Slider>());
        }
    }
    public void OnClick()
    {
        //Date time week Day 알아내는 함수 => DateTime.DayOfWeek
        if (time.Count <= 0) return;
        for (int i = 0; i < content.transform.childCount; i++)
        {
            if (time.Count - 1 - i < 0)
                break;
            sliders[i].maxValue = 86400;
            int start = time[i].StartHour * 3600 + time[i].StartMinute * 60 + time[i].StartSecond;

            int end = time[i].EndHour * 3600 + time[i].EndMinute * 60 + time[i].EndSecond;
            //Debug.Log($"endDate.Hour = {time[i].EndHour}\n endDate.Minute = {time[i].EndMinute}\nendDate.Second = {time[i].EndSecond}");
            //Debug.Log($"start = {start}\n end = {end}");

            //초기화
            sliders[i].value = 0;
            //그래프 움직이기
            StartCoroutine(GraphMove(Mathf.Abs(end - start), i));

            //Debug.Log($"value = {Mathf.Abs(end - start)}");
            content.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = $"{time[i].EndYear}년\n{time[i].EndMonth}월\n{time[i].EndDay}일";
        }
    }
    public IEnumerator GraphMove(int val, int idx)
    {
        while (true)
        {
            sliders[idx].value = Mathf.Lerp(sliders[idx].value, val, Time.deltaTime * upSpeed);
            if (sliders[idx].value >= val - 1)
                yield break;
            yield return null;  
        }
    }
}
