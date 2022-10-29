using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result<T>
{
    public bool result;
    public int count;
    public Data<T> data;
}
public class Data<T>
{
    T data;
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
        Result<ArrayLandDataSever> landData = await DataModule.WebRequest<Result<ArrayLandDataSever>>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);

        //이거 한번 해야한다.
        //ArrayLandData arrayLandData = FileManager.LoadDataFile<ArrayLandData>(landDataFileName);
        //DataTemporary.MyLandData = arrayLandData;

        //ArrayBridgeData arrayBridgeData = FileManager.LoadDataFile<ArrayBridgeData>(bridgeFileName);
        //DataTemporary.MyBridgeData = arrayBridgeData;

        //DataTemporary.MyLandData = landData;
        if(landData.result)
            Debug.Log(landData.data);

    }
}
