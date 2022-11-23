using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ArrayCollectionData
{
    public List<CollectionData> collectionLists;
}

[System.Serializable]
public class CollectionData
{
    public int id;
    public string treeName;
    public int seedNumber;
    public string treePipeName;
    public string barkMaterial;
    public int sproutGroupId;
    public int sproutColor1;
    public int sproutColor2;
    public int sproutColor3;
    public int sproutColor4;
    public int sproutColor5;
    public int rarity;
    public int vitality;
    public int landId;
    public int userId;
    public string createdAt;
    public string updatedAt;
    public List<TreeGrowths_Collection> treeGrowths;
    public List<TreeAttachment> treeAttachments;
    public Product product;
}
[System.Serializable]
public class TreeAttachment
{
    public int id;
    public string filename;
    public string originalName;
    public string path;
    public string mimeType;
    public int size;
    public bool isThumbnail;
    public int treeId;
    public string createdAt;
    public string updatedAt;
}

[System.Serializable]
public class TreeGrowths_Collection
{
    public int id;
    public int treeDay;
    public int treeId;
    public string createdAt;
    public string updatedAt;
    public TreePipeLine_Collection treePipeline;
}
[System.Serializable]
public class TreePipeLine_Collection
{
    public int id;
    public float scale;
    public int branch1;
    public int branch2;
    public int branch3;
    public int branch4;
    public int sproutNum;
    public float rottenRate;
    public float sproutWidth;
    public float gravity;
    public int treeGrowthId;
    public string createdAt;
    public string updatedAt;
}
