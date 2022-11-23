using Cinemachine;
using NativePlugin.HealthData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TreeGraph_UI : MonoBehaviour
{
    [SerializeField] private Transform m_scrollBarParent;
    Text m_text;
    Text m_Createtext;
    CollectionData treeData;
    SleepSample[] samples;
    bool isOnce = false;
    private void Awake()
    {
        m_text = transform.GetChild(0).GetComponent<Text>();
        m_Createtext = transform.GetChild(1).GetComponent<Text>();
        treeData = DataTemporary.arrayCollectionDatas.collectionLists[int.Parse(transform.parent.gameObject.name.Split("_")[1])];

        DateTime dateTime = DateTime.Parse(treeData.createdAt);

        //m_Text에 정보 기입
        string t = "";
        t += "나무 이름: " + treeData.treeName;
        t += "\n생성 날짜: " + dateTime.Year + "." + dateTime.Month + "." + dateTime.Day + " / " + dateTime.Hour + ":" + dateTime.Minute;
        t += "\n만든이: " + DataTemporary.MyUserData.nickname;
        if (treeData.product == null)
            t += "";
        else
            t += "\n토큰: " + treeData.product.tokenId;
        t += "\n희귀성: " + treeData.rarity + "/100";
        t += "\n생명력: " + treeData.vitality + "/100";
        t += " \n일별 수면량";

        m_text.text = t;
        m_Createtext.text = "created by\n" + DataTemporary.MyUserData.nickname;
        samples = DataTemporary.samples;
    }
    private void OnEnable()
    {
        for (int j = 0; j < m_scrollBarParent.childCount; j++)
        {
            m_scrollBarParent.GetChild(j).GetChild(1).GetComponent<Image>().fillAmount = 0;
        }
        StartCoroutine(DrawGraph());
    }
    private IEnumerator DrawGraph()
    {
        yield return new WaitForSeconds(0.3f);
        DrawTreeGraph();
    }    
    private void DrawTreeGraph()
    {
        List<float> treeSleepTimes = new List<float>();
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
            StartCoroutine(GraphMove(j, treeSleepTimes[j] / 28800));
        }
    }

    #region Coroutine
    private IEnumerator GraphMove(int idx, float size)
    {
        m_scrollBarParent.GetChild(idx).GetChild(2).GetComponent<Text>().text = (size * 100).ToString();

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
