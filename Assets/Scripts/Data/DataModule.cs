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
    /// Web�� Data�� Request�� �� �ִ�.
    /// </summary>
    /// <typeparam name="T">Json ����</typeparam>
    /// <param name="_url">Json �ø� url</param>
    /// <param name="networkType">��� Request �� ���ΰ�</param>
    /// <param name="data">���� ������</param>
    /// <returns></returns>
    public static async UniTask<T> WebRequest<T>(string _url, NetworkType networkType, string data = null)
    {
        //��Ʈ��ũ üŷ
        await CheckNetwork();
        //API URL ����
        string requestURL = DOMAIN + _url;
        //Timeout ����
        var cts = new CancellationTokenSource();
        cts.CancelAfterSlim(TimeSpan.FromSeconds(timeout));

        //�� ��û ����(Get,Post,Delete,Update)
        UnityWebRequest request = new UnityWebRequest(requestURL, networkType.ToString());
        
        //Body ���� �Է�
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        if (data != null)
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(data);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        }

        //Header ���� �Է�
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
                //TODO: ��Ʈ��ũ ��õ� �˾� ȣ��.

                //��õ�
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
