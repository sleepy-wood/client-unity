using OpenCover.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Android;
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
    private async Task<string> AsyncGetRequest(string _url)
    {
        UnityWebRequest request = UnityWebRequest.Get(_url);
        request.SendWebRequest();

        while (!request.isDone)
        {
            await Task.Yield();
        }
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Get Request Error!: " + request.error);
            return request.error;
        }
        else
        {
            return request.downloadHandler.text;
        }
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
        string json = JsonUtility.ToJson(userData, true);

        string url = "http://URL";
        //userData2�� Web���� Save
        StartCoroutine(Upload(url, json));

    }

    //public async void test(string _url, string requestData)
    //{
    //    var response = await UnityWebRequest.Post(_url, requestData));


    //    using (UnityWebRequest request = UnityWebRequest.Post(_url, json))
    //    {
    //        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
    //        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
    //        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
    //        request.SetRequestHeader("Content-Type", "application/json");
    //        request.SetRequestHeader("Content-Type", "application/json");

    //        yield return request.SendWebRequest();

    //        if (request.error != null)
    //        {
    //            Debug.Log("Upload Error: " + request.error);
    //        }
    //    }
    //}

    //Upload Data
    IEnumerator Upload(string _url, string json)
    {
        //var client = new RestClient("https://team-buildup.shop/api/v1/users");
        //var request = new RestRequest(Method.POST);
        //request.AddHeader("Authorization", "Bearer REPLACE_BEARER_TOKEN");
        //request.AddHeader("content-type", "application/json");
        //request.AddParameter("application/json", "{}", ParameterType.RequestBody);
        //IRestResponse response = client.Execute(request);

        using (UnityWebRequest request = UnityWebRequest.Post(_url, json))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer REPLACE_BEARER_TOKEN");
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
