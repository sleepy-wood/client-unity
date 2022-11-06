using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_LandCustom : MonoBehaviour
{
    private GameObject itemWindow;
    private int selectCat = 0;
    private void Start()
    {
        itemWindow = transform.GetChild(1).gameObject;
#if UNITY_STANDALONE
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/Resources/LandCustom");
#elif UNITY_IOS || UNITY_ANDROID
        DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath + "/Resources/LandCustom");
#endif
        //Category Button 생성
        foreach(DirectoryInfo fi in di.GetDirectories())
        {
            GameObject objectButtonResource = Resources.Load<GameObject>("ObjectButton_");
            GameObject objectButton = Instantiate(objectButtonResource);
            objectButton.name = objectButton.name.Split('(')[0];
            objectButton.name += fi.Name;
            objectButton.transform.GetChild(0).GetComponent<Text>().text = fi.Name;
            objectButton.transform.parent = transform.GetChild(2);
        }

        //버튼 이벤트 등록
        for(int i = 0; i < transform.GetChild(2).transform.childCount; i++)
        {
            transform.GetChild(2).GetChild(i).GetComponent<Button>().onClick.AddListener(
                ()=>OnClickCategoryActive(transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<Text>().text, i));
            Debug.Log("===========================================");
            Debug.Log(transform.GetChild(2));
            Debug.Log(transform.GetChild(2).GetChild(i));
            Debug.Log(transform.GetChild(2).GetChild(i).GetChild(0));
            Debug.Log(transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<Text>().text);
            Debug.Log("===========================================");
        }

        //초기값 0번째 버튼 활성화
        OnClickCategoryActive(transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<Text>().text, 0);
    }

    public void OnClickCategoryActive(string Cat, int num)
    {
#if UNITY_STANDALONE
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/Resources/LandCustom/" + Cat);
#elif UNITY_IOS || UNITY_ANDROID
        DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath + "/Resources/LandCustom/" + Cat);
        string imagePath = Application.persistentDataPath + "/Resources/LandCustomImage/";
#endif
        selectCat = num;
        int cnt = 0;
        //썸네일 넣기
        foreach (FileInfo fi in di.GetFiles())
        {
            if (fi.Name.Split('.').Length > 2)
            {
                continue;
            }
            Sprite resource = Resources.Load<Sprite>("LandCustomImage/" + fi.Name.Split('.')[0]);
            itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().sprite =
                Instantiate(resource);
            Color color = itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().color;

            color.a = 1;
            itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().color = color;

            cnt++;
        }

        //나머지 버튼들은 비활성화
        for(int i = cnt; i < 15; i++)
        {
            itemWindow.transform.GetChild(cnt / 5).GetChild(cnt % 5).GetComponent<Image>().sprite = Instantiate(Resources.Load<Sprite>("LandCustomImage/ButtonBg"));
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

    }

    private bool isActiveCanvase = true;
    /// <summary>
    /// Custom UI 열고 닫기
    /// </summary>
    public void OnClickLandCustomActive()
    {
        if (!isActiveCanvase)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                if (i != 0)
                    transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (i != 0)
                    transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
