using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class UI_Collection : MonoBehaviour
{
    private RectTransform content;
    private int selectNum = 0;
    private GameObject user;
    private UserInput userInput;
    private int maxNum;
    public List<float> posXList = new List<float>();
    private void Awake()
    {
        content = transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<RectTransform>();
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
    float Draging = 0;
    bool isChange = false;
    bool isOnce = false;
    private void Update()
    {
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
        if (Input.GetMouseButtonUp(0))
        {
            isChange = true;
            isOnce = false;
        }
        else if(Input.GetMouseButtonDown(0))
        {
            Draging = userInput.DragX;
            isChange = false;
        }
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
