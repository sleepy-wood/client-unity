using NativePlugin.HealthData;
using RuntimeInspectorNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Graph_SleepRecord : MonoBehaviour
{
    [Header("Priod")]
    [SerializeField] private RectTransform selectButton;
    [SerializeField] private Transform periodParent;
    [SerializeField] private Text sleep_averTime;
    [SerializeField] private Transform period_sleepGraph;

    [Space]
    [Header("CoreSleepType")]
    [SerializeField] private Text coreSleepText;
    [SerializeField] private GameObject coreSleep_Yesterday;
    [SerializeField] private GameObject coreSleep_Today;
    [Header("DeepSleepType")]
    [SerializeField] private Text deepSleepText;
    [SerializeField] private GameObject deepSleep_Yesterday;
    [SerializeField] private GameObject deepSleep_Today;
    [Header("REMSleepType")]
    [SerializeField] private Text remSleepText;
    [SerializeField] private GameObject remSleep_Yesterday;
    [SerializeField] private GameObject remSleep_Today;


    private SleepSample[] sleepsData;
    bool isOnce = false;
    private int startDay = 0;
    private int endDay = 0;
    private void Awake()
    {
        sleepsData = DataTemporary.samples;
    }
    public enum RecordDate
    {
        Day = 0,
        Week = 1,
        Month = 2,
        SixMonth = 3,
        Year = 4
    }

    private RecordDate curRecordDate = RecordDate.Day;
    void Start()
    {
        
    }
    void ChangeGraph(RecordDate record)
    {
        //버튼 글자 색 변경
        for(int i = 0; i < 5; i++)
        {
            if ((int)record == i)
            {
                periodParent.GetChild(2 + i).GetChild(0).gameObject.SetActive(false);
                periodParent.GetChild(2 + i).GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                periodParent.GetChild(2 + i).GetChild(0).gameObject.SetActive(true);
                periodParent.GetChild(2 + i).GetChild(1).gameObject.SetActive(false);
            }
        }

        //그래프 그리기
        switch ((int)record)
        {
            case 0:
                Graph_Day();
                break;
            case 1:
                Graph_Week();
                break;

        }
    }

    private int preDay = 0;
    TimeSpan day_totalTime;
    void Graph_Day()
    {
        List<TimeSpan> timeSpans = new List<TimeSpan>();
        TimeSpan today = new TimeSpan();
        for (int i = sleepsData.Length - 1; i >= 0; i--)
        {
            if (sleepsData[i].Type.ToString().Contains("Asleep"))
            {
                if (!isOnce)
                {
                    endDay = ReturnDayOfWeek(sleepsData[i].EndDate.DayOfWeek);
                    isOnce = true;
                }
                //두 시간의 중앙값을 알아내어 어느 날에 속하게 할 것인지 정하기
                TimeSpan diff = sleepsData[i].EndDate - sleepsData[i].StartDate;
                diff /= 2;
                var NewDate = new DateTime(
                    sleepsData[i].StartDate.Year,
                    sleepsData[i].StartDate.Month,
                    sleepsData[i].StartDate.Day + diff.Days,
                    sleepsData[i].StartDate.Hour + diff.Hours,
                    sleepsData[i].StartDate.Minute + diff.Minutes,
                    sleepsData[i].StartDate.Second + diff.Seconds
                    );
                //중앙값의 날의 요일
                startDay = ReturnDayOfWeek(NewDate.DayOfWeek);

                //갑자기 이전 계산한 preDay보다 startDay가 커진다면 - 저번주로 넘어감
                if (startDay > preDay)
                {
                    break;
                }

                //이번주
                else
                {
                    //오늘이면
                    if (startDay == endDay)
                    {
                        today += diff;
                    }
                    else
                    {
                        endDay = startDay;
                        timeSpans.Add(today);
                    }
                }
                preDay = startDay;
            }
        }
        for(int i = 0; i < timeSpans.Count; i++)
        {
            if(!period_sleepGraph.GetChild(i).gameObject.activeSelf)
                period_sleepGraph.GetChild(i).gameObject.SetActive(true);

            TimeSpan time = timeSpans[timeSpans.Count - i - 1];
            day_totalTime += time;
            period_sleepGraph.GetChild(i).GetComponent<Scrollbar>().size = (float)(time.TotalSeconds / 86400);
        }
        for(int i = timeSpans.Count - 1; i < period_sleepGraph.childCount; i++)
        {
            period_sleepGraph.GetChild(i).gameObject.SetActive(false);
        }
        day_totalTime /= timeSpans.Count;
        sleep_averTime.text = "평균: " + day_totalTime.Hours + "시간 " + day_totalTime.Minutes + "분";
    }

    TimeSpan week = new TimeSpan();
    TimeSpan week_totalTime;
    void Graph_Week()
    {
        int cnt = 0;
        List<TimeSpan> timeSpans = new List<TimeSpan>();
        for (int i = sleepsData.Length - 1; i >= 0; i--)
        {
            if (sleepsData[i].Type.ToString().Contains("Asleep"))
            {
                if (!isOnce)
                {
                    endDay = ReturnDayOfWeek(sleepsData[i].EndDate.DayOfWeek);
                    isOnce = true;
                }
                //두 시간의 중앙값을 알아내어 어느 날에 속하게 할 것인지 정하기
                TimeSpan diff = sleepsData[i].EndDate - sleepsData[i].StartDate;
                diff /= 2;
                var NewDate = new DateTime(
                    sleepsData[i].StartDate.Year,
                    sleepsData[i].StartDate.Month,
                    sleepsData[i].StartDate.Day + diff.Days,
                    sleepsData[i].StartDate.Hour + diff.Hours,
                    sleepsData[i].StartDate.Minute + diff.Minutes,
                    sleepsData[i].StartDate.Second + diff.Seconds
                    );
                //중앙값의 날의 요일
                startDay = ReturnDayOfWeek(NewDate.DayOfWeek);

                //갑자기 이전 계산한 preDay보다 startDay가 커진다면 - 저번주로 넘어감
                if (startDay > preDay)
                {
                    Debug.Log(cnt);
                    timeSpans.Add(week / cnt);
                    if (timeSpans.Count != 7)
                    {
                        week = new TimeSpan();
                        cnt = 0;
                    }
                    else
                        break;
                }
                if(preDay != startDay)
                {
                    cnt++;
                }
                week += diff;
                preDay = startDay;
            }
        }
        for (int i = 0; i < timeSpans.Count; i++)
        {
            if (!period_sleepGraph.GetChild(i).gameObject.activeSelf)
                period_sleepGraph.GetChild(i).gameObject.SetActive(true);

            TimeSpan time = timeSpans[timeSpans.Count - i - 1];
            week_totalTime += time;
            period_sleepGraph.GetChild(i).GetComponent<Scrollbar>().size = (float)(time.TotalSeconds / 86400);
        }
        week_totalTime /= timeSpans.Count;
        sleep_averTime.text = "평균: " + week_totalTime.Hours + "시간 " + week_totalTime.Minutes + "분";
    }
    
    public void OnClickChangeDate(int i)
    {
        switch (i)
        {
            case 0:
                curRecordDate = RecordDate.Day;
                break;
            case 1:
                curRecordDate = RecordDate.Week;
                break;
            case 2:
                curRecordDate = RecordDate.Month;
                break;
            case 3:
                curRecordDate = RecordDate.SixMonth;
                break;
            case 4:
                curRecordDate = RecordDate.Year;
                break;
            default:
                break;
        }

        ChangeGraph(curRecordDate);
    }

    int ReturnDayOfWeek(DayOfWeek dayOfWeek)
    {
        int day = 0;
        switch (sleepsData[sleepsData.Length - 1].EndDate.DayOfWeek)
        {
            case DayOfWeek.Monday:
                day = 1;
                break;
            case DayOfWeek.Tuesday:
                day = 2;
                break;
            case DayOfWeek.Wednesday:
                day = 3;
                break;
            case DayOfWeek.Thursday:
                day = 4;
                break;
            case DayOfWeek.Friday:
                day = 5;
                break;
            case DayOfWeek.Saturday:
                day = 6;
                break;
            case DayOfWeek.Sunday:
                day = 0;
                break;
            default:
                break;
        }
        return day;
    }

    private bool isStartYesterDay = false;
    private TimeSpan today;
    private TimeSpan yesterday;

    /// <summary>
    /// SleepType에 따라 어제와 오늘을 비교하는 결과 계산
    /// </summary>
    /// <param name="sleepType"></param>
    /// <param name="yesterDay"></param>
    /// <param name="toDay"></param>
    /// <param name="sleepDiffResult"></param>
    public void CalcSleep(SleepType sleepType, GameObject yesterDay, GameObject toDay, Text sleepDiffResult)
    {
        for (int i = sleepsData.Length - 1; i >= 0; i--)
        {
            if (sleepsData[i].Type == SleepType.AsleepREM)
            {
                if (!isOnce)
                {
                    endDay = ReturnDayOfWeek(sleepsData[i].EndDate.DayOfWeek);
                    isOnce = true;
                }
                //두 시간의 중앙값을 알아내어 어느 날에 속하게 할 것인지 정하기
                TimeSpan diff = sleepsData[i].EndDate - sleepsData[i].StartDate;
                diff /= 2;
                var NewDate = new DateTime(
                    sleepsData[i].StartDate.Year,
                    sleepsData[i].StartDate.Month,
                    sleepsData[i].StartDate.Day + diff.Days,
                    sleepsData[i].StartDate.Hour + diff.Hours,
                    sleepsData[i].StartDate.Minute + diff.Minutes,
                    sleepsData[i].StartDate.Second + diff.Seconds
                    );
                //중앙값의 날의 요일
                startDay = ReturnDayOfWeek(NewDate.DayOfWeek);

                if (!isStartYesterDay)
                {
                    //오늘이면
                    if (startDay == endDay)
                    {
                        today += diff;
                    }
                    else
                    {
                        isStartYesterDay = true;
                        //today 나타내기
                        endDay = startDay;
                    }
                }
                else
                {
                    //오늘이면
                    if (startDay == endDay)
                    {
                        yesterday += diff;
                    }
                    else
                    {
                        //yesterDay 나타내기
                        break;
                    }
                }
            }
        }
        if(today.TotalSeconds > yesterday.TotalSeconds)
        {
            sleepDiffResult.text = $"{sleepType}이 어제보다 늘었습니다";
        }
        else if(today.TotalSeconds < yesterday.TotalSeconds)
        {
            sleepDiffResult.text = $"{sleepType}이 어제보다 줄었습니다.";
        }
        else
        {
            sleepDiffResult.text = $"{sleepType}이 어제와 같습니다.";
        }
    }
}
