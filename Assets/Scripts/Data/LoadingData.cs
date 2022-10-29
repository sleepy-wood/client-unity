using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        ArrayLandData landData = await DataModule.WebRequest<ArrayLandData>("/api/v1/lands", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        //JSONObject obj = new JSONObject(landData);
        //Dictionary<string, string> dic = obj.ToDictionary();
        //JSONObject j = new JSONObject(JSONObject.Type.Object);
        //JSONObject arr = new JSONObject(JSONObject.Type.Array);
        //j.AddField("field2", "SampleText");
        //j.AddField("field2", arr);

        //string encodedString = j.Print();
        //Debug.Log(encodedString);

        //DataTemporary.MyLandData = landData;

    }
}
