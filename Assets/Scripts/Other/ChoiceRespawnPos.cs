using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceRespawnPos : MonoBehaviour
{
    private Camera camera_share;
    public GameObject collection;

    private void Start()
    {
        camera_share = transform.GetChild(0).GetComponent<Camera>(); 
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = camera_share.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.transform.GetComponent<IClickedObject>() != null && Input.GetMouseButtonDown(0))
            {
                hit.transform.GetComponent<IClickedObject>().ClickMe();
            }
        }
    }
    public void OnClickBackButton()
    {
        collection.SetActive(false);
        camera_share.gameObject.SetActive(false);
    }
    public void OnClickCreateButton()
    {
        camera_share.gameObject.SetActive(true);
    }
}
