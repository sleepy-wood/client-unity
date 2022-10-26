using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // 플레이어 재방문 여부
    public bool isFirstVisit;
}
