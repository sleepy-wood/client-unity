using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#nullable enable
/// <summary>
/// �� ���� �ִ� ������Ʈ���� ������ �����ϴ� ����ü
/// </summary>
[Serializable]
public class ObjectsInfo
{
    public string? path = null;
    public Vector3? localPosition = null;
    public Vector3? localScale = null;
    public Vector3? localEulerAngle = null;
}

/// <summary>
/// ������ �ִ� ������Ʈ���� List
/// </summary>
[Serializable]
public class ArrayObjectsOfLand
{
    public List<ObjectsInfo>? Objects = null;
}

/// <summary>
/// Land �ϳ��� �ִ� ������
/// </summary>
[Serializable]
public class LandData
{
    public int? landNum = null;
    public Vector3? landPosition = null;
    public Vector3? landScale = null;
    public Vector3? landEulerAngle = null;
    public ArrayObjectsOfLand? arrayObjectsOfLand = null;
}

/// <summary>
/// ���� Land���� ������ ���� List
/// </summary>
[Serializable]
public class ArrayLandData
{
    public List<LandData>? LandLists= null;
}
