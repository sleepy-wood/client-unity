using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#nullable enable

// User - Tree - TreeMetaData (1일차,2일차,3일차,4일차,5일차) - TreePipelineData

[Serializable]
public class TreePipeline
{
    public int id;
    public float scale;
    public int branch1;
    public int branch2;
    public int branch3;
    public int branch4;
    public float trunkLength;
    public int sproutNum;
    public float rottenRate;
    public float gravity;
    public int rootNum;
    public string? barkTexture;
    public int sproutIndex;
    public int treeGrowthId;
    public string? createdAt;
    public string? updateAt;
}

[Serializable]
public class TreeGrowth
{
    public int id;
    public int treeDay;
    public int treeId;
    public string? createdAt;
    public string? updatedAt;
    public List<TreePipeline>? treePipeline;
}

// String만 nullable 가능
[Serializable]
public class PutTreeData
{
    // 나무 이름
    public string? treeName;
    // seed 번호
    public int seedNumber;
    // 나무 종류

    public string? seedType;
    // 현재 land ID
    public int landId;

    // 나무 scale
    public float scale;
    // 나무 가지 개수
    public int branch1;
    public int branch2;
    public int branch3;
    public int branch4;
    // 나무 기둥 길이
    public float trunkLength;
    // 나뭇잎 개수
    public int sproutNum;
    // 나뭇잎 썩은 비율
    public float rottenRate;
    // 중력
    public float gravity;
    // 뿌리 개수
    public int rootNum;
    // 나무가지 텍스처 이름
    public string? barkTexture;
    // 활성화할 sprout Texture index
    public int sproutIndex;
}
[Serializable]
public class GetTreeData
{
    public int id;
    public string? treeName;
    public int seedNumber;
    public string? seedType;
    public int landId;
    public int userId;
    public string? createdAt;
    public string? updatedAt;
    public List<TreeGrowth>? treeGrowths;
}

[Serializable]
public class ArrayPutTreeData
{
    public List<PutTreeData>? putTreeDataList;
}


[Serializable]
public class ArrayGetTreeData
{
    public List<GetTreeData>? getTreeDataList;
}


