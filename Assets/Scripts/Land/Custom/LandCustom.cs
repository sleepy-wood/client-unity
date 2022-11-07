using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandCustom : MonoBehaviour
{
    public enum SelectState
    {
        None,
        Selected
    }
    //현재 고른 상태인가?
    public SelectState selectState = SelectState.None;
    private GameObject selectedObject;
    private UserInput userInput;
    private void Start()
    {
        userInput = GetComponent<UserInput>();
    }
    private void Update()
    {
        if(selectState == SelectState.None)
        {
            if (userInput.Interact)
            {

            }
        }
    }
}
