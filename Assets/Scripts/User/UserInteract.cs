using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInteract : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private UserInput userInput;

    private void Start()
    {
        userInput = GetComponent<UserInput>();
    }
    private void Update()
    {
        #region Player Move

        Vector3 moveDir = userInput.MoveX * transform.right + userInput.MoveZ * transform.forward;
        moveDir.Normalize();
        transform.GetChild(0).LookAt(transform.position + moveDir * 10);
        transform.position += moveSpeed * moveDir * Time.deltaTime;

        #endregion

        #region Player Click
        if (userInput.Interact)
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            LayerMask layerMask = 1 << LayerMask.NameToLayer("Ground");
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
            {
                if (hit.transform.GetComponent<IClickedObject>() != null)
                {
                    hit.transform.GetComponent<IClickedObject>().ClickMe();
                }
            }
        }
        #endregion
    }
}
