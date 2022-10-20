using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
[System.Serializable]
public class UserData
{
    public int? Id = null;
    public string? NickName = null;
    public string? UserAvatar = null;
    public SleepDataStruct? SleepData = null;
}
[System.Serializable]
public class SleepDataStruct
{
    public string? SleepAt = null;
    public string? WakeAt = null;
}
