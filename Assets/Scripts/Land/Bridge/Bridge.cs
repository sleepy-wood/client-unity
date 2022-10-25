using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bridge : MonoBehaviour, IClickedObject
{
    /// <summary>
    /// Bridge가 건설된 상태니?
    /// 건설된거니
    /// </summary>
    public enum BridgeType
    {
        Build,
        WillBuild,
        NotBuild
    }
    
    public BridgeType currentBridgeType = BridgeType.NotBuild;

    private MeshRenderer meshRenderer;
    private Material material;
    private bool isStair = true;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
    }

    private void Update()
    {
        if(currentBridgeType == BridgeType.Build)
        {
            material.color = Color.black;
            Color color = material.color;
            color.a = 1;
            material.color = color;
        }
        else if(!isStair && currentBridgeType == BridgeType.NotBuild)
        {
            material.color = Color.red;
            Color color = material.color;
            color.a = 0.05f;
            material.color = color;
        }
        else if (!isStair && currentBridgeType == BridgeType.WillBuild)
        {
            material.color = Color.green;
            Color color = material.color;
            color.a = 0.05f;
            material.color = color;
        }
        isStair = false;
    }
    public void ClickMe()
    {
        if (currentBridgeType == BridgeType.NotBuild)
        {
            currentBridgeType = BridgeType.WillBuild;
        }
        else if(currentBridgeType == BridgeType.WillBuild)
        {
            currentBridgeType = BridgeType.NotBuild;
        }
    }

    public void StairMe()
    {
        if (currentBridgeType == BridgeType.NotBuild)
        {
            isStair = true;
            material.color = Color.red;
            Color color = material.color;
            color.a = 0.5f;
            material.color = color;
        }
        else if (currentBridgeType == BridgeType.WillBuild)
        {
            isStair = true;
            material.color = Color.green;
            Color color = material.color;
            color.a = 0.5f;
            material.color = color;
        }
    }
}
