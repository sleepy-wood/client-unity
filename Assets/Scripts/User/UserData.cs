using NativePlugin.HealthData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
[System.Serializable]
public class UserData
{
    public int Id;
    public int LandId;
    public string? NickName;
    public string? UserAvatar;
    public SleepDataStruct? SleepData;
}
[System.Serializable]
public class SleepDataStruct
{
    public DateTime StartDate;
    public DateTime EndDate;
    public SleepType Type;
}
