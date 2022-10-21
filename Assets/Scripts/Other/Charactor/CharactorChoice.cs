using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class CharactorChoice : MonoBehaviour
{
    public Transform content;
    public int selectedIndex;
    public Transform explainText;
    public float textSpeed = 7f;

    private UserInput userInput;    
    private void Start()
    {
        OnClickPreviewCharactor(0);
        userInput = GetComponent<UserInput>();
        for (int i = 0; i < explainText.childCount; i++)
        {
            StartCoroutine(AlphaText(i));
        }
    }
    private void Update()
    {
        //Rotate를 하면 안내문 끄기
        if (userInput.Rotate != 0 && explainText.gameObject.activeSelf)
        {
            StopAllCoroutines();
            explainText.gameObject.SetActive(false);
        }

        transform.GetChild(selectedIndex).Rotate(transform.up, -userInput.Rotate * 2);
    }
    public IEnumerator AlphaText(int i)
    {
        Color c = explainText.GetChild(i).name.Contains("Text") ?
            explainText.GetChild(i).GetComponent<Text>().color : explainText.GetChild(i).GetComponent<Image>().color;
        bool isAlphaTrue = c.a == 1 ? true : false;
        while (true)
        {
            while (!isAlphaTrue)
            {
                if (c.a >= 0.99f)
                    isAlphaTrue = true;

                c.a = Mathf.Lerp(c.a, 1, Time.deltaTime * textSpeed);
                if (explainText.GetChild(i).name.Contains("Text"))
                    explainText.GetChild(i).GetComponent<Text>().color = c;
                else
                    explainText.GetChild(i).GetComponent<Image>().color = c;
                yield return null;
            }
            while (isAlphaTrue)
            {
                if (c.a <= 0.01f)
                    isAlphaTrue = false;

                c.a = Mathf.Lerp(c.a, 0, Time.deltaTime * textSpeed);
                if (explainText.GetChild(i).name.Contains("Text"))
                    explainText.GetChild(i).GetComponent<Text>().color = c;
                else
                    explainText.GetChild(i).GetComponent<Image>().color = c;
                yield return null;
            }
            yield return null;
        }
    }
    public void OnClickPreviewCharactor(int index)
    {
        for (int i = 0; i < content.childCount; i++)
        {
            if (i == index)
            {
                transform.GetChild(i).transform.eulerAngles = new Vector3(0, 180, 0);
                transform.GetChild(i).gameObject.SetActive(true);
                content.GetChild(i).GetComponent<Image>().color = Color.gray;
                selectedIndex = i;
            }
            else
            {
                transform.GetChild(i).gameObject.SetActive(false);
                content.GetChild(i).GetComponent<Image>().color = Color.white;
            }
        }
    }
    public async void OnClickChoiceButton()
    {
        DataTemporary.MyUserData.UserAvatar = transform.GetChild(selectedIndex).gameObject.name;
        ResultTemp<Token> data = await DataModule.WebRequest<ResultTemp<Token>>(
            "/api/v1/users/" +DataModule.REPLACE_BEARER_TOKEN.ToString(),
            DataModule.NetworkType.POST);

        if (data.result)
        {

        }
    }
}
