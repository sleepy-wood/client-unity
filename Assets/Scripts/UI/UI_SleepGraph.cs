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
    [SerializeField] private Image pi;
    [SerializeField] private List<SleepSampleTest> time = new List<SleepSampleTest>();
    [SerializeField] private float upSpeed = 5;
    [SerializeField] private UILineRenderer lineRenderer;

    private List<Slider> sliders = new List<Slider>();
    private List<bool> VisitedList = new List<bool>();
    List<Vector2> graphPoints = new List<Vector2>();

    private float loadingTime = 1.5f;
    private float curTime = 0;
    private bool isOnce = false;
    //NativeLoadData nativeLoad = new NativeLoadData();
    private void Start()
    {
        //nativeLoad.LoadNativeData();

        for (int i = 0; i < content.transform.childCount; i++)
        {
            sliders.Add(content.transform.GetChild(i).GetComponent<Slider>());
            VisitedList.Add(false);
        }
        for (int i = 0; i < time.Count; i++)
        {
            graphPoints.Add(Vector2.zero);
        }
    }
    private void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > loadingTime & !isOnce)
        {
            isOnce = true;
            OnDrawGraph();
        }
        if (!lineRenderer.gameObject.activeSelf)
        {
            CheckVisited();
        }
    }
    public void CheckVisited()
    {
        for (int i = 0; i < time.Count; i++)
        {
            if (!VisitedList[i])
            {
                return;
            }
        }

        lineRenderer.points = graphPoints;
        lineRenderer.gameObject.SetActive(true);
    }
    public void OnDrawGraph()
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

        int starthour = time[time.Count - 1].StartHour == 0 ? 24 : time[time.Count - 1].StartHour;
        int endhour = time[time.Count - 1].EndHour == 0 ? 24 : time[time.Count - 1].EndHour;

        starthour = time[time.Count - 1].StartHour > 12 ? time[time.Count - 1].StartHour - 12 : time[time.Count - 1].StartHour;
        endhour = time[time.Count - 1].EndHour > 12 ? time[time.Count - 1].EndHour - 12 : time[time.Count - 1].EndHour;

        int startStartDay = starthour * 3600 + time[time.Count - 1].StartMinute * 60 + time[time.Count - 1].StartSecond;
        int endEndDay = endhour * 3600 + time[time.Count - 1].EndMinute * 60 + time[time.Count - 1].EndSecond;

        pi.gameObject.GetComponent<RectTransform>().localEulerAngles = new Vector3(180, 180, -((float)startStartDay / 46860f) * 375f);
        int hour = endEndDay - startStartDay;
        hour = hour < 0 ? 46860 + hour : hour;
        StartCoroutine(PiMove((hour / 46860f)));
    }
    public IEnumerator PiMove(float amount)
    {
        while (true)
        {
            pi.fillAmount = Mathf.Lerp(pi.fillAmount, amount, Time.deltaTime);
            if(pi.fillAmount > amount  - 0.001f)
            {
                yield break;
            }
            yield return null;
        }
    }
    public IEnumerator GraphMove(int val, int idx)
    {
        while (true)
        {
            sliders[idx].value = Mathf.Lerp(sliders[idx].value, val, Time.deltaTime * upSpeed);
            if (sliders[idx].value >= val - 1)
            {
                //lineRenderer.points
                Vector2 position;
                position.x = content.transform.GetChild(idx).GetComponent<RectTransform>().anchoredPosition.x + 50;
                position.y = 600 + transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition.y + 480 * content.transform.GetChild(idx).GetChild(2).GetChild(0).GetComponent<RectTransform>().anchorMax.x;
                graphPoints[idx] = position;
                VisitedList[idx] = true;
                
                yield break;
            }
            yield return null;  
        }

    }
}
