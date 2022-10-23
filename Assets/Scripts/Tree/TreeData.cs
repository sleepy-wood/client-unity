using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

public class TreeGrowthData
{
        // 나무 길이
        public float? treeLength;
        // 나무 굵기
        public float? treeThick;
        // 나무 풍성함
        public float? treeAbundance;
}

public class TreeData
{
        // 나무 이름
        public string? treeName;
        // 잎 텍스처 
        public Sprite ?leafTexture;
        // DayCount
        public int? treeDay;
        // 나무 정보
        public TreeGrowthData? treegrowthData;
        // 현재 land  ID
        public int ? landID;
}

public class ArrayTreeData
{
        public List<TreeData>? treeDatas;
}


