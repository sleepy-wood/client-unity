using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 통신모듈
// 1. 보내기
// 2. 받기
// 3. 각각 보낼 데이터 양식
public class DataModule_Legacy
{
    //public enum NET_RESULT
    //{
    //    NET_SUCCESS,
    //    NET_FAIL,
    //}
    #region UserData

    ////저장소에 있는 Data 보내기 
    //UserData RecieveUserData()
    //{
    //    return userData2;
    //}

    ////새로고침 눌렀을 경우 load 해서 return.
    //public UserData RefreshUserData(string url)
    //{
    //    //url을 통해 load 받고,(맞나)
    //    //만약에 json이 생긴다면?
    //    string json = "";
    //    UserData userdata = JsonUtility.FromJson<UserData>(json);
    //    userData2 = userdata;
    //    //userData2 에 그 정보를 끼워 넣는다.
    //    return userData2;
    //}
    ////변경된 Data를 보내기
    //public NET_RESULT SendUserData(UserData user)
    //{
    //    userData2 = user;
    //    string json = JsonUtility.ToJson(user);

    //    //userData2를 Web으로 Save
    //    //오류 안생겼으면 NET_SUCCESS return
    //    //생기면 NET_FAIL return
    //    return NET_RESULT.NET_SUCCESS;
    //}
    #endregion

}
