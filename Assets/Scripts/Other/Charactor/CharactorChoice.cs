using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharactorChoice : MonoBehaviour
{
    public Transform content;
    public void OnClickPreviewCharactor(int index)
    {
        for (int i = 0; i < content.childCount; i++)
        {
            if (i == index)
            {
                transform.GetChild(i).gameObject.SetActive(true);
                content.GetChild(i).GetComponent<Image>().color = Color.gray;
            }
            else
            {
                transform.GetChild(i).gameObject.SetActive(false);
                content.GetChild(i).GetComponent<Image>().color = Color.white;
            }
        }

    }
}
