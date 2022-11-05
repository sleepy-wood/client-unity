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

    public GameObject UI_Canvas;
    private void Awake()
    {
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.SendRate = 30;
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
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient && UI_Canvas)
        {
            UI_Canvas.SetActive(false);
        }
    }
    public GameObject User { get; set; }

    // 나무 처음 심은 시간
    public DateTime firstPlantTime;

    // TreeController
    public GameObject treeController;
}
