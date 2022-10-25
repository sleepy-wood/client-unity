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
    public BuildMode buildMode = BuildMode.None;
    [SerializeField] private GameObject cancelButton;
    private GameObject user;

    //test
    private void Start()
    {
        user = GameManager.Instance.User;
        cancelButton.SetActive(false);
        LoadLandData();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Load");
            LoadLandData();
        }
        if (buildMode == BuildMode.Bridge)
            BuildBridge();
        else if(buildMode == BuildMode.None)
        {
            user.GetComponent<UserInput>().InputControl = false;
            for (int i = 0; i < transform.GetChild(transform.childCount - 1).childCount; i++)
            {
                Transform bridge = transform.GetChild(transform.childCount - 1).GetChild(i);
                if (bridge.GetComponent<Bridge>().currentBridgeType == Bridge.BridgeType.NotBuild)
                {
                    bridge.gameObject.SetActive(false);
                }
            }
        }
    }

    public void BuildBridge()
    {
        user.GetComponent<UserInput>().InputControl = true;
        for(int i =0; i < transform.GetChild(transform.childCount - 1).childCount; i++)
        {
            transform.GetChild(transform.childCount - 1).GetChild(i).gameObject.SetActive(true);
        }
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
        for(int i = 0; i< transform.childCount - 1; i++)
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
            arrayObjectsOfLand.objects = objectList;

            landData.arrayObjectsOfLand = arrayObjectsOfLand;
            landData.landNum = i+1;
            landData.landPosition = transform.GetChild(i).localPosition;
            landData.landScale = transform.GetChild(i).localScale;
            landData.landEulerAngle = transform.GetChild(i).localEulerAngles;

            landDataList.Add(landData);
        }
        List<BridgeData> bridgeDataList = new List<BridgeData>();
        List< BridgeFromTo> bridgeList = new List<BridgeFromTo>();

        //Bridge 정보 저장
        for(int i = 0; i < transform.GetChild(transform.childCount -1).childCount; i++)
        {
            BridgeData bridgeData = new BridgeData();

            Transform bridgeTransform = transform.GetChild(transform.childCount - 1).GetChild(i);
            bridgeData.bridgePosition = bridgeTransform.localPosition;
            bridgeData.bridgeRoatation = bridgeTransform.localEulerAngles;
            bridgeData.bridgeName = bridgeTransform.name;
            bridgeDataList.Add(bridgeData);

            Bridge bridge = bridgeTransform.GetComponent<Bridge>();
            //건설된 상태라면
            if(bridge.currentBridgeType == Bridge.BridgeType.Build)
            {
                string[] bridgeStrings = bridgeTransform.name.Split('_')[1].Split('/');
                BridgeFromTo bridgeId = new BridgeFromTo();
                bridgeId.fromId = int.Parse(bridgeStrings[0]);
                bridgeId.toId = int.Parse(bridgeStrings[1]);

                bridgeList.Add(bridgeId);
            }
        }
        ArrayLandData arrayLandData = new ArrayLandData();
        arrayLandData.landLists = landDataList;
        arrayLandData.bridgeLists = bridgeDataList;
        arrayLandData.bridgeInfo = bridgeList;
        //bridge 연결 리스트 저장

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
        //플레이어가 떨어지는 것을 방지
        user.GetComponent<Rigidbody>().useGravity = false;

       for(int i = 0; i < arrayLandData.landLists.Count; i++)
        {
            //Land 자체를 생성
            GameObject landResource = Resources.Load<GameObject>("SkyLand");
            GameObject land = Instantiate(landResource);
            land.name = land.name.Split('(')[0];
            land.name += (i + 1).ToString();
            land.transform.parent = transform;
            land.transform.localPosition = arrayLandData.landLists[i].landPosition;
            land.transform.localScale = arrayLandData.landLists[i].landScale;
            land.transform.localEulerAngles = arrayLandData.landLists[i].landEulerAngle;
            ArrayObjectsOfLand arrayObjectsOf = arrayLandData.landLists[i].arrayObjectsOfLand;
            
            //Land 위에 있는 것 Load 해서 발견하기
            for(int j = 0; j < arrayObjectsOf.objects.Count; j++)
            {
                GameObject objResource = Resources.Load<GameObject>(arrayObjectsOf.objects[j].path);
                GameObject obj = Instantiate(objResource);
                obj.name = obj.name.Split('(')[0];
                obj.transform.parent = land.transform;
                obj.transform.localPosition = arrayObjectsOf.objects[j].localPosition;
                obj.transform.localScale = arrayObjectsOf.objects[j].localScale;
                obj.transform.localEulerAngles = arrayObjectsOf.objects[j].localEulerAngle;
            }
       }

        LoadBridge(arrayLandData.bridgeInfo, arrayLandData.bridgeLists);

        user.GetComponent<Rigidbody>().useGravity = true;
    }

    /// <summary>
    /// Bridge 정보 초기화
    /// </summary>
    /// <param name="bridges"></param>
    public void LoadBridge(List<BridgeFromTo> bridges, List<BridgeData> bridgesData)
    {
        GameObject bridgeResource = Resources.Load<GameObject>("Object/Bridge");

        GameObject bridgeTemp = new GameObject("BridgeTemp");
        bridgeTemp.transform.parent = transform;
        for (int i = 0; i < bridgesData.Count; i++)
        {
            GameObject bridge = Instantiate(bridgeResource);
            bridge.transform.parent = bridgeTemp.transform;
            bridge.transform.localPosition = bridgesData[i].bridgePosition;    
            bridge.transform.localEulerAngles = bridgesData[i].bridgeRoatation;    
            bridge.name = bridgesData[i].bridgeName;
            string[] bridgeStrings = bridge.name.Split('_')[1].Split('/');
            for(int j = 0; j < bridges.Count; j++)
            {
                int a = int.Parse(bridgeStrings[0]);
                int b = int.Parse(bridgeStrings[1]);
                if (bridges[j].fromId == int.Parse(bridgeStrings[0]) &&
                    bridges[j].toId == int.Parse(bridgeStrings[1]))
                {
                    bridge.GetComponent<Bridge>().currentBridgeType = Bridge.BridgeType.Build;
                }
            }
        }
    }

    /// <summary>
    /// UI중에서 Bridge Build를 선택했을 경우
    /// </summary>
    public void SelectBuildMode()
    {
        buildMode = BuildMode.Bridge;
        cancelButton.SetActive(true);
    }
    /// <summary>
    /// UI중에서 Cancel Button을 누른 경우 
    /// 초기화
    /// </summary>
    public void CancelButton()
    {
        buildMode = BuildMode.None;
        cancelButton.SetActive(false);
        SaveLandData();
    }
}
