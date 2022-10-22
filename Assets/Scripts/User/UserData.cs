using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
[System.Serializable]
public class UserData
{
    public int? Id;
    public string? NickName;
    public string? UserAvatar;
    public SleepDataStruct? SleepData;
}
[System.Serializable]
public class SleepDataStruct
{
    public string? SleepAt;
    public string? WakeAt;
}
