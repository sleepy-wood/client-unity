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
    // 희귀성
    public int rarity;
    // 생명력
    public int vitality;
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
    public TreePipeline? treePipeline;
}

[Serializable]
public class TreePipeline
{
    public int treeId;
     
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
    // 희귀성
    public int rarity;
    // 생명력
    public int vitality;
    // 생성 및 업데이트 시간
    public string? createdAt;
    public string? updatedAt;
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
    // 희귀성
    public int rarity;
    // 생명력
    public int vitality;
    // 현재 land ID
    public int landId;
    // 유저 ID
    public string userId;
    // 생성 및 업데이트
    public string? createdAt;
    public string? updatedAt;
    // 일차별 treeGrowth Data
    public List<TreeGrowth>? treeGrowths;  
}

// 파일 업로드
[Serializable]
public class TreeFile
{
    public int id;
} 

// 5일차 나무 이미지 & 영상 업로드
[Serializable]
public class TreeImgVideo
{
    // 나무 아이디
    public int treeId;
    // 첨부파일 아이디
    public List<int>? attachFileIds;
}

[Serializable]
public class GetTreeImgVideo
{
    string filename;
    string originalName;
    string path;
    string mimeType;
    int size;
    int treeId;
    bool isThumbnail;
    string deletedAt;
    int id;
    string createdAt;
    string updatedAt;
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
public class ArrayTreeImgVideo
{
    public List<TreeImgVideo>? TreeImgVideoList;
}

