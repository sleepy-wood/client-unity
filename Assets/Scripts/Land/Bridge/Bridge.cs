using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bridge : MonoBehaviour, IClickedObject
{
    /// <summary>
    /// Bridge가 건설된 상태니?
    /// </summary>
    public enum BridgeType
    {
        Build,
        NotBuild
    }
    
    public BridgeType currentBridgeType = BridgeType.NotBuild;

    private MeshRenderer meshRenderer;
    private Material material;
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
    }

    private void Update()
    {
        if(currentBridgeType == BridgeType.NotBuild)
        {
            material.color = Color.black;
        }
        else if(currentBridgeType == BridgeType.Build)
        {
            material.color = Color.red;
        }

    }
    public void ClickMe()
    {
        material.color = Color.green;
        currentBridgeType = BridgeType.Build;
    }

    public void StairMe()
    {
        material.color = Color.yellow;
    }
}
