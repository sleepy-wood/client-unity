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
        meshRenderer = transform.parent.GetChild(1).GetComponent<MeshRenderer>();
        material = meshRenderer.material;
    }

    private void Update()
    {
        if(currentBridgeType == BridgeType.Build)
        {
            Color color = material.color;
            color.a = 1;
            material.color = color;
        }
        else if(!isStair && currentBridgeType == BridgeType.NotBuild)
        {
            Color color = material.color;
            color.a = 0.3f;
            material.color = color;
        }
        else if (!isStair && currentBridgeType == BridgeType.WillBuild)
        {
            Color color = material.color;
            color.a = 0.7f;
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
            Color color = material.color;
            color.a = 0.7f;
            material.color = color;
        }
        else if (currentBridgeType == BridgeType.WillBuild)
        {
            isStair = true;
            Color color = material.color;
            color.a = 0.7f;
            material.color = color;
        }
    }
}
