using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//UserData: NickName, AvatarModelName, SleepData, UserInventory
public class UserData_Legacy : MonoBehaviour, IChangeData<UserData_Legacy.UserDataStruct>
{
    [System.Serializable]
    public struct UserDataStruct
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

    //ÀúÀåÇÒ UserData
    public UserDataStruct MyUserData;

    public void ChangeData(UserData_Legacy.UserDataStruct data) => MyUserData = data;
}
