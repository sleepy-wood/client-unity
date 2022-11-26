using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UploadTreeData : MonoBehaviour
{
    // FileIds
    public List<int> fileIds = new List<int>();

    /// <summary>
    /// Tree Id에 맞춰 나무 비디오/이미지 아이디 업로드
    /// </summary>
    public async void FileIDUpload()
    {
        string saveUrl = "/api/v1/trees/upload";
        TreeFileID treeFileID = new TreeFileID();
        treeFileID.treeId = GameManager.Instance.treeController.treeId;
        treeFileID.attachFileIds = fileIds;

        string ImgVideoJsonData = JsonUtility.ToJson(treeFileID);
        Debug.Log(JsonUtility.ToJson(treeFileID, true));

        ResultPost<GetTreeFileID> resultPost2 = await DataModule.WebRequestBuffer<ResultPost<GetTreeFileID>>(
            saveUrl,
            DataModule.NetworkType.POST,
            DataModule.DataType.BUFFER,
            ImgVideoJsonData);

        if (!resultPost2.result) Debug.LogError("WebRequestError : NetworkType[Post]");
        else Debug.Log($"Tree Img/Video 파일 ID 업로드 성공");
        Debug.Log($"id = {resultPost2.data}");
    }
}
