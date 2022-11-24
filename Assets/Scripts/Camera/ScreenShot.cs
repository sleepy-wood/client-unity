using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.Networking;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScreenShot : MonoBehaviour
{
    public Transform previewTreePos;
    public GameObject previewTree;
    public Image treeCaptureImg;

    /// <summary>
    /// 5일차에 사용자가 프로필 버튼 누르면 그 즉시 스크린샷해서 마이컬렉션 이미지 업데이트
    /// </summary>

    #region ScreenShot
    //    public void ScreenShotUpdate()
    //    {
    //        // 타겟 렌더 텍스쳐
    //        RenderTexture renderTexture = GetComponent<Camera>().targetTexture;
    //        // 화면 크기의 텍스쳐 생성
    //        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
    //        RenderTexture.active = renderTexture;
    //        // 캡쳐 영역 지정 뒤, 현재 화면 픽셀들을 텍스쳐 픽셀에 저장
    //        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    //        texture.Apply();

    //        // Save Image File
    //#if UNITY_STANDALONE
    //        string path = $"{Application.dataPath}/Resources/ScreenShot/TreeCapture.png";
    //#elif UNITY_IOS || UNITY_ANDROID
    //        string path = $"{Application.persistentDataPath}/TreeCapture.png";
    //#endif
    //        // 텍스쳐를 PNG 포맷의 byte[]로 변환한 뒤 파일 경로에 작성
    //        File.WriteAllBytes(path, texture.EncodeToPNG());
    //        byte[] data = File.ReadAllBytes(path);

    //        // Create the texture
    //        Texture2D screenshotTexture = new Texture2D(renderTexture.width, renderTexture.height);

    //        // Load the image
    //        screenshotTexture.LoadImage(data);

    //        // Create a sprite
    //        Sprite screenshotSprite = Sprite.Create(screenshotTexture, new Rect(0, 0, renderTexture.width, renderTexture.height), new Vector2(0.5f, 0.5f));

    //        // Set the sprite to the screenshotPreview
    //        treeCaptureImg.GetComponent<Image>().sprite = screenshotSprite;
    //        // Update TreeList Image 
    //        //Sprite s = Sprite.Create(texture, new Rect(0, 0, renderTexture.width, renderTexture.height), new Vector2(0.5f, 0.5f));
    //        //treeCaptureImg.sprite = s;
    //    }
    #endregion


    private void Update()
    {
        if (previewTree == null)
        {
            previewTree = GameObject.Find("previewTree");
            previewTree.layer = 11;  // Tree Layer
        }
    }


    bool succeded = true;
    string path;

    #region OnPosterRender
    // 카메라가 렌더링하는 부분만 캡쳐
    /// <summary>
    /// 카메라와 함께있는 컴포넌트에 작성했을 때 동작
    /// 프레임마다 해당 카메라의 렌더링이 끝냔 후에 호출됨 (카메라가 렌더링을 마친 모습만 저장)
    //private void OnPostRender()
    #endregion

    public void SaveCameraView()
    {
        // Screen Capture Code
        RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
        Camera cam = GetComponent<Camera>();
        cam.targetTexture = screenTexture;
        RenderTexture.active = screenTexture;
        cam.Render();

        // 화면 크기의 텍스쳐 생성
        Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
        // 캡쳐 영역 지정 뒤
        Rect area = new Rect(0, 0, Screen.width, Screen.height);
        // 현재 화면 픽셀들을 텍스쳐 픽셀에 저장
        renderedTexture.ReadPixels(area, 0, 0);
        RenderTexture.active = null;
        byte[] byteArray = renderedTexture.EncodeToPNG();

        try
        {
#if UNITY_STANDALONE
            path = $"{Application.dataPath}/ScreenShot/Image/TreeCapture_{GameManager.Instance.treeController.treeId}.png";
            if (Directory.Exists($"{Application.dataPath}/ScreenShot") == false)
            {
                Directory.CreateDirectory($"{Application.dataPath}/ScreenShot");
            }
            else
            {
                File.Delete(path);
            }
#elif UNITY_IOS || UNITY_ANDROID
                path = $"{Application.persistentDataPath}/ScreenShot/Image/TreeCapture_{GameManager.Instance.treeController.treeId}.png";
                if (Directory.Exists($"{Application.persistentDataPath}/ScreenShot") == false)
                {
                    Directory.CreateDirectory($"{Application.persistentDataPath}/ScreenShot");
                }
                else
                {
                    File.Delete(path);
                }
#endif
            // 스크린샷 저장
            File.WriteAllBytes(path, byteArray);
        }
        catch (Exception e)
        {
            succeded = false;
            Debug.LogWarning($"Screenshot Save Failed :  {path}");
            Debug.LogWarning(e);
        }

        if (succeded)
        {
            Debug.Log($"Screenshot Save Succeded :  {path}");
        }
    }

    string saveUrl;
    int fileId;
    public async void SaveTreeImg()
    {
        // Upload Tree File 
        saveUrl = "/api/v1/files/temp/upload";
        List<IMultipartFormSection> treeCaptures = new List<IMultipartFormSection>();
#if UNITY_STANDALONE
        string path = $"{Application.dataPath}/ScreenShot/Image/TreeCapture_{GameManager.Instance.treeController.treeId}.png";
#elif UNITY_IOS || UNITY_ANDROID
        string path = $"{Application.persistentDataPath}/ScreenShot/Image/TreeCapture_{GameManager.Instance.treeController.treeId}.png";
#endif
        #region WWW
        //byte[] bytes = File.ReadAllBytes(path);
        //WWWForm form = new WWWForm();
        //// image
        //form.AddBinaryData("files", bytes, "TreeCapture_{GameManager.Instance.treeController.treeId}.png", "image/png");

        //ResultPost<TreeFile> resultPost = await DataModule.WebRequestBuffer<ResultPost<TreeFile>>(
        //    saveUrl,
        //    DataModule.NetworkType.POST,
        //    DataModule.DataType.BUFFER,
        //    null,
        //    form
        //    );
        #endregion

        #region MultipartFormSection
        treeCaptures.Add(new MultipartFormFileSection("files", File.ReadAllBytes(path), $"TreeCapture_{GameManager.Instance.treeController.treeId}.png", "image/png"));

        ResultPost<List<TreeFile>> resultPost = await DataModule.WebRequestBuffer<ResultPost<List<TreeFile>>>(
            saveUrl,
            DataModule.NetworkType.POST,
            DataModule.DataType.BUFFER,
            null,
            treeCaptures
            );
        #endregion


        if (!resultPost.result)
        {
            Debug.LogError("Tree Screenshot Image 업로드 실패");
            return;
        }
        else
        {
            Debug.Log($"Tree Screenshot Image 업로드 성공");
            Debug.Log($"id = {resultPost.data[0].id}");
            fileId = resultPost.data[0].id;
            UploadImg();
        }
    }
    public async void UploadImg()
    {
        saveUrl = "/api/v1/trees/upload";
        TreeImgVideo treeImgVideo = new TreeImgVideo();
        treeImgVideo.treeId = GameManager.Instance.treeController.treeId;
        treeImgVideo.attachFileIds = new List<int>();
        treeImgVideo.attachFileIds.Add(fileId);

        string ImgVideoJsonData = JsonUtility.ToJson(treeImgVideo);
        Debug.Log(JsonUtility.ToJson(treeImgVideo, true));

        ResultPost<GetTreeImgVideo> resultPost2 = await DataModule.WebRequestBuffer<ResultPost<GetTreeImgVideo>>(
            saveUrl,
            DataModule.NetworkType.POST,
            DataModule.DataType.BUFFER,
            ImgVideoJsonData);

        if (!resultPost2.result) Debug.LogError("WebRequestError : NetworkType[Post]");
        else Debug.Log($"Tree Img/Video 업로드 성공");
        Debug.Log($"id = {resultPost2.data}");
    }
}
