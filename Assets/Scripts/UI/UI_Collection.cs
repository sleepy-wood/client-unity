using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Collection : MonoBehaviour
{
    private RectTransform content;

    private void Start()
    {
        content = transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<RectTransform>();
    }
    private void Update()
    {
        
    }
}
