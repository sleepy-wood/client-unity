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
    // SkyController Script
    public SkyController sky;
    // 나무를 처음 심은 날
    public UDateTime firstPlantDate;
    public Text txtAge;

    private async void Awake()
    {
        //AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundles/newtreebundle");
        //ResultGet<GetTreeData> treeData = await DataModule.WebRequestBuffer<ResultGet<GetTreeData>>("/api/v1/trees", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        //ArrayGetTreeData arrayTreeData = new ArrayGetTreeData();
        //arrayTreeData.getTreeDataList = treeData.data;
        //DataTemporary.GetTreeData = arrayTreeData;

        // 해당 Land의 firstPlantDate 알아내기
        int idx = DataTemporary.GetTreeData.getTreeDataList.Count;
        for (int i = 0; i < idx; i++)
        {
            // User의 CurrentLandId와 같은 LandId인 treeData 가져오기
            if (DataTemporary.GetTreeData.getTreeDataList[i].landId == 3)//DataTemporary.MyUserData.currentLandId)
            {
                firstPlantDate = DateTime.Parse(DataTemporary.GetTreeData.getTreeDataList[i].createdAt);
                GameManager.Instance.treeController.dataIdx = i;
                print($"{i}번째 트리 데이터");
                print("나무 처음 심은 시간 : " + firstPlantDate.dateTime);
            }
        }
        // firstPlantDate로 방문타입 결정  => 5일차 후 새로운 Seed 심기 전 null값 처리필요
        if (firstPlantDate.dateTime == DateTime.MinValue)
        {
            print("First Visit");
            GameManager.Instance.treeController.visitType = TreeController.VisitType.First;
            firstPlantDate = DateTime.Now;
            CalculatePlantDays(firstPlantDate, DateTime.Now);
        }
        else
        {
            print("Revisit");
            GameManager.Instance.treeController.visitType = TreeController.VisitType.ReVisit;
            // totalPlantDay, dayCount 계산
            CalculatePlantDays(firstPlantDate, DateTime.Now);
        }
    }

    private void Start()
    {
        // 현재 날짜
        now = DateTime.Now;
        //txtCurrentDate.text = now.dateTime.ToString("yyyy-MM-dd") + " (" + totalPlantDay + "일차)";

        // 현재시간
        // txtCurrentTime.text = now.dateTime.ToString("tt h : mm");

        
    }

    bool isOnce;
    bool skyChangeDone;
    private void Update()
    {
        //now = DateTime.Now;
        //txtCurrentTime.text = now.dateTime.ToString("tt h : mm : ss");



        #region skyBox 변화 시연
        // 특정 시간 지정
        //if (i == 0 && Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    //now = now.dateTime.AddHours(6);
        //    int month = int.Parse(DateTime.Now.ToString("MM"));
        //    int day = int.Parse(DateTime.Now.ToString("dd"));
        //    // 일몰 시간
        //    now = new DateTime(2022, month, day, 17, 00, 0);
        //    txtCurrentTime.text = now.dateTime.ToString("tt h : mm");
        //    i++;
        //}
        //else if (i == 1 && Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    int month = int.Parse(DateTime.Now.ToString("MM"));
        //    int day = int.Parse(DateTime.Now.ToString("dd"));
        //    // 저녁 시간
        //    now = new DateTime(2022, month, day, 18, 00, 0);
        //    txtCurrentTime.text = now.dateTime.ToString("tt h : mm");
        //    i++;
        //}
        //else if (i == 2 && Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    int month = int.Parse(DateTime.Now.ToString("MM"));
        //    int day = int.Parse(DateTime.Now.ToString("dd"));
        //    // 아침
        //    now = new DateTime(2022, month, day + 1, 07, 00, 0);
        //    CalculatePlantDays(firstPlantDate, now);
        //    txtCurrentDate.text = now.dateTime.ToString("yyyy-MM-dd") + " (" + totalPlantDay + "일차)";
        //    txtCurrentTime.text = now.dateTime.ToString("tt h : mm");
        //    i++;
        //    skyChaneDone = true;
        //}
        #endregion

        #region Day ++
        if (GameManager.Instance.treeController.playMode)
        {
            if (totalPlantDay == 0 && Input.GetKeyDown(KeyCode.Alpha1))
            {
                CalculatePlantDays(firstPlantDate, now);
                GameManager.Instance.treeController.SetTree(1, true);
                //txtCurrentDate.text = now.dateTime.ToString("yyyy-MM-dd") + " (" + totalPlantDay + "일차)";
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && totalPlantDay < 5)
            {
                now = now.dateTime.AddHours(25);//AddDays(1);
                CalculatePlantDays(firstPlantDate, now);
                GameManager.Instance.treeController.SetTree(totalPlantDay, true);
                //txtCurrentDate.text = now.dateTime.ToString("yyyy-MM-dd") + " (" + totalPlantDay + "일차)";
            }
        }
        
        #endregion

        #region SkyBox 변경
        //if (txtCurrentTime.text == "오후 5 : 00" && !isOnce)
        //{
        //    sky.Sunset();
        //    txtCurrentDate.color = Color.black;
        //    txtCurrentTime.color = Color.black;
        //    txtUserData.color = Color.black;
        //    isOnce = true;
        //}
        //else if (txtCurrentTime.text == "오후 6 : 00" && isOnce)
        //{
        //    sky.Night();
        //    txtCurrentDate.color = Color.white;
        //    txtCurrentTime.color = Color.white;
        //    txtUserData.color = Color.white;
        //    isOnce = false;
        //}
        //else if (txtCurrentTime.text == "오전 7 : 00" && !isOnce)
        //{
        //    sky.Day();
        //    txtCurrentDate.color = Color.black;
        //    txtCurrentTime.color = Color.black;
        //    txtUserData.color = Color.black;
        //    isOnce = true;
        //}
        #endregion
    }

    /// <summary>
    /// 내가 계산하고자 하는 Date (to) - 나무 처음 심은 Date (from)
    /// </summary>
    /// <returns>나무 심은지 몇일이 지났는지 int로 반환</returns>
    public int CalculatePlantDays(DateTime from, DateTime to)
    {
        TimeSpan timeDif = to - from;
        totalPlantDay = (int)timeDif.Days + 1;
        GameManager.Instance.treeController.dayCount = totalPlantDay;
        return totalPlantDay;
    }

    public void OnPlusDay()
    {
        now = now.dateTime.AddDays(1);
        CalculatePlantDays(firstPlantDate, now);
        GameManager.Instance.treeController.SetTree(totalPlantDay);
    }
}
