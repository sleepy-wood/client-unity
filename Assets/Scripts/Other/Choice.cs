using Broccoli.Factory;
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
    public List<Sprite> grades = new List<Sprite>();

    public RectTransform content;
    public float scaleTo = 1;
    public List<TreeFactory> treeFactory =new List<TreeFactory>();
    public Transform previewTree;

    private int createPos;

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
            GameObject profile_resource = Resources.Load<GameObject>("Share_CollectionBtn");
            GameObject profile_Prefab = Instantiate(profile_resource, content);
            profile_Prefab.name = profile_Prefab.name.Split('(')[0] + "_" + i;
            CollectionData collectionData = DataTemporary.arrayCollectionDatas.collectionLists[i];

            if (collectionData.rarity > 90 && collectionData.rarity <= 100)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[0];
            }
            else if (collectionData.rarity > 80 && collectionData.rarity <= 90)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[1];
            }
            else if (collectionData.rarity > 70 && collectionData.rarity <= 80)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[2];
            }
            else if (collectionData.rarity > 60 && collectionData.rarity <= 70)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[3];
            }
            else if (collectionData.rarity > 50 && collectionData.rarity <= 60)
            {
                profile_Prefab.GetComponent<Image>().sprite = grades[4];
            }

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
        content.offsetMax = new Vector2(550 * DataTemporary.arrayCollectionDatas.collectionLists.Count, content.offsetMax.y);
    }

    public void OnClickChoice(int i)
    {
        Debug.Log($"선택 : {i}");
        treeFactory[i].gameObject.SetActive(true);

        //CollectionData treeCollectionData = DataTemporary.arrayCollectionDatas.collectionLists[i];
        //print("1");
        //string pipeName = treeCollectionData.treePipeName;
        //Pipeline treePipeline = DataTemporary.assetBundleTreePipeline.LoadAsset<Pipeline>(pipeName);
        //print("2");
        //if (!treePipeline)
        //{
        //    Debug.LogError("Tree Pipe Line is Not loaded!: Pipeline is Null!");
        //}
        //print("3");
        //treePipeline.seed = treeCollectionData.seedNumber;
        //print("4");
        //TreeController.SeedType selectedSeed = pipeNameDict[treeCollectionData.treePipeName];
        //print("5");
        //string name = treeCollectionData.barkMaterial;
        //print("6");
        //Material mat = DataTemporary.treeBarkAssetBundle.LoadAsset<Material>(name);
        //print("7");
        //treePipeline._serializedPipeline.barkMappers[0].customMaterial = mat;
        //print("8");

        //SproutSeed sproutSeed = new SproutSeed();
        //treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
        //print("9");
        //treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[0].groupId = treeCollectionData.sproutGroupId;
        //print("10");
        //// sproutColor
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[0].enabled = treeCollectionData.sproutColor1 == 1 ? true : false;
        //print("11");
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[1].enabled = treeCollectionData.sproutColor2 == 1 ? true : false;
        //print("12");
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[2].enabled = treeCollectionData.sproutColor3 == 1 ? true : false;
        //print("13");
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[3].enabled = treeCollectionData.sproutColor4 == 1 ? true : false;
        //print("14");
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[4].enabled = treeCollectionData.sproutColor5 == 1 ? true : false;
        //print("15");

        //TreePipeLine_Collection treePipeline_Collection = treeCollectionData.treeGrowths[0].treePipeline;
        //print("16");
        //// 1. Scale
        //float scaleTo = treePipeline_Collection.scale;
        //print("17");
        //// 2. Branch Number
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minFrequency = treePipeline_Collection.branch1;
        //print("18");
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].maxFrequency = treePipeline_Collection.branch1;
        //print("19");
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].minFrequency = treePipeline_Collection.branch2;
        //print("20");
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].maxFrequency = treePipeline_Collection.branch2;
        //print("21");
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].minFrequency = treePipeline_Collection.branch3;
        //print("22");
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].maxFrequency = treePipeline_Collection.branch3;
        //print("23");
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].minFrequency = treePipeline_Collection.branch4;
        //print("24");
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].maxFrequency = treePipeline_Collection.branch4;
        //print("25");
        //// 4. Sprout Number
        //treePipeline._serializedPipeline.sproutGenerators[0].minFrequency = 20;
        //treePipeline._serializedPipeline.sproutGenerators[0].maxFrequency = 20;
        //print("26");
        //// 5. Ratio of Rotten Sprout : 0, 25, 50, 75, 100
        //List<int> groupNum = new List<int>() { 5, 6, 7, 8 };
        //for (int j = 0; j < (treePipeline_Collection.rottenRate / 25); j++) // i < 0, 1, 2, 3, 4
        //{
        //    SproutSeed sproutGroup = new SproutSeed();
        //    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutGroup);
        //    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[^1].groupId = groupNum[j];
        //}
        //print("27");
        //// 6. Sprout Width
        //foreach (SproutMesh s in treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes)
        //{
        //    s.width = treePipeline_Collection.sproutWidth;
        //}
        //print("28");
        //// 7. Gravity
        //for (int j = 0; j < 4; j++)
        //{
        //    if (!treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[j].isRoot)
        //    {
        //        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[j].minGravityAlignAtBase = treePipeline_Collection.gravity;
        //        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[j].maxGravityAlignAtBase = treePipeline_Collection.gravity;
        //    }
        //}
        //print("29");
        //treeFactory[createPos].LoadPipeline(treePipeline.Clone(), true);
        //print("30");
        //treeFactory[createPos].transform.GetChild(1).gameObject.layer = 11;
        //print("31");
        //treeFactory[createPos].transform.GetChild(1).localScale = new Vector3(scaleTo, scaleTo, scaleTo);
        //print("32");
        //print($"scaleTo = {scaleTo}");
        //print($"TreeFactory SetActive = {treeFactory[createPos].gameObject.activeSelf}");
        //ChoiceRespawnPos.Instance.isComplete = true;
    }

    public void SettingChoicePos(int Pos)
    {
        createPos = Pos;
    }
}

