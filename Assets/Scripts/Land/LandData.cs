using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// �� ���� �ִ� ������Ʈ���� ������ �����ϴ� ����ü
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
/// ������ �ִ� ������Ʈ���� List
/// </summary>
[Serializable]
public struct ArrayObjectsOfLand
{
    public List<ObjectsInfo> Objects;
}

/// <summary>
/// Land �ϳ��� �ִ� ������
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
/// ���� Land���� ������ ���� List
/// </summary>
[Serializable]
public struct ArrayLandData
{
    public List<LandData> LandLists;
}
