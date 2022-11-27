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
            collection.transform.parent.GetComponent<Choice>().SettingChoicePos(int.Parse(gameObject.name[gameObject.name.Length - 1].ToString()));
            collection.SetActive(true);
            isCreate = true;
        }
    }

    public void StairMe()
    {
    }
    private void Update()
    {
        if (ChoiceRespawnPos.Instance.isCompleteList[int.Parse(gameObject.name[gameObject.name.Length - 1].ToString())])
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
