using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NativePlugin.HealthData;
using System;

public class UI_SleepGraph : MonoBehaviour
{
    [SerializeField] private GameObject content;
    private List<Slider> sliders = new List<Slider>();
    public SleepSample[] samplesData;
    NativeLoadData nativeLoad = new NativeLoadData();
    private void Start()
    {
        nativeLoad.LoadNativeData();

    }
    public void OnClick()
    {
        if (DataTemporary.samples.Length <= 0) return;
        samplesData = DataTemporary.samples;
        for (int i = 0; i < content.transform.childCount; i++)
        {
            if (samplesData.Length - 1 - i < 0)
                break;
            sliders.Add(content.transform.GetChild(i).GetComponent<Slider>());
            sliders[i].maxValue = 86400;
            DateTime startDate = samplesData[samplesData.Length - 1 - i].StartDate;
            int start = startDate.Hour * 3600 + startDate.Minute * 60 + startDate.Second;
            DateTime endDate = samplesData[samplesData.Length - 1 - i].EndDate;
            int end = endDate.Hour * 3600 + endDate.Minute * 60 + endDate.Second;
            Debug.Log($"endDate.Hour = {endDate.Hour}\n endDate.Minute = {endDate.Minute}\nendDate.Second = {endDate.Second}");
            Debug.Log($"start = {start}\n end = {end}");
            sliders[i].value = end - start;
        }
    }
}
