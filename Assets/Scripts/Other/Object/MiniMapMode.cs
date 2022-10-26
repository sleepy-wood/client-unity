using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapMode : MonoBehaviour
{
    private UserInteract userInteract;
    private UserInput userInput;

    private void Start()
    {
        userInteract = GameManager.Instance.User.GetComponent<UserInteract>();
        userInput = GameManager.Instance.User.GetComponent<UserInput>();
    }
    private void Update()
    {
        if (userInput.LongInteract)
        {
            GameObject Land = userInteract.OnLand();
            if (Land)
            {

            }
        }
    }
}
