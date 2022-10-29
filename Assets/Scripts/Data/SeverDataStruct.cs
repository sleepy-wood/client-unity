using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerDataStruct
{

}
/// <summary>
/// 땅 위에 있는 오브젝트들의 정보를 저장하는 구조체
/// </summary>
[Serializable]
public class ObjectsInfoServer
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
    public string createAt;
    public string updateAt;
    public string deleteAt;
}

/// <summary>
/// 땅위에 있는 오브젝트들의 List
/// </summary>
[Serializable]
public class ArrayObjectsOfLandServer
{
    public List<ObjectsInfoServer>? objects = null;
}
/// <summary>
/// Land 하나에 있는 정보들
/// TODO: 랜드구입 등 랜드가 존재하는 형태인지 정보 넣어야함
/// </summary>
[Serializable]
public class LandDataSever
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
    public string createAt;
    public string updateAt;
    public string deleteAt;
    public ArrayObjectsOfLandServer? landDecorations;
}
/// <summary>             
/// 여러 Land들의 정보를 담을 List
/// </summary>
[Serializable]
public class ArrayLandDataSever
{
    public List<LandDataSever>? landLists = null;
}
