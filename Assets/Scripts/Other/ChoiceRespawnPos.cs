using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceRespawnPos : MonoBehaviour
{
    private Camera camera;

    private void Start()
    {
        camera = transform.GetChild(0).GetComponent<Camera>(); 
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = camera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.transform.GetComponent<IClickedObject>() != null)
            {
                hit.transform.GetComponent<IClickedObject>().ClickMe();
            }
        }
    }
}
