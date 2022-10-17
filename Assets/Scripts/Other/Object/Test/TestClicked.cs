using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestClicked : MonoBehaviour, IClickedObject
{
    public Text uiText;

    public void ClickMe()
    {
        //Debug.Log("³ª Å¬¸¯µÆÀ½: " + transform.gameObject);
        uiText.text = "³ª Å¬¸¯µÆÀ½: " + transform.gameObject;
    }
}
