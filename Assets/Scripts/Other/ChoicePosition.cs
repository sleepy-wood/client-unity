using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChoicePosition : MonoBehaviour, IClickedObject
{
    public GameObject collection;
    public void ClickMe()
    {
        collection.SetActive(true);
        collection.GetComponent<Choice>().SettingChoicePos(transform.position);
    }

    public void StairMe()
    {
    }
}
