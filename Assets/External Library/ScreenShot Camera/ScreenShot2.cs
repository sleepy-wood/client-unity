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
        //StartCoroutine(TakeScreenshot());
        //RenderTexture renderTexture = GetComponent<Camera>().targetTexture;
        //RenderTexture.active = renderTexture;
        //Texture2D texture;
        //texture = ScreenCapture.CaptureScreenshotAsTexture();
        //texture.width = renderTexture.width;
        //texture.height = renderTexture.height;
        //texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        //texture.Apply();
        //string path = $"{Application.dataPath}/Resources/ScreenShot/TreeCapture.png";
        //File.WriteAllBytes(path, texture.EncodeToPNG());
        ////Texture2D texture2 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Resources/ScreenShot/TreeCapture.png", typeof(Texture2D));
        //Texture2D texture2 = Resources.Load<Texture2D>("ScreenShot/TreeCapture");
        //SetTexture(texture2, true);
        //Sprite s = Sprite.Create(texture, new Rect(0, 0, renderTexture.width, renderTexture.height), new Vector2(0.5f, 0.5f));
        //treeCaptureImg.sprite = s;
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

    //public IEnumerator TakeScreenshot()
    //{
    //    //yield return new WaitForEndOfFrame();

    //    //string path = Application.persistentDataPath + "Screenshots/"
    //    //        + "_" + 1 + "_" + Screen.width + "X" + Screen.height + "" + ".png";
    //    string path = $"{Application.dataPath}/Resources/ScreenShot/TreeCapture.png";

    //    Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
    //    //Get Image from screen
    //    screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
    //    screenImage.Apply();
    //    //Convert to png
    //    byte[] imageBytes = screenImage.EncodeToPNG();

    //    //Save image to file
    //    File.WriteAllBytes(path, imageBytes);

    //    yield return new WaitForEndOfFrame();

    //    byte[] data = File.ReadAllBytes(path);

    //    // Create the texture
    //    Texture2D screenshotTexture = new Texture2D(Screen.width, Screen.height);

    //    // Load the image
    //    screenshotTexture.LoadImage(data);

    //    // Create a sprite
    //    Sprite screenshotSprite = Sprite.Create(screenshotTexture, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));

    //    // Set the sprite to the screenshotPreview
    //    treeCaptureImg.GetComponent<Image>().sprite = screenshotSprite;


    //}
    public void SetTexture(Texture2D texture, bool isReadable)
    {
        if (!texture)
        {
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(texture);

        TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if(textureImporter!= null)
        {
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.isReadable = isReadable;
            textureImporter.sRGBTexture = false;
            textureImporter.alphaIsTransparency = true;

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }

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