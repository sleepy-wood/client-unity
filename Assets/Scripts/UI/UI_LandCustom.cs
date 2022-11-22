using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UI_LandCustom : MonoBehaviourPun
{
    [SerializeField] private Transform land;
    [SerializeField] private List<GameObject> objects = new List<GameObject>();
    [SerializeField] private GameObject button;
    public GameObject menuBar;
    public GameObject menuBar2;

    private GameObject itemWindow;
    private int selectCat = 0;
    private string selectCatName = "";
    AssetBundle myLoadedAssetBundle;
    private void Start()
    {
        if (PhotonNetwork.PlayerList.Length != 1)
        {
            button.SetActive(false);
        }

        itemWindow = transform.GetChild(2).gameObject;

        //버튼 이벤트 등록
        for (int i = 0; i < transform.GetChild(1).childCount; i++)
        {
            int temp = i;
            transform.GetChild(1).GetChild(temp).GetComponent<Button>().onClick.AddListener(
                () => OnClickCategoryActive(transform.GetChild(1).GetChild(temp).GetChild(0).gameObject.name, temp));
        }

        //초기값 0번째 버튼 활성화
        //OnClickCategoryActive(transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text, 0);

        //비활성화
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].SetActive(false);
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (PhotonNetwork.PlayerList.Length != 1)
        {
            button.SetActive(false);
        }
        else
        {
            button.SetActive(true);
        }
    }
    public void OnClickCategoryActive(string Cat, int num)
    {
        string customPath = Application.streamingAssetsPath + "/LandCustom/" + Cat;
        string imagePath = Application.streamingAssetsPath + "/LandCustomImage/";
        selectCatName = Cat;
        int cnt = 0;
        //썸네일 넣기
        string[] fileEntries = Directory.GetFiles(customPath, "*.prefab");

        for (int i = 0; i < fileEntries.Length; i++)
        {
            Debug.Log(fileEntries[i]);
#if UNITY_STANDALONE
            Sprite resource = DataTemporary.assetBundleImg.LoadAsset<Sprite>(fileEntries[i].Split("/LandCustom/" + Cat + "\\")[1].Split('.')[0]);
#elif UNITY_IOS
            Sprite resource = DataTemporary.assetBundleImg.LoadAsset<Sprite>(fileEntries[i].Split("/LandCustom/" + Cat + "/")[1].Split('.')[0]);
#endif
            itemWindow.transform.GetChild(cnt / 4).GetChild(cnt % 4).GetComponent<Image>().sprite =
                Instantiate(resource);
            Color color = itemWindow.transform.GetChild(cnt / 4).GetChild(cnt % 4).GetComponent<Image>().color;

            color.a = 1;
            itemWindow.transform.GetChild(cnt / 4).GetChild(cnt % 4).GetComponent<Image>().color = color;

            cnt++;
        }
        //마켓에서 샀을경우
        int h = 0;
        for(int i = 0; i < 8; i++)
        {
            string category = ((Category)i).ToString();
            category = category.Remove(0, 1);
            if (Cat.Contains(category))
            {
                h = i;
                selectCat = h;
                break;
            }
        }
#if UNITY_STANDALONE
        string path = Application.dataPath + "/MarketImg/" + (Category)h;
#elif UNITY_IOS || UNITY_ANDROID
        string path = Application.persistentDataPath + "/MarketImg/" + (Category)h;
#endif

        string[] fileMarket = Directory.GetFiles(path, "*.png");
        Debug.Log("1");
        for(int i = 0; i < fileMarket.Length; i++)
        {
            Debug.Log("cnt = " + cnt);

            if (cnt >= 8)
                break;
            Debug.Log(fileMarket[i]);

#if UNITY_STANDALONE
            byte[] byteTexture = File.ReadAllBytes(path + "/" + fileMarket[i].Split($"{(Category)h}\\")[1]);
            //Debug.Log("path = " + path + "/" + fileMarket[i].Split($"{(Category)h}\\")[1]);
#elif UNITY_IOS
            Debug.Log("path = " + path + "/" + fileMarket[i].Split($"{(Category)h}/")[1]);

            byte[] byteTexture = File.ReadAllBytes(fileMarket[i]);
#endif
            Debug.Log("gdfaf");
            if (byteTexture.Length > 0)
            {
                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(byteTexture);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                itemWindow.transform.GetChild(cnt / 4).GetChild(cnt % 4).GetComponent<Image>().sprite = sprite;
            }
            Debug.Log("ffff");
            Color color = itemWindow.transform.GetChild(cnt / 4).GetChild(cnt % 4).GetComponent<Image>().color;
            color.a = 1;
            itemWindow.transform.GetChild(cnt / 4).GetChild(cnt % 4).GetComponent<Image>().color = color;

            cnt++;

        }
        Debug.Log("2");
        //나머지 버튼들은 비활성화
        for (int i = cnt; i < 8; i++)
        {
            Debug.Log("333");
            itemWindow.transform.GetChild(cnt / 4).GetChild(cnt % 4).GetComponent<Image>().sprite = Instantiate(DataTemporary.assetBundleImg.LoadAsset<Sprite>("ButtonBg"));
            Color color = itemWindow.transform.GetChild(i / 4).GetChild(i % 4).GetComponent<Image>().color;
            color.a = 0.3f;
            itemWindow.transform.GetChild(i / 4).GetChild(i % 4).GetComponent<Image>().color = color;
        }
        #region Legacy
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

        #endregion
    }

    /// <summary>
    /// List 목록 중에 특정 오브젝트를 클릭할 시
    /// </summary>
    /// <param name="i"></param>
    public void OnClickCreateObject(int i)
    {
        DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/LandCustom/" + selectCatName);
        string customPath = Application.streamingAssetsPath + "/LandCustom/" + selectCatName;
        //string imagePath = Application.streamingAssetsPath + "/LandCustomImage/";

        int cnt = 0;

        string[] fileEntries = Directory.GetFiles(customPath, "*.prefab");

        //썸네일 넣기
        foreach(string fileName in fileEntries)
        {
            if (cnt == i)
            {
                GameObject resource = DataTemporary.assetBundleCustom.LoadAsset<GameObject>(fileName.Split("/LandCustom/" + selectCatName + "/")[1].Split('.')[0]);
                //GameObject resource = Resources.Load<GameObject>("LandCustom/" + selectCatName + "/" + fileName.Split('\\')[1].Split('.')[0]);
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

        //마켓에서 샀을경우 

#if UNITY_STANDALONE
        string assetBundleDirectory = Application.dataPath + "/MarketBundle/" + (Category)selectCat;
#elif UNITY_IOS
        string assetBundleDirectory = Application.persistentDataPath + "/MarketBundle/" + (Category)selectCat;
#endif
        string[] bundles = Directory.GetFiles(assetBundleDirectory);

        //썸네일 넣기
        foreach (string fileName in bundles)
        {
            //Debug.Log("FileName = " + fileName);
#if UNITY_STANDALONE
            if (fileName.Split("/MarketBundle/" + (Category)selectCat + "\\")[1].Contains("meta"))
                continue;
            //Debug.Log(Path.Combine(assetBundleDirectory + "/", fileName.Split("/MarketBundle/" + (Category)selectCat + "\\")[1].Split('.')[0]));
#elif UNITY_IOS
            if (fileName.Split("/MarketBundle/" + (Category)selectCat + "/")[1].Contains("meta"))
                continue;
            //Debug.Log(Path.Combine(assetBundleDirectory + "/", fileName.Split("/MarketBundle/" + (Category)selectCat + "/")[1].Split('.')[0]));
#endif

            if (cnt == i)
            {
                //Debug.Log(Path.Combine(assetBundleDirectory + "/", fileName.Split("/MarketBundle/" + (Category)selectCat + "/")[1].Split('.')[0]));
#if UNITY_STANDALONE
                myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(assetBundleDirectory + "/", fileName.Split("/MarketBundle/" + (Category)selectCat + "\\")[1].Split('.')[0]));
                GameObject resource = myLoadedAssetBundle.LoadAsset<GameObject>(fileName.Split("/MarketBundle/" + (Category)selectCat + "\\")[1]);
                //Debug.Log(fileName.Split("/MarketBundle/" + (Category)selectCat + "\\")[1]);
#elif UNITY_IOS
                var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(assetBundleDirectory + "/", fileName.Split("/MarketBundle/" + (Category)selectCat + "/")[1].Split('.')[0]));
                GameObject resource = myLoadedAssetBundle.LoadAsset<GameObject>(fileName.Split("/MarketBundle/" + (Category)selectCat + "/")[1]);
#endif
                //GameObject resource = Resources.Load<GameObject>("LandCustom/" + selectCatName + "/" + fileName.Split('\\')[1].Split('.')[0]);
                GameObject prefab = Instantiate(resource);
                Debug.Log(prefab);

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
                myLoadedAssetBundle.Unload(true);
                return;
            }

            cnt++;
        }
#region Legacy
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
#endregion
    }

    private bool isActiveCanvase = false;
    /// <summary>
    /// Custom UI 열고 닫기
    /// </summary>
    public void OnClickLandCustomActive()
    {
        if (!isActiveCanvase)
        {
            menuBar.SetActive(false);
            menuBar2.SetActive(false);
            isActiveCanvase = true;
            GameManager.Instance.User.GetComponent<UserInput>().InputControl = true;
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].SetActive(true);
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            menuBar.SetActive(true);
            menuBar2.SetActive(true);
            //SkyLandManager.Instance.SaveData();
            GameManager.Instance.User.GetComponent<UserInput>().InputControl = false;
            isActiveCanvase = false;
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].SetActive(false);
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    ///// <summary>
    ///// 카테고리 패널 선택 시 색깔 변경
    ///// </summary>
    ///// <param name="idx"></param>
    //public Transform objPanel;
    //public void OnClickPanel(int idx)
    //{
    //    for (int i = 0; i < 5; i++)
    //    {
    //        if (idx == i)
    //        {
    //            // Navy 이미지 Alpha값 조정
    //            objPanel.transform.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);  
    //            // Text 하얀색
    //            objPanel.transform.GetChild(i).GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1, 1);
    //        }
    //        else
    //        {
    //            // Navy 이미지 Alpha값 조정
    //            objPanel.transform.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 0);
    //            // Text 검은색
    //            objPanel.transform.GetChild(i).GetChild(0).GetComponent<Text>().color = new Color(0, 0, 0, 1);
    //        }
    //    }
    //}
}
