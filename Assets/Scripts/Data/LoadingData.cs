using System.Collections;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Random = UnityEngine.Random;

public class LoadingData : MonoBehaviourPunCallbacks
{
    NativeLoadData nativeLoad = new NativeLoadData();
    [SerializeField] private GameObject scrollbar_right;
    [SerializeField] private GameObject scrollbar_left;
    [SerializeField] private float scrollbarSpeed = 2;

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

        //닉네임 설정
        PhotonNetwork.NickName = Random.Range(0, 10).ToString();
        Debug.Log(PhotonNetwork.NickName);
        CreateRoom();
    }
    private void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();

        roomOptions.MaxPlayers = 3;
        roomOptions.IsVisible = true;
        if(PhotonNetwork.CreateRoom(PhotonNetwork.NickName, roomOptions))
            isCreateComplete = true;
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
        OnConnect();
        if (!m_testMode)
        {
            scrollbar_left.SetActive(false);
            StartCoroutine(StartLoading());
        }
        //Native Data Load
        nativeLoad.LoadNativeData();

        //AssetBundle Load
        //await DataModule.WebRequestAssetBundle("/assets/testbundle", DataModule.NetworkType.GET, DataModule.DataType.ASSETBUNDLE);

        //UserData 
        //UserData userData = await DataModule.WebRequest<UserData>("/api/v1/users", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        //DataTemporary.MyUserData = userData;

        //LandData Load
        //Root landData = await DataModule.WebRequest<Root>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        ResultGet<LandData> landData = await DataModule.WebRequest<ResultGet<LandData>>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        ResultGet<BridgeData> bridgeData = await DataModule.WebRequest<ResultGet<BridgeData>>("/api/v1/bridges", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);

        ArrayLandData arrayLandData = new ArrayLandData();
        arrayLandData.landLists = landData.data;
        //DataTemporary.MyLandData = arrayLandData;

        ArrayBridgeData arrayBridgeData = new ArrayBridgeData();
        arrayBridgeData.bridgeLists = bridgeData.data;
        //DataTemporary.MyBridgeData = arrayBridgeData;


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

        if (!m_testMode)
        {
            if (landData.result && bridgeData.result)
            {
                //PhotonNetwork.LoadLevel(1);
                isLoadingComplete = true;
            }
        }
    }
    private void Update()
    {
        if (isLoadingComplete && isCreateComplete)
        {
            PhotonNetwork.LoadLevel(1);
            isCreateComplete = false;
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
