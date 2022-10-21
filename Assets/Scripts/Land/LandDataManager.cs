using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LandDataManager : MonoBehaviour
{
    public static LandDataManager Instance;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    private string landDataFileName = "LandData";

    ////test
    //private void Start()
    //{
    //    SaveLandData();
    //}

    /// <summary>
    /// LandManager 하위에 있는 Land들의 정보를 저장하는 함수
    /// </summary>
    public void SaveLandData()
    {
        List<LandData> landDataList = new List<LandData>();
        for(int i = 0; i< transform.childCount; i++)
        {
            LandData landData = new LandData();
            List<ObjectsInfo> objectList = new List<ObjectsInfo>();
            for(int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                ObjectsInfo objectsInfo = new ObjectsInfo();
                string dataPath = "Object/" + transform.GetChild(i).GetChild(j).name;
                objectsInfo.path = dataPath;
                objectsInfo.localPosition = transform.GetChild(i).GetChild(j).localPosition;
                objectsInfo.localScale = transform.GetChild(i).GetChild(j).localScale;
                objectsInfo.localEulerAngle = transform.GetChild(i).GetChild(j).localEulerAngles;

                objectList.Add(objectsInfo);
            }
            ArrayObjectsOfLand arrayObjectsOfLand = new ArrayObjectsOfLand();
            arrayObjectsOfLand.Objects = objectList;

            landData.arrayObjectsOfLand = arrayObjectsOfLand;
            landData.landNum = i;
            landData.landPosition = transform.GetChild(i).localPosition;
            landData.landScale = transform.GetChild(i).localScale;
            landData.landEulerAngle = transform.GetChild(i).localEulerAngles;

            landDataList.Add(landData);
        }
        ArrayLandData arrayLandData = new ArrayLandData();
        arrayLandData.LandLists = landDataList;
        
        string jsonData = JsonUtility.ToJson(arrayLandData, true);

        //Json을 txt 파일로 레지스트리에 저장
        string filePath = Application.dataPath + "/Data";

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        File.WriteAllText(filePath + "/" + landDataFileName + ".txt", jsonData);
    }


}
