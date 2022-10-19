using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 땅 위에 있는 오브젝트들의 정보를 저장하는 구조체
/// </summary>
[Serializable]
public struct ObjectsInfo
{
    public string path;
    public Vector3 localPosition;
    public Vector3 localScale;
    public Vector3 localEulerAngle;
}

/// <summary>
/// 땅위에 있는 오브젝트들의 List
/// </summary>
[Serializable]
public struct ArrayObjectsOfLand
{
    public List<ObjectsInfo> Objects;
}

/// <summary>
/// Land 하나에 있는 정보들
/// </summary>
[Serializable]
public struct LandData
{
    public int landNum;
    public Vector3 landPosition;
    public Vector3 landScale;
    public Vector3 landEulerAngle;
    public ArrayObjectsOfLand arrayObjectsOfLand;
}

/// <summary>
/// 여러 Land들의 정보를 담을 List
/// </summary>
[Serializable]
public struct ArrayLandData
{
    public List<LandData> LandLists;
}
