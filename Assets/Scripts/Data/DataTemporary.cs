using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터 저장소
/// </summary>
public class DataTemporary
{
    public static UserData MyUserData = new UserData();
    public static ArrayLandData MyLandData = new ArrayLandData();
    public static ArrayBridgeData MyBridgeData = new ArrayBridgeData();
    //알고리즘에 활용할 Connection 정보
    public static List<BridgeFromTo> BridgeConnection = new List<BridgeFromTo>();
    public static AssetBundle assetBundle;
}
