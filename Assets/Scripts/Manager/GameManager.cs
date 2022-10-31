using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        User = GameObject.Find("User");
    }
    
    public GameObject User { get; private set; }

    // 나무 처음 심은 시간
    public DateTime firstPlantTime;

    // TreeController
    public GameObject treeController;
}
