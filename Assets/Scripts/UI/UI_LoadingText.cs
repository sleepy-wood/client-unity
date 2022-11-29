using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoadingText : MonoBehaviour
{
    private Text loadingText;
    void Start()
    {
        loadingText = GetComponent<Text>();
        StartCoroutine(LoadingMove());
    }

    IEnumerator LoadingMove()
    {

        while (true)
        {
            string text = loadingText.text;
            if (text.Split(".").Length >= 4)
            {
                text = "나무에 물 주는 중";
            }
            else
            {
                text += ".";
            }
            loadingText.text = text;

            yield return new WaitForSeconds(0.1f);
        }
    }
}
