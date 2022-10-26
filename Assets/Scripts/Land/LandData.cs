using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;
#nullable enable
/// <summary>
/// 땅 위에 있는 오브젝트들의 정보를 저장하는 구조체
/// </summary>
[Serializable]
public class ObjectsInfo
{
    public string? path;
    public Vector3 localPosition;
    public Vector3 localScale;
    public Vector3 localEulerAngle;
}

/// <summary>
/// 땅위에 있는 오브젝트들의 List
/// </summary>
[Serializable]
public class ArrayObjectsOfLand
{
    public List<ObjectsInfo>? objects = null;
}
/// <summary>
/// Land 하나에 있는 정보들
/// TODO: 랜드구입 등 랜드가 존재하는 형태인지 정보 넣어야함
/// </summary>
[Serializable]
public class LandData
{
    public int? landNum;
    public Vector3 landPosition;
    public Vector3 landScale;
    public Vector3 landEulerAngle;
    public ArrayObjectsOfLand? arrayObjectsOfLand;
}
[Serializable]
public class BridgeData
{
    public string? bridgeName;
    public Vector3 bridgePosition;
    public Vector3 bridgeRoatation;
}
[Serializable]
public class BridgeFromTo
{
    public int fromId;
    public int toId;
}
/// <summary>
/// 여러 Land들의 정보를 담을 List
/// </summary>
[Serializable]
public class ArrayLandData
{
    public List<LandData>? landLists=null;
    public List<BridgeData>? bridgeLists;
    public List<BridgeFromTo>? bridgeInfo;
}
