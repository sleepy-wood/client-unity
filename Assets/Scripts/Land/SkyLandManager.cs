using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyLandManager : MonoBehaviour
{
    public static SkyLandManager Instance;

    private AssetBundle assetBundle;
    void Awake()
    {
        if (!Instance)
            Instance = this;

        assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundles/landcustombundle");

        //LoadData();
    }
    public async void LoadData()
    {
        int landId = DataTemporary.MyUserData.LandId;
        ResultGetId<LandData> landDataResponse = await DataModule.WebRequest<ResultGetId<LandData>>("/api/v1/lands/" + landId, DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        if (!landDataResponse.result)
        {
            Debug.LogError("WebRequestError: NetworkType[Get]");
            return;
        }
        Transform parent = transform.GetChild(0);
        LandData landData = landDataResponse.data;

        for(int i = 0; i < landData.landDecorations.Count; i++)
        {
            GameObject resource = assetBundle.LoadAsset<GameObject>(landData.landDecorations[i].path);
            //동기화가 안될거 같은데,,,
            GameObject decoration = Instantiate(resource);
            decoration.transform.parent = parent;
            Vector3 localPostion = new Vector3(landData.landDecorations[i].localPositionX, landData.landDecorations[i].localPositionY, landData.landDecorations[i].localPositionZ);
            Vector3 localScale = new Vector3(landData.landDecorations[i].localScaleX, landData.landDecorations[i].localScaleY, landData.landDecorations[i].localScaleZ);
            Vector3 localEulerAngle = new Vector3(landData.landDecorations[i].localEulerAngleX, landData.landDecorations[i].localEulerAngleY, landData.landDecorations[i].localEulerAngleZ);
            decoration.transform.localPosition = localPostion;
            decoration.transform.localScale = localScale;
            decoration.transform.localEulerAngles = localEulerAngle;
        }
    }

    public async void SaveData()
    {
        int landId = DataTemporary.MyUserData.LandId;
        LandData landData = DataTemporary.MyLandData.landLists[landId];
        Transform landDecorations = transform.GetChild(0);
        for (int i = 0; i < landDecorations.childCount; i++)
        {
            ObjectsInfo objectsInfo = new ObjectsInfo();
            objectsInfo.path = landDecorations.GetChild(i).name;
            objectsInfo.localPositionX = landDecorations.GetChild(i).localPosition.x;
            objectsInfo.localPositionY = landDecorations.GetChild(i).localPosition.y;
            objectsInfo.localPositionZ = landDecorations.GetChild(i).localPosition.z;
            objectsInfo.localScaleX = landDecorations.GetChild(i).localScale.x;
            objectsInfo.localScaleY = landDecorations.GetChild(i).localScale.y;
            objectsInfo.localScaleZ = landDecorations.GetChild(i).localScale.z;
            objectsInfo.localEulerAngleX = landDecorations.GetChild(i).localEulerAngles.x;
            objectsInfo.localEulerAngleY = landDecorations.GetChild(i).localEulerAngles.y;
            objectsInfo.localEulerAngleZ = landDecorations.GetChild(i).localEulerAngles.z;
            landData.landDecorations.Add(objectsInfo);
        }

        string jsonData = JsonUtility.ToJson(landData);

        ResultPut resultPut = await DataModule.WebRequest<ResultPut>("/api/v1/lands/" + landId, DataModule.NetworkType.PUT, DataModule.DataType.BUFFER, jsonData);

        if (!resultPut.result)
            Debug.LogError("WebRequestError: NetworkType[Put]");
        else
            Debug.Log("수정 성공");
    }

    //[PunRPC]
    //public void RPC_LoadObjects()
    //{

    //}
}
