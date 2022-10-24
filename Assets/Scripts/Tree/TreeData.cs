using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#nullable enable

[Serializable]
public class TreeGrowthData
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
}

[Serializable]
public class TreeData
{
        // 나무 이름
        public string? treeName;
        // 잎 텍스처 
        public Texture2D ?leafTexture;
        // DayCount
        public int? treeDay;
        // 나무 정보
        public TreeGrowthData? treegrowthData;
        // 현재 land  ID
        public int ? landID;
}

[Serializable]
public class ArrayTreeData
{
        public List<TreeData>? treeDatas;
}


