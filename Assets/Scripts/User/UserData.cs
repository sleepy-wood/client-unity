using NativePlugin.HealthData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
[System.Serializable]
public class UserData
{
    public int id;
    public string? profileImg;
    public string? type;
    public string? nickName;
    public string? avatar;
    public int badgeCount;
    public int productCount;
    public string? hp;
    public int currentLandId;
    public string? createdAt;
    public string? updatedAt;
}
[System.Serializable]
public class SleepDataStruct
{
    public DateTime StartDate;
    public DateTime EndDate;
    public SleepType Type;
}
[System.Serializable]
public class ArraySleepData
{
    public List<SleepDataStruct>? arraySleepData;
}
