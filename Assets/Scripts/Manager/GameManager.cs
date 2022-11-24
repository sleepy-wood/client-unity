using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public bool test = false;
    public GameObject UI_Canvas;
    public Transform respawnPos;
    private void Awake()
    {
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!Instance)
        {
            Instance = this;
        }
        if (!test)
        {
            if (!User)
                User = PhotonNetwork.Instantiate("User", respawnPos.position, Quaternion.identity);
        }
        else
        {
            if (!User)
                User = GameObject.Find("User");
        }
    }
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient && UI_Canvas)
        {
            UI_Canvas.SetActive(false);
        }

    }
    public GameObject User { get; set; }

    public TimeManager timeManager;
    public TreeController treeController;
}
