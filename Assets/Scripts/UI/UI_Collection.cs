using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Collection : MonoBehaviour
{
    public List<Sprite> grades = new List<Sprite>();
    
    private RectTransform content;
    private int selectNum = 0;
    private GameObject user;
    private UserInput userInput;
    private int maxNum;
    private List<float> posXList = new List<float>();
    private async void Awake()
    {
        content = transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<RectTransform>();
        for (int i = 0; i < DataTemporary.arrayCollectionDatas.collectionLists.Count; i++)
        {
            GameObject profile_resource = Resources.Load<GameObject>("Profile");
            GameObject profile_Prefab = Instantiate(profile_resource, content);
            profile_Prefab.name = profile_Prefab.name.Split('(')[0] + "_" + i;
            CollectionData collectionData = DataTemporary.arrayCollectionDatas.collectionLists[i];
            if(collectionData.rarity>90 && collectionData.rarity <= 100)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[0];
            }
            else if(collectionData.rarity > 80 && collectionData.rarity <= 90)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[1];
            }
            else if(collectionData.rarity > 70 && collectionData.rarity <= 80)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[2];
            }
            else if(collectionData.rarity > 60 && collectionData.rarity <= 70)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[3];
            }
            else if(collectionData.rarity > 50 && collectionData.rarity <= 60)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[4];
            }
            for(int j = 0; j < collectionData.treeAttachments.Count; j++)
            {
                if (collectionData.treeAttachments[j].mimeType.Contains("image"))
                {
                    Texture2D texture = await DataModule.WebrequestTextureGet(collectionData.treeAttachments[j].path, DataModule.NetworkType.GET);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    profile_Prefab.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
                    break;
                }
            }

        }

        maxNum = content.childCount - 1;
        for(int i = 0; i < content.childCount; i++)
        {
            if (i == 0)
                posXList.Add(0);
            else if (i == 1)
                posXList.Add(-812.5f);
            else
                posXList.Add(-812.5f + -856.9f * (i - 1));
        }
    }
    private void OnEnable()
    {
        if (!user)
        {
            user = GameManager.Instance.User;
        }
        user.GetComponent<UserInteract>().moveControl = true;
        userInput = user.GetComponent<UserInput>();
    }
    private void OnDisable()
    {
        if (!user)
        {
            user = GameManager.Instance.User;
        }
        user.GetComponent<UserInteract>().moveControl = false;
    }
    float Draging = 0;
    bool isChange = false;
    bool isOnce = false;
    private void Update()
    {
        if (!user)
        {
            user = GameManager.Instance.User;
            user.GetComponent<UserInteract>().moveControl = true;
            userInput = user.GetComponent<UserInput>();
        }
#if UNITY_STANDALONE
        if (Input.GetMouseButtonUp(1))
        {
            isChange = true;
            isOnce = false;
        }
        else if(Input.GetMouseButton(1))
        {
            if (userInput.DragX != 0)
                Draging = userInput.DragX;
            isChange = false;
        }

#elif UNITY_IOS || UNITY_ANDROID

        if (Input.touchCount == 1)
        {
            Touch touchFirstFinger = Input.GetTouch(0);
            Vector2 touchMoveBeforePos = touchFirstFinger.position - touchFirstFinger.deltaPosition;
            if (Input.touches[0].phase == TouchPhase.Moved)
            {
                Draging = 0;
                isChange = false;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended)
            {
                if (touchFirstFinger.position.x - touchMoveBeforePos.x != 0)
                    Draging = touchFirstFinger.position.x - touchMoveBeforePos.x;
                isChange = true;
                isOnce = false;
            }
        }

        //if (Input.GetMouseButtonUp(0))
        //{
        //    isChange = true;
        //    isOnce = false;
        //}
        //else if(Input.GetMouseButtonDown(0))
        //{
        //    Debug.Log("Dragging: " + Draging);
        //    if (userInput.MoveX != 0)
        //    {
        //        Draging = userInput.MoveX;
        //    }
        //    isChange = false;
        //}
#endif

        if (isChange && !isOnce)
        {
            StopAllCoroutines();
            isOnce = true;
            if (selectNum == 0)
            {
                if (Draging < 0)
                {
                    selectNum = 1;
                    StartCoroutine(ContentMove(-812.5f));
                }
            }
            else
            {
                if (Draging < 0 && selectNum < maxNum)
                {
                    selectNum++;
                    StartCoroutine(ContentMove(posXList[selectNum]));
                }
                else if (Draging > 0)
                {
                    selectNum--;
                    StartCoroutine(ContentMove(posXList[selectNum]));
                }
            }
            Draging = 0;
        }
    }
    private IEnumerator ContentMove(float endPosx)
    {
        float t = 0;
        while (t < 1)
        {
            t += 0.5f * Time.deltaTime;
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, new Vector2(endPosx, content.anchoredPosition.y), t);
            yield return null;
        }
        
    }
}
