using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceRespawnPos : MonoBehaviourPunCallbacks
{
    public static ChoiceRespawnPos Instance;
    private Camera camera_share;
    public GameObject collection;
    public bool isComplete { get; set; }

    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }
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

        if(isComplete)
        {
            collection.SetActive(false);
            camera_share.gameObject.SetActive(false);
            isComplete = false;
        }
    }
    public void OnClickBackButton()
    {
        collection.SetActive(false);
        camera_share.gameObject.SetActive(false);
    }
    public void OnClickCreateButton()
    {
        for(int i = 1; i < 4; i++)
        {
            if (transform.GetChild(i).GetComponent<MeshRenderer>().enabled)
            {
                camera_share.gameObject.SetActive(true);
            }
        }
    }
    public void OnClickBackSharedLandButton()
    {
        PhotonNetwork.LoadLevel("MyWorld");
    }
}
