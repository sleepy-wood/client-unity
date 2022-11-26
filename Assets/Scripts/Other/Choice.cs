using Broccoli.Pipe;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static TreeController;

public class Choice : MonoBehaviour
{
    public RectTransform content;
    private Vector3 createPos;


    public Dictionary<string, SeedType> pipeNameDict = new Dictionary<string, SeedType>()
    {
        { "BasicTree", SeedType.Basic },
        { "OakTree", SeedType.Oak },
        { "SakuraTree", SeedType.Sakura },
        { "DRTree", SeedType.DR },
        { "DemoTree_Red", SeedType.Demo }
    };
    async void Start()
    {
        for (int i = 0; i < DataTemporary.arrayCollectionDatas.collectionLists.Count; i++)
        {
            GameObject profile_resource = Resources.Load<GameObject>("Profile");
            GameObject profile_Prefab = Instantiate(profile_resource, content);
            profile_Prefab.name = profile_Prefab.name.Split('(')[0] + "_" + i;
            CollectionData collectionData = DataTemporary.arrayCollectionDatas.collectionLists[i];
           
            for (int j = 0; j < collectionData.treeAttachments.Count; j++)
            {
                if (collectionData.treeAttachments[j].mimeType.Contains("image"))
                {
                    Texture2D texture = await DataModule.WebrequestTextureGet(collectionData.treeAttachments[j].path, DataModule.NetworkType.GET);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    profile_Prefab.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
                    break;
                }
            }
            int temp = i;
            profile_Prefab.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => OnClickChoice(temp));
        }
        content.offsetMax = new Vector2(480 * DataTemporary.arrayCollectionDatas.collectionLists.Count, content.offsetMax.y);
    }

    public void OnClickChoice(int i)
    {
        CollectionData treeCollectionData = DataTemporary.arrayCollectionDatas.collectionLists[i];
        
        string pipeName = treeCollectionData.treeName;
        Pipeline treePipeline = DataTemporary.assetBundleTreePipeline.LoadAsset<Pipeline>(pipeName);

        if (!treePipeline)
        {
            Debug.LogError("Tree Pipe Line is Not loaded!: Pipeline is Null!");
        }

        treePipeline.seed = treeCollectionData.seedNumber;
        TreeController.SeedType selectedSeed = pipeNameDict[treeCollectionData.treePipeName];

        
        //// bark Material
        //string name = currentTreeData.barkMaterial;
        //Material mat = barkAssetBundle.LoadAsset<Material>(name);
        //treePipeline._serializedPipeline.barkMappers[0].customMaterial = mat;
        //// sproutGroup
        //SproutSeed sproutSeed = new SproutSeed();
        //treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
        //treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[0].groupId = currentTreeData.sproutGroupId;
        //// sproutColor
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[0].enabled = currentTreeData.sproutColor1 == 1 ? true : false;
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[1].enabled = currentTreeData.sproutColor2 == 1 ? true : false;
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[2].enabled = currentTreeData.sproutColor3 == 1 ? true : false;
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[3].enabled = currentTreeData.sproutColor4 == 1 ? true : false;
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[4].enabled = currentTreeData.sproutColor5 == 1 ? true : false;

        //#region TreeGrowth
        //// 1. Scale
        //scaleTo = pipeData.scale;
        //// 2. Branch Number
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minFrequency = pipeData.branch1;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].maxFrequency = pipeData.branch1;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].minFrequency = pipeData.branch2;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].maxFrequency = pipeData.branch2;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].minFrequency = pipeData.branch3;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].maxFrequency = pipeData.branch3;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].minFrequency = pipeData.branch4;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].maxFrequency = pipeData.branch4;
        //// 4. Sprout Number
        //treePipeline._serializedPipeline.sproutGenerators[0].minFrequency = pipeData.sproutNum;
        //treePipeline._serializedPipeline.sproutGenerators[0].maxFrequency = pipeData.sproutNum;
        //// 5. Ratio of Rotten Sprout : 0, 25, 50, 75, 100
        //List<int> groupNum = new List<int>() { 5, 6, 7, 8 };
        //for (int i = 0; i < (pipeData.rottenRate / 25); i++) // i < 0, 1, 2, 3, 4
        //{
        //    SproutSeed sproutGroup = new SproutSeed();
        //    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutGroup);
        //    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[^1].groupId = groupNum[i];
        //}
        //// 6. Sprout Width
        //foreach (SproutMesh s in treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes)
        //{
        //    s.width = pipeData.sproutWidth;
        //}
        //// 7. Gravity
        //for (int i = 0; i < 4; i++)
        //{
        //    if (!treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].isRoot)
        //    {
        //        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].minGravityAlignAtBase = pipeData.gravity;
        //        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].maxGravityAlignAtBase = pipeData.gravity;
        //    }
        //}
    }

    public void SettingChoicePos(Vector3 Pos)
    {
        createPos = Pos;
    }
}

