using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestClicked : MonoBehaviour, IClickedObject
{
    public Text uiText;

    public void ClickMe()
    {
        //Debug.Log("�� Ŭ������: " + transform.gameObject);
        uiText.text = "�� Ŭ������: " + transform.gameObject;
    }
}
