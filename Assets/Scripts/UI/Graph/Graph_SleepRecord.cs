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
    public enum RecordDate
    {
        Day = 0,
        Week = 1,
        Month = 2,
        SixMonth = 3,
        Year = 4
    }

    public enum GraphSleepType
    {
        AsleepREM = 0,
        AsleepCore = 1,
        AsleepDeep = 2
    }

    [Header("Priod")]
    [SerializeField] private RectTransform selectButton;
    [SerializeField] private Transform periodParent;
    [SerializeField] private Text sleep_averTime;
    [SerializeField] private Transform period_sleepGraph;
    [SerializeField] private float select_imageSpeed = 3;

    [Space]
    [Header("SleepFlow")]
    [SerializeField] private Transform graphParent;
    [SerializeField] private Text graphResult;

    private SleepSample[] sleepsData;
    bool isOnce = false;
    private int startDay = 0;
    private int endDay = 0;

    private RecordDate curRecordDate = RecordDate.Day;

    private void Awake()
    {
        sleepsData = DataTemporary.samples;
    }
    void OnEnable()
    {
        Init();
        OnClickChangeDate(0);
        Draw_SleepFlow();
    }
    void Init()
    {
        for(int i = 0; i < period_sleepGraph.childCount; i++)
        {
            period_sleepGraph.GetChild(i).GetComponent<Scrollbar>().size = 0;
        }
        for(int i = 0; i < graphParent.childCount; i++)
        {
            for(int j = graphParent.GetChild(i).childCount - 1; j>=0; j --)
            {
                Destroy(graphParent.GetChild(i).GetChild(j).gameObject);
            }
        }
    }
    #region Average_SleepTime
    private int preDay = 0;
    TimeSpan day_totalTime;
    void Graph_Day()
    {
        //초기화
        day_totalTime = new TimeSpan();
        isOnce = false;
        List<TimeSpan> timeSpans = new List<TimeSpan>();
        TimeSpan today = new TimeSpan();

        for (int i = sleepsData.Length - 1; i >= 0; i--)
        {
            if (sleepsData[i].Type.ToString().Contains("Asleep"))
            {
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
                //중앙값의 날의 요일
                startDay = (int)(NewDate.DayOfWeek);

                if (!isOnce)
                {
                    endDay = (int)(sleepsData[i].EndDate.DayOfWeek);
                    isOnce = true;
                    preDay = startDay;
                }
                //갑자기 이전 계산한 preDay보다 startDay가 커진다면 - 저번주로 넘어감
                if (startDay > preDay)
                {
                    timeSpans.Add(today);
                    break;
                }
                //이번주
                //오늘이면
                if (startDay == endDay)
                {
                    today += diff;
                }
                else
                {
                    endDay = startDay;
                    timeSpans.Add(today);
                    today = new TimeSpan();

                }
                preDay = startDay;
            }
        }
        for(int i = timeSpans.Count - 1; i >= 0; i--)
        {
            if(!period_sleepGraph.GetChild(i).gameObject.activeSelf)
                period_sleepGraph.GetChild(i).gameObject.SetActive(true);

            TimeSpan time = timeSpans[(timeSpans.Count - 1) - i];
            period_sleepGraph.GetChild(i).GetChild(0).GetComponent<Text>().text = ((DayOfWeek)i).ToString();
            day_totalTime += time;
            StartCoroutine(GraphMove(i, (float)(time.TotalSeconds / 86400)));
        }
        for (int i = timeSpans.Count; i < period_sleepGraph.childCount; i++)
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
        //초기화
        int cnt = 1;
        List<TimeSpan> timeSpans = new List<TimeSpan>();
        isOnce = false;
        week_totalTime = new TimeSpan();
        week = new TimeSpan();

        for (int i = sleepsData.Length - 1; i >= 0; i--)
        {
            if (sleepsData[i].Type.ToString().Contains("Asleep"))
            {
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
                //중앙값의 날의 요일
                startDay = (int)NewDate.DayOfWeek;

                if (!isOnce)
                {
                    endDay = (int)sleepsData[i].EndDate.DayOfWeek;
                    isOnce = true;
                    preDay = startDay;
                }
                //갑자기 이전 계산한 preDay보다 startDay가 커진다면 - 저번주로 넘어감
                if (startDay > preDay)
                {
                    timeSpans.Add(week / cnt);
                    if (timeSpans.Count < 7)
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

            TimeSpan time = timeSpans[i];
            week_totalTime += time;
            StartCoroutine(GraphMove(i, (float)(time.TotalSeconds / 86400)));
            if(i == 0)
                period_sleepGraph.GetChild(i).GetChild(0).GetComponent<Text>().text = "이번주";
            else
                period_sleepGraph.GetChild(i).GetChild(0).GetComponent<Text>().text = i.ToString() + "주 전";
        }
        week_totalTime /= timeSpans.Count;
        sleep_averTime.text = "평균: " + week_totalTime.Hours + "시간 " + week_totalTime.Minutes + "분";
    }
    /// <summary>
    /// 상단바 제어
    /// </summary>
    /// <param name="record"></param>
    void ChangeGraph(RecordDate record)
    {
        //스크롤바 초기화
        for (int i = 0; i < period_sleepGraph.childCount; i++)
        {
            period_sleepGraph.GetChild(i).GetComponent<Scrollbar>().size = 0;
        }
        //버튼 글자 색 변경
        for (int i = 0; i < 5; i++)
        {
            if ((int)record == i)
            {
                periodParent.GetChild(2 + i).GetChild(0).gameObject.SetActive(false);
                periodParent.GetChild(2 + i).GetChild(1).gameObject.SetActive(true);
                StartCoroutine(SelectButtonMove(selectButton.anchoredPosition, periodParent.GetChild(2 + i).GetComponent<RectTransform>().anchoredPosition));
                selectButton.anchoredPosition = periodParent.GetChild(2 + i).GetComponent<RectTransform>().anchoredPosition;
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
    #endregion

    #region Sleep_Flow
    private float posX = 83f;
    private int startDayGraph = 0;
    private int endDayGraph = 0;
    private bool isGraphOnce = false;
    private TimeSpan totalFlow = new TimeSpan();
    void Draw_SleepFlow()
    {
        posX = 83f;
        startDayGraph = 0;
        endDayGraph = 0;
        isGraphOnce = false;
        for (int i = sleepsData.Length - 1; i >= 0; i--)
        {
            if (!isGraphOnce)
            {
                endDayGraph = (int)(sleepsData[i].EndDate.DayOfWeek);
                isGraphOnce = true;
            }
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
            //중앙값의 날의 요일
            startDayGraph = (int)(NewDate.DayOfWeek);

            //오늘이면
            if (startDayGraph == endDayGraph)
            {
                float per = (float)(diff.TotalSeconds / 28800);
                if (sleepsData[i].Type.ToString().Contains("Asleep") && sleepsData[i].Type != SleepType.AsleepUnspecified)
                {
                    GraphSleepType type = (GraphSleepType)Enum.Parse(typeof(GraphSleepType), sleepsData[i].Type.ToString());
                    GameObject resource = Resources.Load<GameObject>("GraphUI/" + type.ToString());
                    GameObject graphGO = Instantiate(resource, graphParent.GetChild((int)type));
                    Vector2 rect = graphGO.GetComponent<RectTransform>().anchoredPosition;
                    rect.x = posX;
                    graphGO.GetComponent<RectTransform>().anchoredPosition = rect;
                    StartCoroutine(MoveSleepFlow(graphGO, per));
                    totalFlow += diff;
                }
                posX += 750 * per;
            }
            else
            {
                //Debug.Log(totalFlow.Hours + "시간 " + totalFlow.Minutes + "분 " + totalFlow.Seconds + "초");
                break;
            }
        }
        CalcSleep(SleepType.AsleepREM, graphResult);
    }
    #endregion


    /// <summary>
    /// 단위 Button 이벤트
    /// </summary>
    /// <param name="i"></param>
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

    private bool isStartYesterDay = false;
    private TimeSpan today;
    private TimeSpan yesterday;

    /// <summary>
    /// SleepType에 따라 어제와 오늘을 비교하는 결과 계산
    /// </summary>
    /// <param name="sleepType"></param>
    /// <param name="sleepDiffResult"></param>
    public void CalcSleep(SleepType sleepType, Text sleepDiffResult)
    {
        for (int i = sleepsData.Length - 1; i >= 0; i--)
        {
            if (sleepsData[i].Type == SleepType.AsleepREM)
            {
                if (!isOnce)
                {
                    endDay = (int)(sleepsData[i].EndDate.DayOfWeek);
                    isOnce = true;
                }
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
                //중앙값의 날의 요일
                startDay = (int)(NewDate.DayOfWeek);

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


    #region Coroutine
    private IEnumerator MoveSleepFlow(GameObject graphGO,  float endSize)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.05f;
            graphGO.GetComponent<Scrollbar>().size = Mathf.Lerp(graphGO.GetComponent<Scrollbar>().size, endSize, t);
            yield return null;
        }
    }
    /// <summary>
    /// Select Button 선택 이미지 이동
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private IEnumerator SelectButtonMove(Vector2 start, Vector2 end)
    {
        float t = 0;
        while (t < 1f)
        {
            t += select_imageSpeed * Time.deltaTime;
            selectButton.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
    }

    private IEnumerator GraphMove(int idx, float endSize)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.1f;
            period_sleepGraph.GetChild(idx).GetComponent<Scrollbar>().size = Mathf.Lerp(period_sleepGraph.GetChild(idx).GetComponent<Scrollbar>().size, endSize, t);
            yield return null;
        }
    }
    #endregion
}
