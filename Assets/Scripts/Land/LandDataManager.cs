using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cysharp.Threading.Tasks.Triggers;
using System.Net;

class ResultTemp
{
    public bool result;
}

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


        //ArrayLandData arrayLandData = FileManager.LoadDataFile<ArrayLandData>(landDataFileName);
        //DataTemporary.MyLandData = arrayLandData;

        //ArrayBridgeData arrayBridgeData = FileManager.LoadDataFile<ArrayBridgeData>(bridgeFileName);
        //DataTemporary.MyBridgeData = arrayBridgeData;


    }
    [Header("껐다 킬 Minimap 오브젝트 할당")]
    [SerializeField] private List<GameObject> minimapObject = new List<GameObject>();

    public string landDataFileName = "LandData";
    public string bridgeFileName = "BridgeData";
    public BuildMode buildMode = BuildMode.None;
    public GameObject buildBridgeCamera;
    public bool testMode = false;

    private bool isOnClickMinimap = false;
    private bool isOnClickBuildMode = false;
    private GameObject user;
    public bool isLoad = false;

    private void Start()
    {
        user = GameManager.Instance.User;
        buildBridgeCamera.SetActive(false);
        for (int i = 0; i < minimapObject.Count; i++)
        {
            minimapObject[i].SetActive(false);
        }
        //LoadLandData();
        //LoadBridge();
        //SaveLandData();
        //SaveBridgeData();
    }

    private void Update()
    {
        if (isLoad)
        {
            //Build Mode - Bridge일때
            if (buildMode == BuildMode.Bridge)
                BuildBridge();
            else if (buildMode == BuildMode.None)
            {
                //Bridge 건설용 카메라 끄기
                buildBridgeCamera.SetActive(false);

                //건설되지 않은 Bridge 안보여주기
                for (int i = 0; i < transform.GetChild(transform.childCount - 1).childCount; i++)
                {
                    Transform bridge = transform.GetChild(transform.childCount - 1).GetChild(i);
                    if (bridge.GetChild(0).GetComponent<Bridge>().currentBridgeType != Bridge.BridgeType.Build)
                    {
                        bridge.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void BuildBridge()
    {
        //플레이어 Move 제어
        //user.GetComponent<UserInteract>().moveControl = true;

        //Bridge 건설용 카메라 키기
        buildBridgeCamera.SetActive(true);

        //건설되지 않은 Bridge 보여주기
        for (int i =0; i < transform.GetChild(transform.childCount - 1).childCount; i++)
        {
            transform.GetChild(transform.childCount - 1).GetChild(i).gameObject.SetActive(true);
        }

        //Bridge 선택layerMask
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Bridge");
        user.GetComponent<UserInteract>().ScreenToRayStair(buildBridgeCamera.GetComponent<Camera>(), layerMask);
    }

    /// <summary>
    /// LandManager 하위에 있는 Land들의 정보를 수정하는 함수
    /// 웹에 수정 요청을 할것
    /// </summary>
    public async void SaveLandData()
    {
        List<LandData> landDataList = new List<LandData>();
        for(int i = 0; i< transform.childCount - 1; i++)
        {
            //LandData
            LandData landData = new LandData();
            
            //Land 위에 있는 Object 정보 담기
            List<ObjectsInfo> objectList = new List<ObjectsInfo>();
            for(int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                if (transform.GetChild(i).GetChild(j).gameObject.layer != LayerMask.NameToLayer("Mark"))
                {
                    ObjectsInfo objectsInfo = new ObjectsInfo();
                    string dataPath = "Object/" + transform.GetChild(i).GetChild(j).name;
                    objectsInfo.path = dataPath;
                    objectsInfo.localPositionX = transform.GetChild(i).GetChild(j).localPosition.x;
                    objectsInfo.localPositionY = transform.GetChild(i).GetChild(j).localPosition.y;
                    objectsInfo.localPositionZ = transform.GetChild(i).GetChild(j).localPosition.z;

                    objectsInfo.localScaleX = transform.GetChild(i).GetChild(j).localScale.x;
                    objectsInfo.localScaleY = transform.GetChild(i).GetChild(j).localScale.y;
                    objectsInfo.localScaleZ = transform.GetChild(i).GetChild(j).localScale.z;

                    objectsInfo.localEulerAngleX = transform.GetChild(i).GetChild(j).localEulerAngles.x;
                    objectsInfo.localEulerAngleY = transform.GetChild(i).GetChild(j).localEulerAngles.y;
                    objectsInfo.localEulerAngleZ = transform.GetChild(i).GetChild(j).localEulerAngles.z;

                    objectList.Add(objectsInfo);
                }
            }

            //LandData 담기
            landData.landDecorations = objectList;
            landData.unityLandId = i+1;
            landData.landPositionX = transform.GetChild(i).localPosition.x;
            landData.landPositionY = transform.GetChild(i).localPosition.y;
            landData.landPositionZ = transform.GetChild(i).localPosition.z;

            landData.landScaleX = transform.GetChild(i).localScale.x;
            landData.landScaleY = transform.GetChild(i).localScale.y;
            landData.landScaleZ = transform.GetChild(i).localScale.z;
            
            landData.landEulerAngleX = transform.GetChild(i).localEulerAngles.x;
            landData.landEulerAngleY = transform.GetChild(i).localEulerAngles.y;
            landData.landEulerAngleZ = transform.GetChild(i).localEulerAngles.z;

            string landJsonData = JsonUtility.ToJson(landData);
            //Web에 데이터 수정
            ResultPut resultPut = await DataModule.WebRequest<ResultPut>(
                "/api/v1/lands/" + DataTemporary.MyLandData.landLists[i].id, 
                DataModule.NetworkType.PUT, 
                DataModule.DataType.BUFFER, 
                landJsonData);

            if (!resultPut.result)
            {
                Debug.LogError("WebRequestError: NetworkType[Put]");
            }

            landDataList.Add(landData);
        }

        ArrayLandData arrayLandData = new ArrayLandData();
        arrayLandData.landLists = landDataList;

        //DataTemporary에 Update or Save
        DataTemporary.MyLandData = arrayLandData;

        //File 형식으로 Update or Save
        FileManager.SaveDataFile(landDataFileName, arrayLandData);
    }
    /// <summary>
    /// Bridge정보 저장 및 수정
    /// </summary>
    public async void SaveBridgeData()
    {
        List<BridgeData> bridgeDataList = new List<BridgeData>();
        List<BridgeFromTo> bridgeList = new List<BridgeFromTo>();

        //Bridge 정보 저장
        for (int i = 0; i < transform.GetChild(transform.childCount - 1).childCount; i++)
        {
            BridgeData bridgeData = new BridgeData();

            Transform bridgeTransform = transform.GetChild(transform.childCount - 1).GetChild(i);
            Bridge bridge = bridgeTransform.GetChild(0).GetComponent<Bridge>();

            BridgeFromTo bridgeFromTo1 = new BridgeFromTo();
            if (testMode)
            {
                string[] bridgeStrings = bridgeTransform.name.Split('_')[1].Split('/');
                bridgeFromTo1.fromId = int.Parse(bridgeStrings[0]);
                bridgeFromTo1.toId = int.Parse(bridgeStrings[1]);
                bridgeFromTo1.bridgeId = i;
            }
            else
            {
                bridgeFromTo1.fromId = DataTemporary.MyBridgeData.bridgeLists[i].bridgeInfo.fromId;
                bridgeFromTo1.toId = DataTemporary.MyBridgeData.bridgeLists[i].bridgeInfo.toId;
                bridgeFromTo1.bridgeId = DataTemporary.MyBridgeData.bridgeLists[i].bridgeInfo.bridgeId;
            }


            bridgeData.bridgeInfo = bridgeFromTo1;

            bridgeData.bridgePositionX = bridgeTransform.localPosition.x;
            bridgeData.bridgePositionY = bridgeTransform.localPosition.y;
            bridgeData.bridgePositionZ = bridgeTransform.localPosition.z;

            bridgeData.bridgeRotationX = bridgeTransform.localEulerAngles.x;
            bridgeData.bridgeRotationY = bridgeTransform.localEulerAngles.y;
            bridgeData.bridgeRotationZ = bridgeTransform.localEulerAngles.z;


            //건설된 상태라면 그래프 구조에 넣기
            if (bridge.currentBridgeType != Bridge.BridgeType.NotBuild)
            {
                //건설 예정이면 건설 상태로 변경
                if (bridge.currentBridgeType == Bridge.BridgeType.WillBuild)
                {
                    bridge.currentBridgeType = Bridge.BridgeType.Build;
                }
                BridgeFromTo bridgeFromTo = new BridgeFromTo();
                if (testMode)
                {
                    bridgeData.name = bridgeTransform.name;
                    bridgeFromTo.fromId = 1;
                    bridgeFromTo.toId = 1;
                }
                else
                {
                    bridgeData.name = "Bridge"
                        + "_" + DataTemporary.MyBridgeData.bridgeLists[i].bridgeInfo.fromId
                        + "/" + DataTemporary.MyBridgeData.bridgeLists[i].bridgeInfo.toId;
                    bridgeFromTo.fromId = DataTemporary.MyBridgeData.bridgeLists[i].bridgeInfo.fromId;
                    bridgeFromTo.toId = DataTemporary.MyBridgeData.bridgeLists[i].bridgeInfo.toId;
                }
                bridgeList.Add(bridgeFromTo);
            }
            else
            {

                bridgeData.name = "Bridge";
            }

            //Web에 데이터 수정
            string bridgeJsonData = JsonUtility.ToJson(bridgeData);

            ResultPut resultPut = await DataModule.WebRequest<ResultPut>(
                "/api/v1/bridges/" + DataTemporary.MyBridgeData.bridgeLists[i].id,
                DataModule.NetworkType.PUT,
                DataModule.DataType.BUFFER,
                bridgeJsonData);

            if (!resultPut.result)
            {
                Debug.LogError("WebRequestError: NetworkType[Put]");
            }

            bridgeDataList.Add(bridgeData);
        }
        ArrayBridgeData arrayBridgeData = new ArrayBridgeData();

        arrayBridgeData.bridgeLists = bridgeDataList;

        //bridge 연결 리스트 저장
        DataTemporary.BridgeConnection = bridgeList;

        DataTemporary.MyBridgeData = arrayBridgeData;
        //파일로 저장
        FileManager.SaveDataFile(bridgeFileName, arrayBridgeData);
        //Web으로 저장

    }
    /// <summary>
    /// Load Land Data and Create Object of Land
    /// </summary>
    public void LoadLandData()
    {
        ArrayLandData arrayLandData = DataTemporary.MyLandData;
        //Web 하기 전 준비
        //ArrayLandData arrayLandData = FileManager.LoadDataFile<ArrayLandData>(landDataFileName);
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
            Vector3 localPosition = new Vector3((float)arrayLandData.landLists[i].landPositionX, (float)arrayLandData.landLists[i].landPositionY, (float)arrayLandData.landLists[i].landPositionZ);
            land.transform.localPosition = localPosition;


            Vector3 localScale = new Vector3((float)arrayLandData.landLists[i].landScaleX, (float)arrayLandData.landLists[i].landScaleY, (float)arrayLandData.landLists[i].landScaleZ);
            land.transform.localScale = localScale;

            Vector3 localEulerAngles = new Vector3((float)arrayLandData.landLists[i].landEulerAngleX, (float)arrayLandData.landLists[i].landEulerAngleY, (float)arrayLandData.landLists[i].landEulerAngleZ);
            land.transform.localEulerAngles = localEulerAngles;

            List<ObjectsInfo> arrayObjectsOf = arrayLandData.landLists[i].landDecorations;
            
            //Land 위에 있는 것 Load 해서 발견하기
            for(int j = 1; j < arrayObjectsOf.Count; j++)
            {
                GameObject objResource = Resources.Load<GameObject>(arrayObjectsOf[j].path);
                GameObject obj = Instantiate(objResource);
                obj.name = obj.name.Split('(')[0];
                obj.transform.parent = land.transform;

                Vector3 objPosition = new Vector3((float)arrayObjectsOf[j].localPositionX, (float)arrayObjectsOf[j].localPositionY, (float)arrayObjectsOf[j].localPositionZ);
                Vector3 objScale = new Vector3((float)arrayObjectsOf[j].localScaleX, (float)arrayObjectsOf[j].localScaleY, (float)arrayObjectsOf[j].localScaleZ);
                Vector3 objEulerAngles = new Vector3((float)arrayObjectsOf[j].localEulerAngleX, (float)arrayObjectsOf[j].localEulerAngleY, (float)arrayObjectsOf[j].localEulerAngleZ);
                
                obj.transform.localPosition = objPosition;
                obj.transform.localScale = objScale;
                obj.transform.localEulerAngles = objEulerAngles;
            }
       }

        user.GetComponent<Rigidbody>().useGravity = true;
    }



    /// <summary>
    /// Bridge 정보 초기화
    /// </summary>
    /// <param name="bridges"></param>
    public void LoadBridge()
    {
        ArrayBridgeData arrayBridgeData = DataTemporary.MyBridgeData;
        //ArrayBridgeData arrayBridgeData = FileManager.LoadDataFile<ArrayBridgeData>(bridgeFileName);
        List<BridgeData> bridgesData = arrayBridgeData.bridgeLists;

        List<BridgeFromTo> bridgeList = new List<BridgeFromTo>();

        GameObject bridgeResource = Resources.Load<GameObject>("Object/Bridge");

        GameObject bridgeTemp = new GameObject("BridgeTemp");
        bridgeTemp.transform.parent = transform;
        for (int i = 0; i < bridgesData.Count; i++)
        {
            GameObject bridge = Instantiate(bridgeResource);
            bridge.transform.parent = bridgeTemp.transform;

            Vector3 bridgePosition = new Vector3((float)bridgesData[i].bridgePositionX, (float)bridgesData[i].bridgePositionY, (float)bridgesData[i].bridgePositionZ);
            Vector3 bridgeRotation = new Vector3((float)bridgesData[i].bridgeRotationX, (float)bridgesData[i].bridgeRotationY, (float)bridgesData[i].bridgeRotationZ);
            
            bridge.transform.localPosition = bridgePosition;    
            bridge.transform.localEulerAngles = bridgeRotation;    
            bridge.name = bridgesData[i].name;
            if (bridge.name.Length > "Bridge".Length)
            {
                
                bridge.transform.GetChild(0).GetComponent<Bridge>().currentBridgeType = Bridge.BridgeType.Build;

                string[] bridgeStrings = bridge.name.Split('_')[1].Split('/');
                BridgeFromTo bridgeFromTo = new BridgeFromTo();
                bridgeFromTo.fromId = int.Parse(bridgeStrings[0]);
                bridgeFromTo.toId = int.Parse(bridgeStrings[1]);

                bridgeList.Add(bridgeFromTo);
            }
        }
        DataTemporary.BridgeConnection = bridgeList;
    }

    /// <summary>
    /// UI중에서 Bridge Build를 선택했을 경우
    /// </summary>
    public void OnClickSelectBuildMode()
    {
        for (int i = 0; i < minimapObject.Count; i++)
        {
            if (minimapObject[i].activeSelf)
                return;
        }
        if (!isOnClickBuildMode)
        {
            buildMode = BuildMode.Bridge;
            isOnClickBuildMode = true;
        }
        else
        {
            buildMode = BuildMode.None;
            SaveBridgeData();
            SaveLandData();
            isOnClickBuildMode = false;
        }
    }
    /// <summary>
    /// Minimap Button을 눌렀을 경우
    /// </summary>
    public void OnClickMinimapButton()
    {
        if (buildBridgeCamera.activeSelf)
            return;
        for(int i = 0; i < minimapObject.Count; i++)
        {
            if (!isOnClickMinimap)
            {
                minimapObject[i].SetActive(true);
            }
            else
            {
                minimapObject[i].SetActive(false);
            }
        }

        for (int j = 0; j < transform.childCount - 1; j++)
        {
            transform.GetChild(j).GetChild(0).gameObject.SetActive(false);
        }

        if (!isOnClickMinimap)
        {
            user.GetComponent<UserInteract>().moveControl = true;
        }
        else
        {
            user.GetComponent<UserInteract>().moveControl = false;
        }

        isOnClickMinimap = isOnClickMinimap == true ? false : true;
    }
}
