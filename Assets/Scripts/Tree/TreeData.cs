using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#nullable enable

[Serializable]
public class TreePipelineData 
{
    // 가지 개수
    public int? branchNum;
    // 잎 Frequency
    public int? sproutFreq;
    // 나뭇잎 텍스처 enabled 상태
    public List<bool>? sproutTexture;
    // 자식 개수
    public int? rootChild;
    // Base 나무 길이
    public int? baseLength;
    // 나무 굵기
    public float trunkThick;
}

[Serializable]
public class TreeData
{
    // seed 번호
    public int? seedNumber;
    // 나무 이름
    public string? treeName;
    // 처음 심은 날짜
    public DateTime? firstPlantDate;
    // 나무 파이프라인 관련 정보
    public TreePipelineData? treePipelineData;
    // 현재 land ID
    public int? landID;
}

[Serializable]
public class ArrayTreeData
{
    public List<TreeData>? treeDatas;
}


