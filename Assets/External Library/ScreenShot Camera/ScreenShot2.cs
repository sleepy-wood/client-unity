using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScreenShot2 : MonoBehaviour
{
    public Transform previewTreePos;
    public GameObject previewTree;
    public Image treeCaptureImg;
    //[SerializeField] string fileName;
    /// <summary>
    /// 사용자가 프로필 버튼 누르면 그 즉시 스크린샷해서 트리 리스트 이미지 업데이트
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

    bool once = false;
    private void Start()
    {
        _willTakeScreenShot = true;
    }

    private void Update()
    {
        if (previewTree == null)
        {
            previewTree = GameObject.Find("previewTree");
            previewTree.layer = 11;  // Tree Layer
        }

        if (Input.GetMouseButton(0)&& !once)
        {
            _willTakeScreenShot = true;
        }
    }


    private bool _willTakeScreenShot = false;
    bool succeded = true;
    string path;
    // 카메라가 렌더링하는 부분만 캡쳐
    /// <summary>
    /// 카메라와 함께있는 컴포넌트에 작성했을 때 동작
    /// 프레임마다 해당 카메라의 렌더링이 끝냔 후에 호출됨 (카메라가 렌더링을 마친 모습만 저장)
    /// </summary>
    private void OnPostRender()
    {
        if (_willTakeScreenShot)
        {
            _willTakeScreenShot = false;

            // Screen Capture Code

            // 타겟 렌더 텍스쳐
            RenderTexture renderTexture = GetComponent<Camera>().targetTexture;
            // 화면 크기의 텍스쳐 생성
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, true, true);
            //RenderTexture.active = renderTexture;
            // 캡쳐 영역 지정 뒤
            Rect area = new Rect(0, 0, renderTexture.width, renderTexture.height);
            // 현재 화면 픽셀들을 텍스쳐 픽셀에 저장
            texture.ReadPixels(area, 0, 0);
            // Save Image File
            
            try
            {
#if UNITY_STANDALONE
                path = $"{Application.dataPath}/ScreenShot/TreeCapture.png";
                if (Directory.Exists($"{Application.dataPath}/ScreenShot") == false)
                {
                    Directory.CreateDirectory($"{Application.dataPath}/ScreenShot");
                }
                else
                {
                    File.Delete(path);
                }

#elif UNITY_IOS || UNITY_ANDROID
                path = $"{Application.persistentDataPath}/TreeCapture.png";
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
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            catch (Exception e)
            {
                succeded = false;
                Debug.LogWarning($"Screenshot Save Failed :  {path}");
                Debug.LogWarning(e);
            }


            // 텍스쳐 메모리에서 해제
            Destroy(texture);

            if (succeded)
            {
                Debug.Log($"Screenshot Save Succeded :  {path}");
            }
        }
    }

    
}
