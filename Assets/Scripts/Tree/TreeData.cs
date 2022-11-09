using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#nullable enable

// User - Tree - TreeMetaData (1일차,2일차,3일차,4일차,5일차) - TreePipelineData

[Serializable]
public class TreePipelineData
{
    public int pipeId;
    public int treeGrowthId;
    // 나무 scale
    public float scale;
    // 나무 가지 개수(4)
    public int branch1;
    public int branch2;
    public int branch3;
    public int branch4;
    // 나무 기둥 길이
    public float trunkLength;
    // 나뭇잎 개수
    public int sproutNum;
    // 나뭇잎 썩은 비율
    public float? rottenRate;
    // 중력
    public float gravity;
    // 뿌리 개수
    public int rootNum;
    // 나무가지 텍스처 이름
    public string? barkTexture;
    // 활성화할 sprout Texture index
    public int? sproutIndex;
    // 나무 첫 생성 시간
    public DateTime createdAt;
    public DateTime updatedAt;
}

[Serializable]
public class TreeGrowth
{
    public int growId;
    public int treeDay;
    public int treeId;  
    public DateTime createdAt;
    public DateTime deletedAt;
}

[Serializable]
public class TreeData
{
    public int treeDataId;
    // seed 번호
    public int seedNumber;
    // 나무 이름
    public string? treeName;
    // 나무 종류
    public string ?seedType; 
    // 나무 파이프라인 관련 정보
    public TreePipelineData? treePipelineData;
    // 현재 land ID
    public int landID;
    // 나무 생성 날짜
    public DateTime createdAt;
    public DateTime updatedAt;
}

[Serializable]
public class ArrayTreeData
{
    public List<TreeData>? treeDataList;
}


