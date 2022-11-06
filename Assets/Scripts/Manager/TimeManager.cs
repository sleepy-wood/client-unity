using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Globalization;

public class TimeManager : MonoBehaviour
{
    // 현재 시간
    public UDateTime now;
    // 나무 심은 누적 일
    public int totalPlantDay;
    // 임시 현재 날짜 저장 변수
    public UDateTime testFirstDay;
    // ChangeSky Script
    public ChangeSky sky;
    // test 현재 날짜
    public UDateTime testDate;
    // 나무를 처음 심은 날
    public UDateTime firstPlantDate;

    private void Start()
    {
        // Test : 현재 시간 저장
        testFirstDay = DateTime.Now;
        testDate = DateTime.Now;
    }

    /// <summary>
    /// 사용자가 MyRoom에 들어왔을 때 나무 심은 누적 Day Count해주는 함수
    /// </summary>
    /// <returns>나무 심은지 몇일이 지났는지 int로 반환</returns>
    public void CalculatePlantDays(DateTime date)
    {
        // Test 용
        TimeSpan timeDif = date - testFirstDay;
        // 실제 구현용
        //TimeSpan timeDif = DateTime.Now - start;
        totalPlantDay = (int)timeDif.Days;
        //return totalPlantDay;
    }

    public Text txtDayCount;
    public Text txtAge;
    /// <summary>
    /// Test : 버튼 누르면 Plus Day
    /// </summary>
    public void onPlusDay()
    {
        testDate = testDate.dateTime.AddHours(25);
        CalculatePlantDays(testDate);
        // Tree Update
        GameManager.Instance.treeController.TreeUpdate();
        // DayCount Text 변경
        //txtDayCount.text = $"Day{totalPlantDay}";
    }


    bool isDay = true;
    /// <summary>
    /// 6시 기준으로 오전 오후 Skybox 변경
    /// </summary>
    public void onChangeSky()
    {
        // 실제 구현용
        //if (now.dateTime.ToString("tt") == "오전")
        // Test
        if (!isDay)
        {
            sky.Day();
            isDay = true;
        }
        else
        {
            sky.Night();
            isDay = false;
        }

    }

}
