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
    private void Start()
    {
        OnClickPreviewCharactor(0);
    }
    public void OnClickPreviewCharactor(int index)
    {
        for (int i = 0; i < content.childCount; i++)
        {
            if (i == index)
            {
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
