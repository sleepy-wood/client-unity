using NativePlugin.HealthData;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Graph_Initial_Window : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private Text m_thisWeek_Aver;
    [SerializeField] private Text m_lastWeek_Aver;
    [SerializeField] private Scrollbar m_thisWeek_Scroll;
    [SerializeField] private Scrollbar m_lastWeek_Scroll;

    [Space]
    [Header("SleepRecord")]
    [SerializeField] private Text m_todaySleep;

    [Space]
    [Header("Activity")]
    [SerializeField] private Text m_Exercise_Time;
    [SerializeField] private Text m_Stand_Count;

    private SleepSample[] sleepsData;
    private int startDay = 0;
    private int endDay = 0;
    private TimeSpan today;
    private TimeSpan preWeek;
    private TimeSpan curWeek;
    private int preDay = 0;
    private void Awake()
    {
        sleepsData = DataTemporary.samples;
    }
    private bool isOnce = false;
    private bool startPreWeek = false;
    private bool endPreWeek = false;
    void Start()
    {
        HealthDataStore.Init();
    }

    bool once = false;

    void Update()
    {
        if (HealthDataStore.GetStatus() == HealthDataStoreStatus.Loaded && !once)
        {
            once = true;

            DateTime now = DateTime.Now;

            DateTime nowStart = now.AddDays(-1);
            ActivitySample[] activitySamples = HealthDataStore.GetActivitySamples(
                nowStart, now);

            if(activitySamples != null|| activitySamples.Length ==0)
            {
                Debug.LogError("ActivitySamples is null or empty");
                return;
            }

            //달성률 수치로 알려줌
            //HealthReport report = HealthDataAnalyzer.GetDailyReport(
            //    DateTime.Now,
            //    6
            //);
            int lastIdx = 0;

            for(int i = 0; i < activitySamples.Length; i++)
            {
                lastIdx = activitySamples[i].Date > activitySamples[lastIdx].Date ? i : lastIdx;
            }
            //수면 관련 계산
            Calc_Sleep();
            //활동 관련 계산
            Calc_Activity(activitySamples[lastIdx]);
        }
    }
    /// <summary>
    /// Sleep 관련 계산 - 저번주 평균 수면 시간, 이번주 평균 수면시간, 오늘 수면 시간
    /// </summary>
    void Calc_Sleep()
    {
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
                    //이미 저번주로 넘어간상태 였다면 저저번주로 넘어갔음
                    endPreWeek = startPreWeek == true ? true : false;
                    startPreWeek = true;
                }
                //저저번주
                if (endPreWeek)
                {
                    //나가기
                    break;
                }
                //저번주
                else if (startPreWeek)
                {
                    preWeek += diff;
                }
                //이번주
                else
                {
                    //오늘이면
                    if (startDay == endDay)
                    {
                        today += diff;
                    }
                    curWeek += diff;
                }
            }
        }
        //평균값 구하기
        curWeek /= (endDay + 1);
        preWeek /= 7;
        m_thisWeek_Aver.text = curWeek.Hours.ToString() + "시간 " + curWeek.Minutes.ToString() + "분 /";
        m_lastWeek_Aver.text = preWeek.Hours.ToString() + "시간 " + preWeek.Minutes.ToString() + "분 /";

        m_thisWeek_Scroll.size = (float)(curWeek.TotalSeconds / 86400);
        m_lastWeek_Scroll.size = (float)(preWeek.TotalSeconds / 86400);

        //오늘 몇시간 잤는가?
        m_todaySleep.text = today.Hours.ToString() + "시간 " + today.Minutes.ToString() + "분 /";
    }

    /// <summary>
    /// Activity 관련 계산 - 운동하기, 일어서기
    /// </summary>
    void Calc_Activity(ActivitySample activitySample)
    {
        Debug.Log(activitySample.ExerciseTimeInMinutes);
        m_Exercise_Time.text = activitySample.ExerciseTimeInMinutes / 60 + "시간 " + activitySample.ExerciseTimeInMinutes % 60 + "분";

        Debug.Log(activitySample.StandHours);
        m_Stand_Count.text = activitySample.StandHours.ToString() + "회 / 시간";
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
}
