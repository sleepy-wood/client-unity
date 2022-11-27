using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MyWorld 각 섬이 어떤 나무를 지니고 있는지 Bush & 팻말 표시
/// </summary>
public class MyWorldSign : MonoBehaviour
{
    [SerializeField] private TextMesh txtTreeName;
    [SerializeField] private int mylandId;
    [SerializeField] private GetTreeData treeData;
    AssetBundle leafTextureBundle;
    [SerializeField] private Transform bushTr;
    [SerializeField] private Transform landManagerTr;

    void Start()
    {
        // TreeData에 있는 데이터를 통해 각 섬의 팻말 & Bush 표시
        List<GetTreeData> treeList = DataTemporary.GetTreeData.getTreeDataList;
        for (int i = 0; i < treeList.Count; i++)
        {
            int landId = treeList[i].landId;
            GameObject land = landManagerTr.GetChild(landId - 1).gameObject;

            // Tree Data가 있다면 팻말 & Bush 활성화
            GameObject sign = land.transform.GetChild(0).gameObject;
            sign.SetActive(true);
            GameObject bush = land.transform.GetChild(1).gameObject;
            bush.SetActive(true); 

            // Tree Name으로 팻말 Text 표시
            sign.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = treeList[i].treeName;

            // 나무의 Day에 따라 Bush 그룹 활성화
            int day = treeList[i].treeGrowths.Count;
            for (int j=0; j<day-1; j++)
            {
                
            }
                                                                                             
        }



            // 팻말이 있는 랜드의 아이디
            mylandId = int.Parse(gameObject.name.Split('d')[1]);

        // 해당 섬 나무의 이름 팻말에 표시
        
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
