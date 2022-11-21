using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Playables;

using Broccoli.Factory;
using Broccoli.Pipe;
using Broccoli.Generator;
using System.IO;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Networking;

/// <summary>
/// 나무 첫 Date를 입력 -> 버튼을 통해 해당 Date+5일의 데이터로 나무 변화 -> My Collection 업로드
/// </summary>
public class DeveloperMode : MonoBehaviour
{
    // 나무 첫 Date 입력 
    [Header("Tree First Date")]
    public int year;
    public int month;
    public int day;
    public int hour;
    public int minute;
    DateTime firstDate;

    // 헬스 데이터 5일치 담을 리스트
    List<HealthReport> healthReports = new List<HealthReport>();

    //[Space]

    TreeController treeController;
    TimeManager timeManager;

    void Start()
    {
        treeController = GetComponent<TreeController>();
        timeManager = GetComponent<TimeManager>();

        // 입력된 Tree First Date로 5일치 헬스 데이터 불러오기
        if (HealthDataStore.GetStatus() == HealthDataStoreStatus.Loaded)
        {
            firstDate = new DateTime(year, month, day, hour, minute, 0, 0);
            for (int i = 1; i < 6; i++) 
            {
                HealthReport report = HealthDataAnalyzer.GetDailyReport(firstDate, i);
                healthReports.Add(report);
            }
            print($"{firstDate}의 HealthData Count = {healthReports.Count}");
        }
    }

    // +Day 버튼 누를 때마다 헬스 데이터로 나무 업데이트 (2일차 ~ 5일차)
    public void OnPlusDay()
    {
        // 1일차 -> 2일차
        if (treeController.dayCount == 1)
        {
            //timeManager.now = timeManager.now.dateTime.AddDays(1);
            //CalculatePlantDays(firstPlantDate, now);
            //treeControll.SetTree(totalPlantDay);

            treeController.SetTree(2);
        }
        // 2일차 이후 : 헬스데이터 적용
        if (treeController.dayCount > 1)
        {
            treeController.SetTree(treeController.dayCount);
        }

    }
}
