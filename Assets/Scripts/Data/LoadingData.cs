using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;


public class LoadingData : MonoBehaviour
{
    NativeLoadData nativeLoad = new NativeLoadData();
    public GameObject startButton;
    private async void Start()
    {
        //Native Data Load
        //nativeLoad.LoadNativeData();

        //AssetBundle Load
        //await DataModule.WebRequestAssetBundle("/assets/testbundle", DataModule.NetworkType.GET, DataModule.DataType.ASSETBUNDLE);

        //UserData 
        //UserData userData = await DataModule.WebRequest<UserData>("/api/v1/users", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        //DataTemporary.MyUserData = userData;

        //LandData Load
        //TODO: Land Data 구조 수정s
        //Root landData = await DataModule.WebRequest<Root>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        ResultGet<LandData> landData = await DataModule.WebRequest<ResultGet<LandData>>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        ResultGet<BridgeData> bridgeData = await DataModule.WebRequest<ResultGet<BridgeData>>("/api/v1/bridges", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);

        ArrayLandData arrayLandData = new ArrayLandData();
        arrayLandData.landLists = landData.data;
        DataTemporary.MyLandData = arrayLandData;

        ArrayBridgeData arrayBridgeData = new ArrayBridgeData();
        arrayBridgeData.bridgeLists = bridgeData.data;
        DataTemporary.MyBridgeData = arrayBridgeData;


        if (landData.result)
        {
            Debug.Log(landData.data);
            DataTemporary.MyLandData = arrayLandData;
        }
        if(landData.result)
        {
            Debug.Log(bridgeData.data);
            DataTemporary.MyBridgeData = arrayBridgeData;
        }

        if (landData.result && bridgeData.result)
        {
            //LandDataManager.Instance.LoadLandData();
            //LandDataManager.Instance.LoadBridge();
            //LandDataManager.Instance.isLoad = true;
            startButton.SetActive(true);
        }
    }
    public void OnClickStartButton()
    {
        SceneManager.LoadScene(1);
    }
}
