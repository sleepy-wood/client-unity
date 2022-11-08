using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Assets
{
    public string assetName;
}

[Serializable]
public class AssetBundlesData
{
    public string bundleName;
    public List<Assets> assetList;
}

[Serializable]
public class ArrayBundlesData
{
    public List<AssetBundlesData> bundlesList;
}