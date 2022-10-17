using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

//����� ���� 
//����ü�� ���� Ʋ�� ��,,,
public class DataModule:MonoBehaviour
{
    public enum NET_RESULT
    {
        NET_SUCCESS,
        NET_FAIL,
    }
    #region UserData
    UserData userData = new UserData();
    
    /// <summary>
    /// ����ҿ� �ִ� Data ������ 
    /// </summary>
    /// <returns></returns>
    public UserData RecieveUserData()
    {
        return userData;
    }

    /// <summary>
    /// ���ΰ�ħ ������ ��� Get Data �ؼ� return.
    /// </summary>
    /// /api/v1/users/{:id}
    /// const strING domain = isDev? "https://team-buildup.shop" : "htpps:"
    public void RefreshUserData()
    {
        string url = "http://URL";

        StartCoroutine(DownLoadGet(url));
    }
    //DownLoad Data
    IEnumerator DownLoadGet(string _url)
    {
        //url�� ���� load �ް�,(�³�)
        UnityWebRequest request = UnityWebRequest.Get(_url);

        yield return request.SendWebRequest();

        if (request.error == null)
        {
            //Debug.Log(request.downloadHandler.text);
            UserDataReturn(request.downloadHandler.text);
        }
        else
        {
            Debug.Log("DownLoad Error: " + request.error);
        }
    }
    //UserData���·� return
    UserData UserDataReturn(string json)
    {
        UserData userdataTemp = JsonUtility.FromJson<UserData>(json);
        //userData2 �� �� ������ ���� �ִ´�.
        userData = userdataTemp;
        return userData;
    }

    //Send
    /// <summary>
    /// ����� Data�� ������
    /// </summary>
    /// <param name="user"></param>
    public void SendUserData(UserData user)
    {
        userData = user;
        string json = JsonUtility.ToJson(userData);

        string url = "http://URL";
        //userData2�� Web���� Save
        StartCoroutine(Upload(url, json));

    }
    //Upload Data
    IEnumerator Upload(string _url, string json)
    {
        using(UnityWebRequest request = UnityWebRequest.Post(_url, json))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if(request.error != null)
            {
                Debug.Log("Upload Error: " + request.error);
            }
        }
    }
    #endregion

}
