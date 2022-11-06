using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#nullable enable


[Serializable]
public class TreePipelineData
{
    // 나무 높이
    public float? baseLength;
    // 나무 가지 개수
    public List<int>? branchNums;
    // 나뭇잎 개수
    public int? sproutNum;
    // 나뭇잎 썩은 비율
    public float? rottenRate;
    // 나무 두께
    public float? thickness;
    // 나무 꺾임(Noise)
    public float? bending;
    // 중력
    public float? gravity;
    // 뿌리 개수
    public int? rootNum;
    // 나무가지 텍스처
    public Texture2D? barkTexture;
    // 나뭇잎 enabled 상태

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


