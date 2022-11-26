using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class VideoUploadHelper : MonoBehaviour
{   
    public bool IsCompleteMakingVideo { get; set; }
    string zipPath;
    private void Update()
    {
        if (IsCompleteMakingVideo)
        {
            IsCompleteMakingVideo = false;
            UploadZipFile();
        }
    }
    /// <summary>
    /// 이미지 압축 파일 웹에 업로드
    /// </summary>
    public async void UploadZipFile()
    {

#if UNITY_STANDALONE
        zipPath = $"{Application.dataPath}/Zipfiles/TreeImgZip_{GameManager.Instance.treeController.treeId}.zip";
#elif UNITY_IOS || UNITY_ANDROID
        zipPath = $"{Application.persistentDataPath}/Zipfiles/TreeImgZip_{GameManager.Instance.treeController.treeId}.zip";
#endif

        string saveUrl = "/api/v1/files/temp/image-to-video";
        List<IMultipartFormSection> videoCaptures = new List<IMultipartFormSection>();

        videoCaptures.Add(new MultipartFormFileSection("files", File.ReadAllBytes(zipPath), $"TreeImgZip_{GameManager.Instance.treeController.treeId}.zip", "application/zip"));

        ResultPost<GetVideoFromZip> resultPost = await DataModule.WebRequestBuffer<ResultPost<GetVideoFromZip>>(
           saveUrl,
           DataModule.NetworkType.POST,
           DataModule.DataType.BUFFER,
           null,
           videoCaptures);

        if (!resultPost.result)
        {
            Debug.Log("Tree Video Zip File Upload : Fail");
            return;
        }
        else
        {
            Debug.Log("Tree Video Zip File Upload : Success");
            Debug.Log($"Video Zip FileId = {resultPost.data.id}");
            // Video Zip File Id 저장
            GameManager.Instance.treeController.GetComponent<UploadTreeData>().fileIds.Add(resultPost.data.id);
            // 나무 비디오/이미지 파일 ID 웹 업로드
            GameManager.Instance.treeController.GetComponent<UploadTreeData>().FileIDUpload();
        }
    }
}
