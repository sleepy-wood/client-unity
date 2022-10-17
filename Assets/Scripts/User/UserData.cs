using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UserData
{
    public int Id;
    public string NickName;
    public string UserAvatar;
    public SleepDataStruct SleepData;
}
[System.Serializable]
public struct SleepDataStruct
{
    public string SleepAt;
    public string WakeAt;
}
