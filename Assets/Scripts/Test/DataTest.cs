using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTest : MonoBehaviour
{
    class Temp<T>
    {
        public bool result;
        public T data;
    }

    class Token
    {
        public string token;
    }

    UserData userData = new UserData();

    private async void Start()
    {
        // IOS���� ������ ������
        //SleepDataStruct sleepDataStruct = new SleepDataStruct();
        //sleepDataStruct.WakeAt = "2022-10-20 10:10:10";
        //sleepDataStruct.SleepAt = "2022-10-20 18:10:10";

        // �������� ������ ������
        //userData.Id = 1;
        //userData.NickName = "test";
        //userData.UserAvatar = "Julia";
        //userData.SleepData = sleepDataStruct;

        Temp<Token> data = await DataModule.WebRequest<Temp<Token>>("/api/v1/auth/login/temp", DataModule.NetworkType.POST);

        Debug.Log(data.result);

        if (data.result)
        {
            Token data1 = data.data;
            string token = data1.token;

            // token�� User�� id�� ���ڵ��� ��������
            // Server�� token decode�ؼ� ���� ������ ������ �� ����.
            // token�� ��ȿ�ð��̶�°� �־ 30�� �Ŀ� �����
            // ����� token�� �ٽ� �߱޹޾ƾߵ�
            // refresh token�� Server���� �߱����ٰ���.
            // ���࿡ ���μ����� ���������� => �������� ��ū ���� ��ū�� ����ð��� �Ϸ� ������ �ø� ����
            // token�� ���������� ����� => ���Ȼ� ������ ����
            PlayerPrefs.SetString("Bearer", token);
            
            // token�� WebRequest Header�� ������.
            UserData user = await DataModule.WebRequest<UserData>("/api/v1/user", DataModule.NetworkType.GET);


            
        }
    }
}
