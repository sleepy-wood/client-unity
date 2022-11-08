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
    // 나무 scale
    public float scale;
    // 나무 가지 개수
    public List<int>? branchNums;
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
    // 나뭇잎 enabled 상태
    public List<bool>? sproutEnabled;
}

[Serializable]
public class TreeData
{
    // seed 번호
    public int seedNumber;
    // 나무 이름
    public string? treeName;
    // 나무 종류
    public string ?seedType; 
    // 처음 심은 날짜
    public DateTime firstPlantDate;
    // 나무 파이프라인 관련 정보
    public TreePipelineData? treePipelineData;
    // 현재 land ID
    public int landID;
}

[Serializable]
public class ArrayTreeData
{
    public List<TreeData>? treeDataList;
}


