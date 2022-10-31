using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


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
    public void ScreenShotUpdate()
    {
        
        RenderTexture renderTexture = GetComponent<Camera>().targetTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        // Save Image File
        File.WriteAllBytes($"{Application.dataPath}/Resources/ScreenShot/TreeCapture.png", texture.EncodeToPNG());
        // Update TreeList Image 
        Sprite s = Sprite.Create(texture, new Rect(0, 0, renderTexture.width, renderTexture.height), new Vector2(0.5f, 0.5f));
        treeCaptureImg.sprite = s;
    }



    private void Update()
    {
        if (previewTree == null)
        {
            previewTree = GameObject.Find("previewTree");
            previewTree.layer = 11;  // Tree Layer
        }
    }
}

//[CustomEditor(typeof(CustomUtils.ScreenShot))]
//public class ScreenShotEditor : Editor 
//{
//    CustomUtils.ScreenShot screenShot;
//	void OnEnable() => screenShot = target as CustomUtils.ScreenShot;

//	public override void OnInspectorGUI()
//	{
//        base.OnInspectorGUI();
//        if (GUILayout.Button("ScreenShot"))
//        {
//            screenShot.ScreenShotClick();
//            EditorApplication.ExecuteMenuItem("Assets/Refresh");
//        } 
//	}
//}
//#endif

//namespace CustomUtils
//{
//    public class ScreenShot : MonoBehaviour
//    {
//        [SerializeField] string screenShotName;

//        public void ScreenShotClick()
//        {
//            RenderTexture renderTexture = GetComponent<Camera>().targetTexture;
//            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
//            RenderTexture.active = renderTexture;
//            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
//            texture.Apply();

//            File.WriteAllBytes($"{Application.dataPath}/{screenShotName}.png", texture.EncodeToPNG());
//        }
//    }
//}