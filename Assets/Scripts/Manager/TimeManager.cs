using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Globalization;

public class TimeManager : MonoBehaviour
{
    // 현재 시간, 날짜, 유저 데이터 
    public Text txtCurrentTime, txtCurrentDate, txtUserData;
    public UDateTime now;
    // 나무 심은 누적 일
    public int totalPlantDay;
    // ChangeSky Script
    public ChangeSky sky;
    // 나무를 처음 심은 날
    public UDateTime firstPlantDate;
    public Text txtAge;

    private void Start()
    {
        firstPlantDate = DateTime.Now;
        // 현재 날짜
        now = DateTime.Now;
        txtCurrentDate.text = now.dateTime.ToString("yyyy-MM-dd") + " (" + totalPlantDay + "일차)";

        // 현재시간
        txtCurrentTime.text = now.dateTime.ToString("tt h : mm");

        // 현재 시간 고려해 SkyBox 세팅
        // 낮 : 오전 7시~ / 일몰 : 오후 5시~ / 저녁 : 오후 6시~
        if (DateTime.Now.Hour > 7 && DateTime.Now.Hour < 17) sky.Day();
        else if (DateTime.Now.Hour > 17 && DateTime.Now.Hour < 18) sky.Sunset();
        else
        {
            txtCurrentDate.color = Color.white;
            txtCurrentTime.color = Color.white;
            txtUserData.color = Color.white;
            sky.Night();
        }
    }

    bool isOnce;
    bool skyChaneDone;
    int i=0;
    private void Update()
    {
        //now = DateTime.Now;
        //txtCurrentTime.text = now.dateTime.ToString("tt h : mm : ss");

        
        # region skyBox 변화 시연
        // 특정 시간 지정
        if (i == 0 && Input.GetKeyDown(KeyCode.Alpha2))
        {
            //now = now.dateTime.AddHours(6);
            int month = int.Parse(DateTime.Now.ToString("MM"));
            int day = int.Parse(DateTime.Now.ToString("dd"));
            // 일몰 시간
            now = new DateTime(2022, month, day, 17, 00, 0);
            txtCurrentTime.text = now.dateTime.ToString("tt h : mm");
            i++;
        }
        else if (i == 1 && Input.GetKeyDown(KeyCode.Alpha2))
        {
            int month = int.Parse(DateTime.Now.ToString("MM"));
            int day = int.Parse(DateTime.Now.ToString("dd"));
            // 저녁 시간
            now = new DateTime(2022, month, day, 18, 00, 0);
            txtCurrentTime.text = now.dateTime.ToString("tt h : mm");
            i++;
        }
        else if (i == 2 && Input.GetKeyDown(KeyCode.Alpha2))
        {
            int month = int.Parse(DateTime.Now.ToString("MM"));
            int day = int.Parse(DateTime.Now.ToString("dd"));
            // 아침
            now = new DateTime(2022, month, day + 1, 07, 00, 0);
            CalculatePlantDays(now);
            txtCurrentDate.text = now.dateTime.ToString("yyyy-MM-dd") + " (" + totalPlantDay + "일차)";
            txtCurrentTime.text = now.dateTime.ToString("tt h : mm");
            i++;
            skyChaneDone = true;
        }
        #endregion

        #region Day ++
        if (totalPlantDay==0 && Input.GetKeyDown(KeyCode.Alpha0))
        {
            CalculatePlantDays(now);
            GameManager.Instance.treeController.LoadTree();
            txtCurrentDate.text = now.dateTime.ToString("yyyy-MM-dd") + " (" + totalPlantDay + "일차)";
        }
        else if(skyChaneDone && Input.GetKeyDown(KeyCode.Alpha1) && totalPlantDay < 5)
        {
            now = now.dateTime.AddDays(1);
            CalculatePlantDays(now);
            GameManager.Instance.treeController.LoadTree();
            txtCurrentDate.text = now.dateTime.ToString("yyyy-MM-dd") + " (" + totalPlantDay + "일차)";
        }
        #endregion


        // SkyBox 변경
        if (txtCurrentTime.text == "오후 5 : 00" && !isOnce)
        {
            sky.Sunset();
            txtCurrentDate.color = Color.black;
            txtCurrentTime.color = Color.black;
            txtUserData.color = Color.black;
            isOnce = true;
        }
        else if (txtCurrentTime.text == "오후 6 : 00" && isOnce)
        {
            sky.Night();
            txtCurrentDate.color = Color.white;
            txtCurrentTime.color = Color.white;
            txtUserData.color = Color.white;
            isOnce = false;
        }
        else if (txtCurrentTime.text == "오전 7 : 00" && !isOnce)
        {
            sky.Day();
            txtCurrentDate.color = Color.black;
            txtCurrentTime.color = Color.black;
            txtUserData.color = Color.black;
            isOnce = true;
        }
    }

    /// <summary>
    /// 내가 입력한 Date - 나무 처음 심은 Date
    /// </summary>
    /// <returns>나무 심은지 몇일이 지났는지 int로 반환</returns>
    public void CalculatePlantDays(DateTime date)
    {
        TimeSpan timeDif = date - firstPlantDate;
        totalPlantDay = (int)timeDif.Days + 1;
        GameManager.Instance.treeController.dayCount = totalPlantDay;
    }
}
