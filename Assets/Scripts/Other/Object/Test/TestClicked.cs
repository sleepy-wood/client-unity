using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestClicked : MonoBehaviour, IClickedObject
{
    public Text uiText;

    public void ClickMe()
    {
        //Debug.Log("나 클릭됐음: " + transform.gameObject);
        uiText.text = "나 클릭됐음: " + transform.gameObject;
    }
}
