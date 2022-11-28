using Broccoli.Factory;
using Broccoli.Pipe;
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
    public List<bool> isCompleteList = new List<bool>();
    public TreeFactory treeFactory;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }
    private void Start()
    {
        camera_share = transform.GetChild(0).GetComponent<Camera>();
        for (int i = 1; i < transform.childCount; i++)
        {
            isCompleteList.Add(false);
        }
        isCompleteList[3] = true;
        isCompleteList[4] = true;
    }


    private void Update()
    {
        if (collection.activeSelf)
        {
            GameManager.Instance.User.GetComponent<UserInteract>().moveControl = true;
        }
        else
        {
            GameManager.Instance.User.GetComponent<UserInteract>().moveControl = false;
        }
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
        for(int i = 0; i < transform.childCount-1; i++)
        {
            if (!isCompleteList[i])
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
