using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

//저장소 역할 
//구조체는 그저 틀일 뿐,,,
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
    /// 저장소에 있는 Data 보내기 
    /// </summary>
    /// <returns></returns>
    public UserData RecieveUserData()
    {
        return userData;
    }

    /// <summary>
    /// 새로고침 눌렀을 경우 Get Data 해서 return.
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
        //url을 통해 load 받고,(맞나)
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
    //UserData형태로 return
    UserData UserDataReturn(string json)
    {
        UserData userdataTemp = JsonUtility.FromJson<UserData>(json);
        //userData2 에 그 정보를 끼워 넣는다.
        userData = userdataTemp;
        return userData;
    }

    //Send
    /// <summary>
    /// 변경된 Data를 보내기
    /// </summary>
    /// <param name="user"></param>
    public void SendUserData(UserData user)
    {
        userData = user;
        string json = JsonUtility.ToJson(userData);

        string url = "http://URL";
        //userData2를 Web으로 Save
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
