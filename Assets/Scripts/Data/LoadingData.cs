using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Result<T>
{
    public bool result;
    public int count;
    public List<T> data;
}

public class LoadingData : MonoBehaviour
{
    NativeLoadData nativeLoad = new NativeLoadData();
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
        Result<LandData> landData = await DataModule.WebRequest<Result<LandData>>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        Result<BridgeData> bridgeData = await DataModule.WebRequest<Result<BridgeData>>("/api/v1/bridges", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);

        //이거 한번 해야한다.
        //ArrayLandData arrayLandData = FileManager.LoadDataFile<ArrayLandData>(landDataFileName);
        //DataTemporary.MyLandData = arrayLandData;

        //ArrayBridgeData arrayBridgeData = FileManager.LoadDataFile<ArrayBridgeData>(bridgeFileName);
        //DataTemporary.MyBridgeData = arrayBridgeData;

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

    }
}
