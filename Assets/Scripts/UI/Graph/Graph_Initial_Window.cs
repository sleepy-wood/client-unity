using NativePlugin.HealthData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Graph_Initial_Window : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private Text m_sleepResultText;
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
    bool once = false;
    private void Awake()
    {
        sleepsData = DataTemporary.samples;
    }
    private bool isOnce = false;
    private bool startPreWeek = false;
    private bool endPreWeek = false;

    private void OnEnable()
    {
        m_thisWeek_Scroll.size = 0;
        m_lastWeek_Scroll.size = 0;
        once = false;
    }
    void Update()
    {
        if (HealthDataStore.GetStatus() == HealthDataStoreStatus.Loaded && !once)
        {
            once = true;

            //DateTime now = DateTime.Now;

            //DateTime nowStart = now.AddYears(-1);
            //ActivitySample[] activitySamples = HealthDataStore.GetActivitySamples(
            //    nowStart, now);

            //if (activitySamples != null || activitySamples.Length == 0)
            //{
            //    Debug.LogError("ActivitySamples is null or empty");
            //    return;
            //}

            ////달성률 수치로 알려줌
            ////HealthReport report = HealthDataAnalyzer.GetDailyReport(
            ////    DateTime.Now,
            ////    6
            ////);
            //int lastIdx = 0;

            //for (int i = 0; i < activitySamples.Length; i++)
            //{
            //    lastIdx = activitySamples[i].Date > activitySamples[lastIdx].Date ? i : lastIdx;
            //}
            //수면 관련 계산
            Calc_Sleep();
            //활동 관련 계산
            //Calc_Activity(activitySamples[lastIdx]);
        }
    }

    //한주동안 데이터가 있는 날의 일수
    //이번주는 맨 끝값을 갖고 preDay랑 같은값으로 시작하므로 하나 갖고 시작
    private int curWeekCnt = 1;
    private int preWeekCnt = 0;
    /// <summary>
    /// Sleep 관련 계산 - 저번주 평균 수면 시간, 이번주 평균 수면시간, 오늘 수면 시간
    /// </summary>
    void Calc_Sleep()
    {
        startPreWeek = false;
        endPreWeek = false;
        //초기화
        curWeekCnt = 1;
        preWeekCnt = 0;
        isOnce = false;
        preWeek = new TimeSpan();
        curWeek = new TimeSpan();
        today = new TimeSpan();
        for (int i = sleepsData.Length - 1; i >= 0; i--)
        {
            Debug.Log(sleepsData[i].Type);
            if (sleepsData[i].Type.ToString().Contains("Asleep"))
            {
                //Debug.Log("endDay = " + endDay);
                //두 시간의 중앙값을 알아내어 어느 날에 속하게 할 것인지 정하기
                TimeSpan diff = sleepsData[i].EndDate - sleepsData[i].StartDate;
                diff /= 2;
                var NewDate = new DateTime(
                    sleepsData[i].StartDate.Year,
                    sleepsData[i].StartDate.Month,
                    sleepsData[i].StartDate.Day,
                    sleepsData[i].StartDate.Hour,
                    sleepsData[i].StartDate.Minute,
                    sleepsData[i].StartDate.Second
                    );
                NewDate.AddDays(diff.Days);
                NewDate.AddHours(diff.Hours);
                NewDate.AddMinutes(diff.Minutes);
                NewDate.AddSeconds(diff.Seconds);
                //Debug.Log(NewDate);
                //Debug.Log(NewDate.DayOfWeek);
                //중앙값의 날의 요일
                startDay = (int)NewDate.DayOfWeek;
                if (!isOnce)
                {
                    endDay = (int)sleepsData[i].EndDate.DayOfWeek;
                    isOnce = true;
                    preDay = startDay;
                }
                //Debug.Log("startDay = " + startDay);
                //갑자기 이전 계산한 preDay보다 startDay가 커진다면 - 저번주로 넘어감
                if (startDay > preDay)
                {
                    //이미 저번주로 넘어간상태 였다면 저저번주로 넘어갔음
                    endPreWeek = startPreWeek == true ? true : false;
                    startPreWeek = true;
                    //Debug.Log("전주");
                }
                //저저번주
                if (endPreWeek)
                {
                    //나가기
                    //Debug.Log("끝");
                    break;
                }
                //저번주
                else if (startPreWeek)
                {
                    if (preDay != startDay)
                        preWeekCnt++;
                    preWeek += diff;
                    //Debug.Log("preWeek = " + preWeek);
                }
                //이번주
                else
                {
                    if (preDay != startDay)
                        curWeekCnt++;
                    //오늘이면
                    if (startDay == endDay)
                    {
                        today += diff;
                    }
                    //Debug.Log("curWeek = " + curWeek);
                    curWeek += diff;
                }
                preDay = startDay;
            }
        }
        //평균값 구하기
        curWeek /= curWeekCnt;
        preWeek /= preWeekCnt;

        m_thisWeek_Aver.text = curWeek.Hours.ToString() + "시간 " + curWeek.Minutes.ToString() + "분";
        m_lastWeek_Aver.text = preWeek.Hours.ToString() + "시간 " + preWeek.Minutes.ToString() + "분";

        StartCoroutine(GraphMove((float)(curWeek.TotalSeconds / 86400), (float)(preWeek.TotalSeconds / 86400)));

        if (m_thisWeek_Scroll.size < m_lastWeek_Scroll.size)
        {
            m_sleepResultText.text = "이번주 하루 평균 수면시간이 지난주보다 줄었습니다.";
        }
        else if (m_thisWeek_Scroll.size > m_lastWeek_Scroll.size)
        {
            m_sleepResultText.text = "이번주 하루 평균 수면시간이 지난주보다 늘었습니다.";
        }
        else
        {
            m_sleepResultText.text = "이번주 하루 평균 수면시간이 지난주와 같습니다.";
        }
        //오늘 몇시간 잤는가?
        m_todaySleep.text = today.Hours.ToString() + "시간 " + today.Minutes.ToString() + "분 ";
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
    #region Coroutine
    private IEnumerator GraphMove(float endSize_this, float endSize_last)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.1f;
            m_thisWeek_Scroll.size = Mathf.Lerp(m_thisWeek_Scroll.size, endSize_this, t);
            m_lastWeek_Scroll.size = Mathf.Lerp(m_lastWeek_Scroll.size, endSize_last, t);
            yield return null;
        }
    }
    #endregion

}
