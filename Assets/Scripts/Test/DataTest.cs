using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataTest : MonoBehaviour
{
    //UserData userData = new UserData();
    NativeLoadData nativeLoad = new NativeLoadData();

    private void Start()
    {
        nativeLoad.LoadNativeData();
    }

    //private async void Start()
    //{
    //    // IOS에서 기져올 데이터
    //    //SleepDataStruct sleepDataStruct = new SleepDataStruct();
    //    //sleepDataStruct.WakeAt = "2022-10-20 10:10:10";
    //    //sleepDataStruct.SleepAt = "2022-10-20 18:10:10";

    //    // 서버에서 가져올 데이터
    //    //userData.Id = 1;
    //    //userData.NickName = "test";
    //    //userData.UserAvatar = "Julia";
    //    //userData.SleepData = sleepDataStruct;

    //    ResultTemp<Token> data = await DataModule.WebRequest<ResultTemp<Token>>("/api/v1/auth/login/temp", DataModule.NetworkType.POST, DataModule.DataType.BUFFER);

    //    Debug.Log(data.result);

    //    if (data.result)
    //    {
    //        Token data1 = data.data;
    //        string token = data1.token;

    //        // token은 User의 id를 인코딩한 데이터임
    //        // Server는 token decode해서 유저 정보를 가져올 수 있음.
    //        // token은 유효시간이라는게 있어서 30분 후에 만료됨
    //        // 만료된 token은 다시 발급받아야됨
    //        // refresh token을 Server에서 발급해줄것임.
    //        // 만약에 프로세스가 복잡해졌어 => 리프레시 토큰 없이 토큰의 만료시간을 하루 정도로 늘릴 것임
    //        // token을 누군가에게 뺏기면 => 보안상 굉장히 위험
    //        PlayerPrefs.SetString("Bearer", token);

    //        // token은 WebRequest Header에 들어가야함.
    //        UserData user = await DataModule.WebRequest<UserData>("/api/v1/user", DataModule.NetworkType.GET, DataModule.DataType.BUFFER);



    //    }
    //}
}
