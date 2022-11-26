using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChoicePosition : MonoBehaviour, IClickedObject
{
    public GameObject collection;
    private bool isCreate = false;
    public void ClickMe()
    {
        Debug.Log("클릭됨: " + gameObject);
        if (!isCreate)
        {
            collection.SetActive(true);
            collection.GetComponent<Choice>().SettingChoicePos(int.Parse(gameObject.name[gameObject.name.Length - 1].ToString()));
            isCreate = true;
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void StairMe()
    {
    }
}
