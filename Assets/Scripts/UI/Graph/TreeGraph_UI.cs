using Cinemachine;
using NativePlugin.HealthData;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TreeGraph_UI : MonoBehaviour
{
    [SerializeField] private Transform m_scrollBarParent;
    GetTreeData treeData;
    SleepSample[] samples;
    List<float> treeSleepTimes = new List<float>();
    bool isOnce = false;
    private void Awake()
    {
        //for(int i = 0; i <  DataTemporary.GetTreeData.getTreeDataList.Count; i++)
        //{
        //     if(DataTemporary.MyUserData.currentLandId == DataTemporary.GetTreeData.getTreeDataList[i].landId)
        //     {
        //         treeData = DataTemporary.GetTreeData.getTreeDataList[i];
        //         break;
        //     }
        //}
        treeData = DataTemporary.GetTreeData.getTreeDataList[0];
        samples = DataTemporary.samples;
    }
    private void OnEnable()
    {
        DrawTreeGraph();
    }
    private void DrawTreeGraph()
    {
        DateTime treeDateTime = DateTime.Parse(treeData.createdAt);
        //시작점을 구하자
        int temp = 0;
        bool isStartDay = false;
        for (int i = samples.Length - 1; i >=0; i--)
        {
            if (samples[i].Type.ToString().Contains("Asleep"))
            {
                //두 시간의 중앙값을 알아내어 어느 날에 속하게 할 것인지 정하기
                TimeSpan diff = samples[i].EndDate - samples[i].StartDate;
                diff /= 2;
                var NewDate = new DateTime(
                    samples[i].StartDate.Year,
                    samples[i].StartDate.Month,
                    samples[i].StartDate.Day,
                    samples[i].StartDate.Hour,
                    samples[i].StartDate.Minute,
                    samples[i].StartDate.Second
                    );
                NewDate.AddDays(diff.Days);
                NewDate.AddHours(diff.Hours);
                NewDate.AddMinutes(diff.Minutes);
                NewDate.AddSeconds(diff.Seconds);
                if(NewDate == treeDateTime && !isStartDay)
                {
                    isStartDay = true;
                }
                else
                {
                    if (isStartDay)
                    {
                        temp = i + 1;
                        break;
                    }
                }
            }
        }
        //시작으로부터 5일까지 데이터 가져오기
        int startDay = 0;
        int preDay = 0;
        int cnt = 0;
        float times = 0;
        for (int j = temp; j < samples.Length; j++)
        {
            if (samples[j].Type.ToString().Contains("Asleep"))
            {
                //두 시간의 중앙값을 알아내어 어느 날에 속하게 할 것인지 정하기
                TimeSpan diff = samples[j].EndDate - samples[j].StartDate;
                diff /= 2;
                var NewDate = new DateTime(
                    samples[j].StartDate.Year,
                    samples[j].StartDate.Month,
                    samples[j].StartDate.Day,
                    samples[j].StartDate.Hour,
                    samples[j].StartDate.Minute,
                    samples[j].StartDate.Second
                    );
                NewDate.AddDays(diff.Days);
                NewDate.AddHours(diff.Hours);
                NewDate.AddMinutes(diff.Minutes);
                NewDate.AddSeconds(diff.Seconds);
                //중앙값의 날의 요일
                startDay = (int)NewDate.DayOfWeek;

                if (!isOnce)
                {
                    isOnce = true;
                    preDay = startDay;
                }
                if (cnt >= 5)
                    break;
                if (preDay != startDay)
                {
                    treeSleepTimes.Add(times);
                    times = 0;
                    cnt++;
                }
                else
                {
                    times += (float)diff.TotalSeconds;
                }
                preDay = startDay;
            }
        }
        for(int j = 0;j < treeSleepTimes.Count; j++)
        {
            StartCoroutine(GraphMove(j, treeSleepTimes[j] / 86400));
        }
    }

    #region Coroutine
    private IEnumerator GraphMove(int idx, float size)
    {
        Debug.Log(size);
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.05f;
            m_scrollBarParent.GetChild(idx).GetChild(1).GetComponent<Image>().fillAmount = Mathf.Lerp(m_scrollBarParent.GetChild(idx).GetChild(1).GetComponent<Image>().fillAmount, size, t);
            yield return null;
        }
        m_scrollBarParent.GetChild(idx).GetChild(1).GetComponent<Image>().fillAmount = size;
    }
    #endregion
}
