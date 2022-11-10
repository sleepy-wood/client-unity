using JetBrains.Annotations;
using NativePlugin.HealthData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

[Serializable]
public class UserData
{
    public int id;
    public string profileImg;
    public string type;
    public string nickname;
    public string avatar;
    public int badgeCount;
    public int productCount;
    public string hp;
    public int currentLandId;
    public string createdAt;
    public string updatedAt;
}

[Serializable]
public class UserLogin
{
    public string token;
    public UserData user;
}
[Serializable]
public class SleepDataStruct
{
    public DateTime StartDate;
    public DateTime EndDate;
    public SleepType Type;
}
[Serializable]
public class ArraySleepData
{
    public List<SleepDataStruct> arraySleepData;
}
