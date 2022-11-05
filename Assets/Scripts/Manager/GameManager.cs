using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (!User)
            User = PhotonNetwork.Instantiate("User", new Vector3(Random.Range(-3, 3), 2, Random.Range(-3, 3)), Quaternion.identity);

    }
    public GameObject User { get; set; }

    // 나무 처음 심은 시간
    public DateTime firstPlantTime;

    // TreeController
    public GameObject treeController;
}
