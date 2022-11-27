using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MyWorld 각 섬이 어떤 나무를 지니고 있는지 표시
/// </summary>
public class MyWorldSign : MonoBehaviour
{
    [SerializeField] private TextMesh txtTreeName;
    [SerializeField] private int mylandId;
    [SerializeField] private GetTreeData treeData;
    AssetBundle leafTextureBundle;
    [SerializeField] private Transform bushTr;

    void Start()
    {
        // 팻말이 있는 랜드의 아이디
        mylandId = int.Parse(gameObject.name.Split('d')[1]);

        // 해당 섬 나무의 이름 팻말에 표시
        List<GetTreeData> treeList = DataTemporary.GetTreeData.getTreeDataList;
        for (int i=0; i< treeList.Count; i++)
        {
            if (treeList[i].landId == mylandId)
            {
                txtTreeName.text = treeList[i].treeName;
                treeData = treeList[i];
            }
        }

        // Bush 표현
        leafTextureBundle = DataTemporary.treeLeafTextureBundle;

        foreach ( Transform bush in bushTr)
        {
            Material bushMat = bush.GetComponent<Material>();
            
        }
        // Sprout Group 선택
        // Sprout Group 내의 Color 선택
    }
}
