using System.Collections;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingData : MonoBehaviour
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
        Debug.Log("-1");
        if (!m_testMode)
        {
            right = scrollbar_right.GetComponent<Scrollbar>();
            left = scrollbar_left.GetComponent<Scrollbar>();
        }
    }

    private async void Start()
    {
        Debug.Log("0");
        if (!m_testMode)
        {
            scrollbar_left.SetActive(false);
            StartCoroutine(StartLoading());
        }
        //Native Data Load
        Debug.Log("1");
        nativeLoad.LoadNativeData();
        Debug.Log("2");

        //AssetBundle Load
        //await DataModule.WebRequestAssetBundle("/assets/testbundle", DataModule.NetworkType.GET, DataModule.DataType.ASSETBUNDLE);

        //UserData 
        //UserData userData = await DataModule.WebRequest<UserData>("/api/v1/users", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        //DataTemporary.MyUserData = userData;

        //LandData Load
        //Root landData = await DataModule.WebRequest<Root>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        //ResultGet<LandData> landData = await DataModule.WebRequest<ResultGet<LandData>>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        //ResultGet<BridgeData> bridgeData = await DataModule.WebRequest<ResultGet<BridgeData>>("/api/v1/bridges", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);

        //ArrayLandData arrayLandData = new ArrayLandData();
        //arrayLandData.landLists = landData.data;
        //DataTemporary.MyLandData = arrayLandData;

        //ArrayBridgeData arrayBridgeData = new ArrayBridgeData();
        //arrayBridgeData.bridgeLists = bridgeData.data;
        //DataTemporary.MyBridgeData = arrayBridgeData;


        //if (landData.result)
        //{
        //    Debug.Log(landData.data);
        //    DataTemporary.MyLandData = arrayLandData;
        //}
        //if(bridgeData.result)
        //{
        //    Debug.Log(bridgeData.data);
        //    DataTemporary.MyBridgeData = arrayBridgeData;
        //}

        //if (!m_testMode)
        //{
        //    if (landData.result && bridgeData.result)
        //    {
        //        SceneManager.LoadScene(1);
        //    }
        //}
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
