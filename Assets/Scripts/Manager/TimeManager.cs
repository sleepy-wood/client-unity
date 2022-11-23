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
    public TreeController treeControll;
    public GameObject landCanvas;

    private void Awake()
    {
        if (treeControll.playMode)
        {
            firstPlantDate.dateTime = DateTime.MinValue;
        }
        else
        {
            print("Land Id : " + DataTemporary.MyUserData.currentLandId);
            int treeDataCount = DataTemporary.GetTreeData.getTreeDataList.Count;
            print($"Tree Data Count : {treeDataCount}개");
            // 해당 랜드의 firstPlantDate 알아내기
            if (treeDataCount > 0)
            {
                for (int i = 0; i < treeDataCount; i++)
                {
                    // User의 CurrentLandId와 같은 LandId인 treeData 가져오기
                    if (DataTemporary.GetTreeData.getTreeDataList[i].landId == DataTemporary.MyUserData.currentLandId)
                    {
                        // 현재 랜드의 트리 데이터 인덱스 저장
                        treeControll.treeDataIdx = i;

                        // 처음 심은 날 저장
                        firstPlantDate = DateTime.Parse(DataTemporary.GetTreeData.getTreeDataList[i].createdAt);
                        print("firstPlantDate : " + firstPlantDate);

                        // 현재 랜드의 나무 데이터
                        treeControll.currentTreeData = DataTemporary.GetTreeData.getTreeDataList[i];
                        print($"{treeDataCount}개의 트리 데이터 중 {i}번째 트리 데이터");  // 심은 순서대로 저장 
                        print("나무 처음 심은 시간 : " + firstPlantDate.dateTime);
                    }
                }
            }
        }
        

        // firstPlantDate로 방문타입 결정 => 5일차 후 새로운 Seed 심기 전 null값 처리필요
        if (firstPlantDate.dateTime == DateTime.MinValue) 
        {
            print("First Visit");
            treeControll.visitType = TreeController.VisitType.First;
            firstPlantDate = DateTime.Now;
            CalculatePlantDays(firstPlantDate, DateTime.Now);
        }
        else
        {
            print("Revisit");
            treeControll.visitType = TreeController.VisitType.ReVisit;
            // totalPlantDay, dayCount 계산
            CalculatePlantDays(firstPlantDate, DateTime.Now);
            landCanvas.SetActive(true);
        }
    }

    private void Start()
    {
        // 현재 날짜
        now = DateTime.Now;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            GetTreeData();
        }
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
        //if (treeControll.playMode)
        //{
        //    if (totalPlantDay == 0 && Input.GetKeyDown(KeyCode.Alpha1))
        //    {
        //        CalculatePlantDays(firstPlantDate, now);
        //        GameManager.Instance.treeController.SetTree(1, true);
        //    }
        //    else if (Input.GetKeyDown(KeyCode.Alpha2) && totalPlantDay < 5)
        //    {
        //        now = now.dateTime.AddDays(1);
        //        CalculatePlantDays(firstPlantDate, now);
        //        treeControll.PipelineSetting(totalPlantDay - 1);
        //        GameManager.Instance.treeController.SetTree(totalPlantDay, true);
        //    }
        //}
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
        treeControll.dayCount = totalPlantDay;
        print("dayCount = " + totalPlantDay);
        return totalPlantDay;
    }

    /// <summary>
    /// 일 수 하루 더한 뒤 트리 헬스 데이터로 세팅
    /// </summary>
    int day = 0;
    public void OnPlusDay(DateTime firstDate)
    {
        day += 1;
        now = firstDate.AddDays(day);
        if (day == 5) day = 1;  
        CalculatePlantDays(firstDate, now);
        if (day > 0)
        {
            // Tree Data 받아와서 Tree Id 세팅해서 2~5일 데이터 저장
            GetTreeData();

            int treeDataCount = DataTemporary.GetTreeData.getTreeDataList.Count;
            for (int i = 0; i < treeDataCount; i++)
            {
                // User의 CurrentLandId와 같은 LandId인 treeData 가져오기
                if (DataTemporary.GetTreeData.getTreeDataList[i].treeName == treeControll.treeName)
                {
                    // 현재 랜드의 트리 데이터 인덱스 저장
                    //treeControll.treeDataIdx = i;

                    //// 현재 랜드의 나무 데이터
                    //treeControll.currentTreeData = DataTemporary.GetTreeData.getTreeDataList[i];
                    //print($"{treeDataCount}개의 트리 데이터 중 {i}번째 트리 데이터");  // 심은 순서대로 저장 
                    //print("나무 처음 심은 시간 : " + firstPlantDate.dateTime);

                    // Tree Id 저장
                    treeControll.treeId = DataTemporary.GetTreeData.getTreeDataList[i].id;
                }
            }
        }
        if (treeControll.playMode) treeControll.SetTree(totalPlantDay, 0);
        else treeControll.SetTree(totalPlantDay, 1);
    }

    public async void GetTreeData()
    {
        ResultGet<GetTreeData> treeData = await DataModule.WebRequestBuffer<ResultGet<GetTreeData>>("/api/v1/trees", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);

        if (treeData.result)
        {
            Debug.Log(treeData.data);
            ArrayGetTreeData arrayTreeData = new ArrayGetTreeData();
            arrayTreeData.getTreeDataList = treeData.data;
            DataTemporary.GetTreeData = arrayTreeData;
        }


    }
}
