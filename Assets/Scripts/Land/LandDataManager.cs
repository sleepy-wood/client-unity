using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cysharp.Threading.Tasks.Triggers;

public class LandDataManager : MonoBehaviour
{
    public static LandDataManager Instance;
    /// <summary>
    /// Build하려고 했을때, 어떤 mode인가
    /// </summary>
    public enum BuildMode
    {
        None,
        Bridge,
        Object
    }

    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    private string landDataFileName = "LandData";
    private BuildMode buildMode = BuildMode.None;

    ////test
    //private void Start()
    //{
    //    SaveLandData();
    //}
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        Debug.Log("Load");
    //        LoadLandData();
    //    }
    //}

    private void Update()
    {
        if(buildMode == BuildMode.Bridge)
            BuildBridge();
    }

    public void BuildBridge()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Bridge");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (hit.transform.GetComponent<IClickedObject>() != null)
            {
                hit.transform.GetComponent<IClickedObject>().StairMe();
            }
        }
    }

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
            landData.landNum = i+1;
            landData.landPosition = transform.GetChild(i).localPosition;
            landData.landScale = transform.GetChild(i).localScale;
            landData.landEulerAngle = transform.GetChild(i).localEulerAngles;

            landDataList.Add(landData);
        }
        ArrayLandData arrayLandData = new ArrayLandData();
        arrayLandData.LandLists = landDataList;

        //DataTemporary에 Update or Save
        DataTemporary.MyLandData = arrayLandData;

        //File 형식으로 Update or Save
        FileManager.SaveDataFile(landDataFileName, arrayLandData);
    }

    /// <summary>
    /// Load Land Data and Create Object of Land
    /// </summary>
    public void LoadLandData()
    {

        ArrayLandData arrayLandData = FileManager.LoadDataFile<ArrayLandData>(landDataFileName);
        GameManager.Instance.User.GetComponent<Rigidbody>().useGravity = false;

        InitBridge(arrayLandData.BridgeInfo);

       for(int i = 0; i < arrayLandData.LandLists.Count; i++)
        {
            //Land 자체를 생성
            GameObject landResource = Resources.Load<GameObject>("SkyLand");
            GameObject land = Instantiate(landResource);
            land.name = land.name.Split('(')[0];
            land.name += (i + 1).ToString();
            land.transform.parent = transform;
            land.transform.localPosition = arrayLandData.LandLists[i].landPosition;
            land.transform.localScale = arrayLandData.LandLists[i].landScale;
            land.transform.localEulerAngles = arrayLandData.LandLists[i].landEulerAngle;
            ArrayObjectsOfLand arrayObjectsOf = arrayLandData.LandLists[i].arrayObjectsOfLand;
            
            //Land 위에 있는 것 Load 해서 발견하기
            for(int j = 0; j < arrayObjectsOf.Objects.Count; j++)
            {
                GameObject objResource = Resources.Load<GameObject>(arrayObjectsOf.Objects[j].path);
                GameObject obj = Instantiate(objResource);
                obj.name = obj.name.Split('(')[0];
                obj.transform.parent = land.transform;
                obj.transform.localPosition = arrayObjectsOf.Objects[j].localPosition;
                obj.transform.localScale = arrayObjectsOf.Objects[j].localScale;
                obj.transform.localEulerAngles = arrayObjectsOf.Objects[j].localEulerAngle;
            }
       }
        GameManager.Instance.User.GetComponent<Rigidbody>().useGravity = true;
    }

    public void InitBridge(List<List<int>> bridges)
    {

    }

    /// <summary>
    /// UI중에서 Bridge Build를 선택했을 경우
    /// </summary>
    public void SelectBuildMode()
    {
        buildMode = BuildMode.Bridge;
    }
    /// <summary>
    /// UI중에서 Cancel Button을 누른 경우 
    /// 초기화
    /// </summary>
    public void CancelButton()
    {
        buildMode = BuildMode.None;
    }
}
