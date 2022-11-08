using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UI_LandCustom : MonoBehaviour
{
    [SerializeField] private Transform land;

    private GameObject itemWindow;
    private int selectCat = 0;
    private string selectCatName = "";
    private AssetBundle assetBundle;
    private AssetBundle assetBundleImg;
    private void Start()
    {
        Debug.Log("111111111");
#if UNITY_IOS ||UNITY_ANDROID
        assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath  + "/landcustombundle");
        assetBundleImg = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/landcustomimg");
#elif UNITY_STANDALONE
        assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath  + "/landcustombundle");
        assetBundleImg = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/landcustomimg");
#endif
        Debug.Log("222222222222");
        itemWindow = transform.GetChild(1).gameObject;

        //버튼 이벤트 등록
        for(int i = 0; i < transform.GetChild(2).childCount; i++)
        {
            int temp = i;
            transform.GetChild(2).GetChild(temp).GetComponent<Button>().onClick.AddListener(
                () => OnClickCategoryActive(transform.GetChild(2).GetChild(temp).GetChild(0).GetComponent<Text>().text, temp));
        }

        Debug.Log("3333333333333");
        //초기값 0번째 버튼 활성화
        OnClickCategoryActive(transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<Text>().text, 0);
    }

    public void OnClickCategoryActive(string Cat, int num)
    {
        Debug.Log("444444444444444");
#if UNITY_IOS|| UNITY_ANDROID
        string customPath = Application.streamingAssetsPath + "/LandCustom/" + Cat;
        string imagePath = Application.streamingAssetsPath + "/LandCustomImage/";
#elif UNITY_STANDALONE
        DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/LandCustom/" + Cat);
        string imagePath = Application.streamingAssetsPath + "/LandCustomImage/";
#endif
        Debug.Log("55555555555555");
        selectCatName = Cat;
        selectCat = num;
        int cnt = 0;
        //썸네일 넣기
        Debug.Log("666666666666");
        if(Directory.Exists(customPath))
        {

            Debug.Log("77777777777");
        }
        else
        {
            Debug.Log(customPath);
            Debug.Log("8888888888888");
        }
        string[] fileEntries = Directory.GetFiles(customPath, "*.prefab");

        for (int i = 0; i < fileEntries.Length; i++)
        {
            Sprite resource = assetBundleImg.LoadAsset<Sprite>(fileEntries[i].Split('\\')[1].Split('.')[0]);
            itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().sprite =
                Instantiate(resource);
            Color color = itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().color;

            color.a = 1;
            itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().color = color;

            cnt++;
        }
        //foreach (FileInfo fi in di.GetFiles())
        //{
        //    if (fi.Name.Split('.').Length > 2)
        //    {
        //        continue;
        //    }

        //    Sprite resource = assetBundleImg.LoadAsset<Sprite>(fi.Name.Split('.')[0]);
        //    Debug.Log(resource);
        //    itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().sprite =
        //        Instantiate(resource);
        //    Color color = itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().color;

        //    color.a = 1;
        //    itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().color = color;

        //    cnt++;
        //}
        Debug.Log("7777777777777");

        //나머지 버튼들은 비활성화
        for (int i = cnt; i < 15; i++)
        {
            itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().sprite = Instantiate(assetBundleImg.LoadAsset<Sprite>("ButtonBg"));
            Color color = itemWindow.transform.GetChild(i / 5).GetChild(i % 5).GetComponent<Image>().color;
            color.a = 0.3f;
            itemWindow.transform.GetChild(i / 5).GetChild(i % 5).GetComponent<Image>().color = color;
        }
    }

    /// <summary>
    /// List 목록 중에 특정 오브젝트를 클릭할 시
    /// </summary>
    /// <param name="i"></param>
    public void OnClickCreateObject(int i)
    {
        Debug.Log("888888888888888");
#if UNITY_IOS || UNITY_ANDROID
        DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/LandCustom/" + selectCatName);
        string customPath = Application.streamingAssetsPath + "/LandCustom/" + selectCatName;
#elif UNITY_STANDALONE
        DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/LandCustom/" + selectCatName);
#endif
        //string imagePath = Application.streamingAssetsPath + "/LandCustomImage/";

        Debug.Log("9999999999999");
        int cnt = 0;

        string[] fileEntries = Directory.GetFiles(customPath, "*.prefab");

        //썸네일 넣기
                
        foreach(string fileName in fileEntries)
        {
            if (cnt == i)
            {
                GameObject resource = assetBundle.LoadAsset<GameObject>(fileName.Split('\\')[1].Split('.')[0]);
                //GameObject resource = Resources.Load<GameObject>("LandCustom/" + selectCatName + "/" + fileName.Split('\\')[1].Split('.')[0]);
                Debug.Log("File = " + fileName.Split('\\')[1].Split('.')[0]);
                GameObject prefab = Instantiate(resource);
                prefab.name = prefab.name.Split('(')[0];
                prefab.transform.position = new Vector3(0, 0.5f, 0);
                //landDecorations라는 가방에 담기 => 존재하지 않으면 만들자
                for (int j = 0; j < land.childCount; j++)
                {
                    if (land.GetChild(j).gameObject.name == "landDecorations")
                    {
                        prefab.transform.parent = land.GetChild(j);
                        return;
                    }
                }
                GameObject landDecorations = new GameObject("landDecorations");
                landDecorations.transform.parent = land;
                landDecorations.transform.position = Vector3.zero;
                prefab.transform.parent = landDecorations.transform;
                return;
            }

            cnt++;
        }

        //foreach (FileInfo fi in di.GetFiles())
        //{
        //    if (fi.Name.Split('.').Length > 2)
        //    {
        //        continue;
        //    }
        //    if(cnt == i)
        //    {
        //        GameObject resource = assetBundle.LoadAsset<GameObject>(fi.Name.Split('.')[0]);
        //        GameObject prefab = Instantiate(resource);
        //        prefab.name = prefab.name.Split('(')[0];
        //        prefab.transform.position = new Vector3(0, 0.5f, 0);
        //        //landDecorations라는 가방에 담기 => 존재하지 않으면 만들자
        //        for(int j = 0; j < land.childCount; j++)
        //        {
        //            if(land.GetChild(j).gameObject.name == "landDecorations")
        //            {
        //                prefab.transform.parent = land.GetChild(j);
        //                return;
        //            }
        //        }
        //        GameObject landDecorations = new GameObject("landDecorations");
        //        landDecorations.transform.parent = land;
        //        landDecorations.transform.position = Vector3.zero;
        //        prefab.transform.parent = landDecorations.transform;
        //        return;
        //    }

        //    cnt++;
        //}
        Debug.Log(selectCat);
        Debug.Log(i);
    }

    private bool isActiveCanvase = true;
    /// <summary>
    /// Custom UI 열고 닫기
    /// </summary>
    public void OnClickLandCustomActive()
    {
        if (!isActiveCanvase)
        {
            isActiveCanvase = true;
            for(int i = 0; i < transform.childCount; i++)
            {
                if (i != 0)
                    transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            isActiveCanvase = false;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (i != 0)
                    transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
