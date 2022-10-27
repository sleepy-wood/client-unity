using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Unity.VisualScripting.FullSerializer;

public static class FileManager
{
    /// <summary>
    /// fileName을 받고 File을 Load 한 뒤 Json을 풀어서 return
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T LoadDataFile<T>(string fileName)
    {
        string filePath = fileName;

       // TextAsset jsonData = Resources.Load<TextAsset>("Data/" + filePath);
        string s = File.ReadAllText(Application.dataPath + "/Resources/Data/" + filePath + ".txt");
        T data = JsonUtility.FromJson<T>(s);

        return data;
    }
    /// <summary>
    /// Data를 받아서 Json으로 변환 후 File로 저장
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <param name="data"></param>
    public static string SaveDataFile<T>(string fileName, T data)
    {
        string jsonData = JsonUtility.ToJson(data, true);

#if UNITY_STANDALONE
        //Json을 txt 파일로 레지스트리에 저장
        string filePath = Application.dataPath + "/Resources/Data";
#elif UNITY_IOS || UNITY_ANDROID
        string filePath = Application.persistentDataPath + "/Resources/Data";

#endif
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        File.WriteAllText(filePath + "/" + fileName + ".txt", jsonData);
        return jsonData;
    }
}
