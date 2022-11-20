using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#nullable enable

// String만 nullable 가능
[Serializable]
public class TreeData
{
    // 나무 이름
    public string? treeName;
    // seed 번호
    public int seedNumber;
    // 나무 파이프라인 이름
    public string? treePipeName;
    // 현재 land ID
    public int landId;
    // 나무가지 Material 이름
    public string? barkMaterial;
    // 나뭇잎 종류 groupId
    public int sproutGroupId;
    // 나뭇잎 종류에 따른 색깔 5가지 활성화 여부 (0=false, 1=true)
    public int sproutColor1;
    public int sproutColor2;
    public int sproutColor3;
    public int sproutColor4;
    public int sproutColor5;

    // Tree Growth Data //
    // 나무 scale
    public float scale;
    // 나무 가지 개수
    public int branch1;
    public int branch2;
    public int branch3;
    public int branch4;
    // 나뭇잎 개수
    public int sproutNum;
    // 나뭇잎 썩은 비율
    public float rottenRate;
    // 나뭇잎 너비
    public float sproutWidth;
    // 중력
    public float gravity;
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

[Serializable]
public class TreePipeline
{
    public int treeId;
    public List<string>? sleepIds;
     
    // Tree Growth Data //
    // 나무 scale
    public float scale;
    // 나무 가지 개수
    public int branch1;
    public int branch2;
    public int branch3;
    public int branch4;
    // 나뭇잎 개수
    public int sproutNum;
    // 나뭇잎 썩은 비율
    public float rottenRate;
    // 나뭇잎 너비
    public float sproutWidth;
    // 중력
    public float gravity;
    public string? createdAt;
    public string? updateAt;
}

[Serializable]
public class GetTreeData
{
    public int id;
    // 나무 이름
    public string? treeName;
    // seed 번호
    public int seedNumber;
    // 나무 파이프라인 이름
    public string? treePipeName;
    // 현재 land ID
    public int landId;
    // 나무가지 Material 이름
    public string? barkMaterial;
    // 나뭇잎 종류 groupId
    public int sproutGroupId;
    // 나뭇잎 종류에 따른 색깔 5가지 활성화 여부 (0=false, 1=true)
    public int sproutColor1;
    public int sproutColor2;
    public int sproutColor3;
    public int sproutColor4;
    public int sproutColor5;
    // 생성 및 업데이트
    public string? createdAt;
    public string? updatedAt;
    // 일차별 treeGrowth Data
    public List<TreeGrowth>? treeGrowths;  
}

// 5일차 나무 이미지 & 영상 업로드
[Serializable]
public class TreeFile
{
    // 나무 아이디
    public int treeId;
    // 첨부파일 아이디
    public string? attachFileIds;
}





// Day1 데이터 저장소
[Serializable]
public class ArrayTreeData
{
    public List<TreeData>? TreeDataList;
}
// Day2~5 데이터 저장소
[Serializable]
public class ArrayTreeData2
{
    public List<TreePipeline>? TreeDataList2;
}
// 데이터 로드할 때 저장소
[Serializable]
public class ArrayGetTreeData
{
    public List<GetTreeData>? getTreeDataList;
}
// My Collection 나무 이미지 & 영상 저장소
[Serializable]
public class ArrayTreeFile
{
    public List<TreeImgVideo>? treeFileList;
}

