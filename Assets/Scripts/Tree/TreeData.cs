using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#nullable enable

[Serializable]
public class TreeGrowthData  // TreeCustomData
{
    // 가지 개수
    public int? branchNum;
    // 잎 Frequency
    public int? sproutFreq;
    // 자식 개수
    public int? rootChild;
    // Base 나무 길이
    public int? baseLength;
    // 나무 굵기
    public float trunkThick;

    // 나무 가지, 잎, 꽃 텍스처 ?
}

[Serializable]
public class TreeData
{
    // 나무 이름
    public string? treeName;
    // 잎 텍스처 리스트
    public List<Texture2D> leafTextures = new List<Texture2D>();
    // 가지 텍스처 리스트
    public List<Texture2D> branchTextures = new List<Texture2D>();
    // 꽃 텍스처 리스트
    public List<Texture2D> flowerTextures = new List<Texture2D>();
    // 나무를 심은 날짜
    public DateTime? treePlantDate;
    // 나무 성장 관련 정보
    public TreeGrowthData? treeGrowthData;
    // 현재 land  ID
    public int? landID;
}

[Serializable]
public class ArrayTreeData
{
    public List<TreeData>? treeDatas;
}


