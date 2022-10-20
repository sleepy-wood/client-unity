using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharactorChoice : MonoBehaviour
{
    public Transform content;
    public int selectedIndex;
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
    public void OnClickChoiceButton()
    {

    }
}
