using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;
#nullable enable
/// <summary>
/// 땅 위에 있는 오브젝트들의 정보를 저장하는 구조체
/// List로 담아야함
/// </summary>
[Serializable]
public class ObjectsInfo
{
    public int id;
    public string? path;
    public float localPositionX;
    public float localPositionY;
    public float localPositionZ;
    public float localScaleX;
    public float localScaleY;
    public float localScaleZ;
    public float localEulerAngleX;
    public float localEulerAngleY;
    public float localEulerAngleZ;
    public int landId;
    public int userId;
    public DateTime createdAt;
    public DateTime updatedAt;
    public object? deletedAt;
}
/// <summary>
/// Land 하나에 있는 정보들
/// TODO: 랜드구입 등 랜드가 존재하는 형태인지 정보 넣어야함
/// </summary>
[Serializable]
public class LandData
{
    public int? id;
    public int? unityLandId;
    public float landPositionX;
    public float landPositionY;
    public float landPositionZ;
    public float landScaleX;
    public float landScaleY;
    public float landScaleZ;
    public float landEulerAngleX;
    public float landEulerAngleY;
    public float landEulerAngleZ;
    public int userId;
    public DateTime createdAt;
    public DateTime updatedAt;
    public object? deletedAt;
    public List<ObjectsInfo>? landDecorations;
}
[Serializable]
public class BridgeData
{
    public int? id;
    public string? name;
    public float bridgePositionX;
    public float bridgePositionY;
    public float bridgePositionZ;
    public float bridgeRoatationX;
    public float bridgeRoatationY;
    public float bridgeRoatationZ;
    public int userId;
    public DateTime createdAt;
    public DateTime updatedAt;
    public object? deletedAt;
    public BridgeFromTo? bridgeInfo;
}
//TODO: 섬들의 연결 관계를 담아줄 리스트 만들어야함
//          => 알고리즘에 써야해
[Serializable]
public class BridgeFromTo
{
    public int bridgeId;
    public int fromId;
    public int toId;
    public DateTime createdAt;
    public DateTime updatedAt;
    public object? deletedAt;
}
/// <summary>             
/// 여러 Land들의 정보를 담을 List
/// </summary>
[Serializable]
public class ArrayLandData
{
    public List<LandData>? landLists;
}
/// <summary>
/// 여러 Bridge 정보를 담을 List
/// </summary>
[Serializable]
public class ArrayBridgeData
{
    public List<BridgeData>? bridgeLists;
}
