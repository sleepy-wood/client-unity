//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class TreeScreenShot : MonoBehaviour
//{
//    public Camera cam;

//    private void Update()
//    {
//        if (Input.GetKey(KeyCode.Space))
//        {
//            Capture();
//        }
//    }


//    void Capture()
//    {
//#if UNITY_STANDALONE
//        string path = Application.dataPath + "/Screenshot/capture.png";
//#elif UNITY_IOS
//        string path = Application.persistentDataPath + "/Screenshot/capture.png";
//#endif
//        StartCoroutine(CoCapture(path));
//    }

//    IEnumerator CoCapture(string path)
//    {
//        if (path == null)
//        {
//            yield break;
//        }

//        // ReadPixels 하기 위해 쉬어줌
//        yield return new WaitForEndOfFrame();

//        // Rect = 2차원 사각형 영역
//        Rect rect = new Rect(0f, 0f, Screen.width, Screen.height);
//        Texture2D texture = Capture(Camera.main, rect);

//        byte[] bytes = texture.EncodeToPNG();
//        System.IO.File.WriteAllBytes(path, bytes);
//    }

//    Texture2D Capture(Camera camera, Rect pRect)
//    {
//        Texture2D capture;
//        CameraClearFlags preClearFlags = camera.clearFlags;
//        Color preBackgroundColor = camera.backgroundColor;

//        camera.clearFlags = CameraClearFlags.SolidColor;
//        camera.backgroundColor = Color.black;
//        camera.Render();
//        Texture2D blackBackgroundCapture = CaptureView(pRect);
//    }

//}
