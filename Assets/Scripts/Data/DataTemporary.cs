using NativePlugin.HealthData;
using System;
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
    //Activity Datas
    public static ActivitySample[] activitySamples = new ActivitySample[100];
    // Tree Data
    public static AssetBundle assetBundleTreePipeline;
    public static ArrayTreeData TreeData = new ArrayTreeData(); // Day1
    public static ArrayTreeData2 TreeData2 = new ArrayTreeData2();  // Day2~Day5
    public static ArrayGetTreeData GetTreeData = new ArrayGetTreeData();
    //Market Data
    public static ArrayMarketData arrayMarketData = new ArrayMarketData();
    public static List<string> emoji_Url = new List<string>();
    //public static List<List<string>> market_url = new List<List<string>>();
    public static Dictionary<DateTime, TimeSpan> DateTimeTotalTimeSpan = new Dictionary<DateTime, TimeSpan>();

}
