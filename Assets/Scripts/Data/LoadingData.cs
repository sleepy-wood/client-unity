using System.Collections;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Random = UnityEngine.Random;
using NativePlugin.HealthData;
using System.Collections.Generic;
using System.Linq;
//using TreeEditor;

public class LoadingData : MonoBehaviourPunCallbacks
{
    NativeLoadData nativeLoad = new NativeLoadData();
    [SerializeField] private GameObject scrollbar_right;
    [SerializeField] private GameObject scrollbar_left;
    [SerializeField] private float scrollbarSpeed = 2;
    [SerializeField] private int loginId = 1; 

    public bool m_testMode = false;
    private Scrollbar right;
    private Scrollbar left;

    private void Awake()
    {
        if (!m_testMode)
        {
            right = scrollbar_right.GetComponent<Scrollbar>();
            left = scrollbar_left.GetComponent<Scrollbar>();
        }
    }
    public void OnConnect()
    {
        //마스터 서버에 접속 요청
        PhotonNetwork.ConnectUsingSettings();
    }
    //마스터 서버에 접속 성공, 로비 생성 및 진입을 할 수 없는 상태
    public override void OnConnected()
    {
        base.OnConnected();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        //print("OnConnected");
    }
    //마스터 서버에 접속, 로비 생성 및 진입이 가능한 상태
    //이때 로비에 진입해야함
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        //print("OnConnectedToMaster");

        string imgNum = DataTemporary.MyUserData.profileImg.Split('.')[0];
        //닉네임 설정
        PhotonNetwork.NickName = DataTemporary.MyUserData.nickname;
        Debug.Log(PhotonNetwork.NickName);
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        CreateRoom();
    }
    private void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();

        roomOptions.MaxPlayers = 3;
        roomOptions.IsVisible = true;
        if (PhotonNetwork.CreateRoom(PhotonNetwork.NickName, roomOptions))
        {
            isCreateComplete = true;
        }
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        //방생성 실패 UI로 알려주기
    }
    bool isLoadingComplete = false;
    bool isCreateComplete = false;


    private async void Start()
    {
        if (!m_testMode)
        {
            scrollbar_left.SetActive(false);
            StartCoroutine(StartLoading());
        }

        DataTemporary.assetBundleCustom = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundles/landcustombundle");
        DataTemporary.assetBundleImg = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundles/landcustomimg");

        //로그인
        //"/api/v1/auth/login/temp/{id}"
        ResultPost<UserLogin> login = await DataModule.WebRequestBuffer<ResultPost<UserLogin>>("/api/v1/auth/login/temp/" + loginId, DataModule.NetworkType.POST, DataModule.DataType.BUFFER);
        if (login.result)
        {
            DataModule.REPLACE_BEARER_TOKEN = login.data.token;
        }
        //Native Data Load
        HealthDataStore.Init();
        DataTemporary.samples = HealthDataStore.GetSleepSamples(new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                DateTime.Now);

        SleepSample[] sleepsData = DataTemporary.samples;
        List<SleepSample> sleepSamples = new List<SleepSample>();
        //중복데이터 제거
        sleepSamples = sleepsData.Distinct().ToList();
        sleepSamples.Sort((x, y) => DateTime.Compare(x.StartDate, y.StartDate));
        sleepsData = sleepSamples.ToArray();

        DataTemporary.samples = sleepsData;

        //AssetBundle Load
        //await DataModule.WebRequestAssetBundle("/assets/testbundle", DataModule.NetworkType.GET, DataModule.DataType.ASSETBUNDLE);

        //UserData 
        ResultGetId<UserData> userData = await DataModule.WebRequestBuffer<ResultGetId<UserData>>("/api/v1/users", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);

        //LandData Load
        //Root landData = await DataModule.WebRequest<Root>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        ResultGet<LandData> landData = await DataModule.WebRequestBuffer<ResultGet<LandData>>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        ResultGet<BridgeData> bridgeData = await DataModule.WebRequestBuffer<ResultGet<BridgeData>>("/api/v1/bridges", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);

        // TreeData Load
        //ResultGet<TreeData> treeData = await DataModule.WebRequest<ResultGet<TreeData>>("api/v1/trees", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);

        ArrayLandData arrayLandData = new ArrayLandData();
        arrayLandData.landLists = landData.data;
        //DataTemporary.MyLandData = arrayLandData;

        ArrayBridgeData arrayBridgeData = new ArrayBridgeData();
        arrayBridgeData.bridgeLists = bridgeData.data;
        //DataTemporary.MyBridgeData = arrayBridgeData;

        //ArrayTreeData arrayTreeData = new ArrayTreeData();
        //arrayTreeData.treeDataList = treeData.data;


        if (landData.result)
        {
            Debug.Log(landData.data);
            DataTemporary.MyLandData = arrayLandData;
        }
        if (bridgeData.result)
        {
            Debug.Log(bridgeData.data);
            DataTemporary.MyBridgeData = arrayBridgeData;
        }
        if (userData.result)
        {
            DataTemporary.MyUserData = userData.data;
        }
        //if (treeData.result)
        //{
        //    Debug.Log(treeData.data);
        //    DataTemporary.MyTreeData = arrayTreeData;
        //}

        if (!m_testMode)
        {
             //&& treeData.result
            if (landData.result && bridgeData.result && userData.result && login.result)
            {
                isLoadingComplete = true;
            }
        }
    }
    private float curTime = 0;
    private void Update()
    {
        if (isLoadingComplete)
        {
            OnConnect();
            isLoadingComplete = false;
        }
        if (isCreateComplete)
        {
            curTime += Time.deltaTime;
            if (curTime > 3)
            {
                PhotonNetwork.LoadLevel(1);
                isCreateComplete = false;
            }
        }
    }
    public IEnumerator StartLoading()
    {
        float t = 0;
        bool isRight = true;
        while (true)
        {
            if (isRight)
            {
                t += Time.deltaTime * scrollbarSpeed;
                if (t > 1)
                {
                    left.size = 1;
                    scrollbar_right.SetActive(false);
                    scrollbar_left.SetActive(true);
                    t = 1;
                    isRight = false;
                }
                right.size = t;
            }
            else
            {

                t -= Time.deltaTime * scrollbarSpeed;
                if (t < 0)
                {
                    right.size = 0;
                    scrollbar_right.SetActive(true);
                    scrollbar_left.SetActive(false);
                    t = 0;
                    isRight = true;
                }
                left.size = t;
            }
            yield return null;
        }
    }
}
