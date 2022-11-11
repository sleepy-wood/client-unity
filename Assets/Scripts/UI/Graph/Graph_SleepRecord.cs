using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph_SleepRecord : MonoBehaviour
{
    [Header("Priod")]
    [SerializeField] private RectTransform selectButton;
    [SerializeField] private Transform periodParent;
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

        ////그래프 그리기
        //switch ((int)record)
        //{
        //    case 0:
        //        Draw_Graph();
        //        break;
        //}
    }

    void Draw_Graph(DateTime start, DateTime end, int idx)
    {

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
}
