using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.Rendering;


class ResultTemp<T>
{
    public bool result;
    public T data;
}

class Token
{
    public string token;
}

public class DataModule
{
    private const string DOMAIN = "https://team-buildup.shop";
    public static string REPLACE_BEARER_TOKEN = "";

    public enum NetworkType
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    protected static double timeout = 5;

    /// <summary>
    /// Web에 Data를 Request할 수 있다.
    /// </summary>
    /// <typeparam name="T">Json 형식</typeparam>
    /// <param name="_url">Json 올릴 url</param>
    /// <param name="networkType">어떻게 Request 할 것인가</param>
    /// <param name="data">보낼 데이터</param>
    /// <returns></returns>
    public static async UniTask<T> WebRequest<T>(string _url, NetworkType networkType, string data = null)
    {
        //네트워크 체킹
        await CheckNetwork();
        //API URL 생성
        string requestURL = DOMAIN + _url;
        //Timeout 설정
        var cts = new CancellationTokenSource();
        cts.CancelAfterSlim(TimeSpan.FromSeconds(timeout));

        //웹 요청 생성(Get,Post,Delete,Update)
        UnityWebRequest request = new UnityWebRequest(requestURL, networkType.ToString());
        
        //Body 정보 입력
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        if (data != null)
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(data);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        }

        //Header 정보 입력
        if (REPLACE_BEARER_TOKEN == "" && PlayerPrefs.GetString("Bearer") != "")
        {
            REPLACE_BEARER_TOKEN = PlayerPrefs.GetString("Bearer");
        }
       
        SetHeaders(request, "Authorization", "Bearer " + REPLACE_BEARER_TOKEN);
        SetHeaders(request, "Content-Type", "application/json");

        try
        {
            var res = await request.SendWebRequest().WithCancellation(cts.Token);
            T result = JsonUtility.FromJson<T>(res.downloadHandler.text);
            return result;
        }
        catch (OperationCanceledException ex)
        {
            if (ex.CancellationToken == cts.Token)
            {
                Debug.Log("Timeout");
                //TODO: 네트워크 재시도 팝업 호출.

                //재시도
                return await WebRequest<T>(_url, networkType, data);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return default;
        }
        return default;
    }

    private static async UniTask CheckNetwork()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("The netwok is not connected");
            await UniTask.WaitUntil(() => Application.internetReachability != NetworkReachability.NotReachable);
            Debug.Log("The network is connected");
        }
    }
    private static void SetHeaders(UnityWebRequest request, string value1, string value2)
    {
        request.SetRequestHeader(value1, value2);
    }
}
