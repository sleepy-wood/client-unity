using NativePlugin.HealthData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터 저장소
/// </summary>
public class DataTemporary
{
    //UserData
    public static UserData MyUserData = new UserData();
    //LandData
    public static ArrayLandData MyLandData = new ArrayLandData();
    //BridgeData
    public static ArrayBridgeData MyBridgeData = new ArrayBridgeData();
    //알고리즘에 활용할 Connection 정보
    public static List<BridgeFromTo> BridgeConnection = new List<BridgeFromTo>();
    //AssetBundles
    public static AssetBundle assetBundle;
    public static AssetBundle assetBundleCustom;
    public static AssetBundle assetBundleImg;
    //금칙어
    public static AssetBundle stopwordsAsset;
    //SleepDatas
    public static SleepSample[] samples = new SleepSample[100];
    // Tree Data
    public static AssetBundle assetBundleTreePipeline;
    public static ArrayPutTreeData PutTreeData = new ArrayPutTreeData();
    public static ArrayGetTreeData GetTreeData = new ArrayGetTreeData();
    //Market Data
    public static ArrayMarketData arrayMarketData = new ArrayMarketData();
}
