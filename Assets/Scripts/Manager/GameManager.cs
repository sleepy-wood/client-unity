using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        User = PhotonNetwork.Instantiate("User", new Vector3(Random.Range(-3, 3), 5, Random.Range(-3, 3)), Quaternion.identity);
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    
    public GameObject User { get; private set; }

    // 나무 처음 심은 시간
    public DateTime firstPlantTime;

    // TreeController
    public GameObject treeController;
}
