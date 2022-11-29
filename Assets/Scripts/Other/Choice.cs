using Broccoli.Factory;
using Broccoli.Pipe;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Choice : MonoBehaviour
{
    public List<Sprite> grades = new List<Sprite>();

    public RectTransform content;
    public float scaleTo = 1;
    public List<TreeFactory> treeFactory =new List<TreeFactory>();
    public Transform previewTree;

    private int createPos;


    #region 기본 세팅 변수 저장소
    // Pipeline Element별 frequency Min/Max값 저장소
    [System.Serializable]
    public class MinMax
    {
        public int min;
        public int max;
    }
    [System.Serializable]
    public class TreeSetting
    {
        public List<MinMax> minMaxList = new List<MinMax>();
        public int rootFreq;
        public int rootBaseLength;
        public float girthBase;
        public float scale;
    }
    public enum SeedType
    {
        None,
        Basic,
        Oak,
        Sakura,
        DR,
        Demo
    }
    // 나무 종류별 관련 변수 클래스
    [System.Serializable]
    public class TreeStore
    {
        public SeedType seedType = SeedType.None;
        // seed 기본 세팅값
        public List<TreeSetting> treeSettings = new List<TreeSetting>();
    }
    // 나무 종류별 관련 변수 클래스의 모음 리스트
    public List<TreeStore> treeStores = new List<TreeStore>();
    #endregion

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

        Debug.Log("시작");
        createPos = 3;
        if(DataTemporary.arrayCollectionDatas.collectionLists.Count > 0)
            OnClickChoice(0);
    }

    public void OnClickChoice(int i)
    {

        //// Tree Data
        //CollectionData treeCollectionData = DataTemporary.arrayCollectionDatas.collectionLists[i];

        //// Pipeline
        //string pipeName = treeCollectionData.treePipeName;
        //Pipeline treePipeline = DataTemporary.assetBundleTreePipeline.LoadAsset<Pipeline>("Lava");
        //if (!treePipeline)
        //{
        //    Debug.LogError("Tree Pipe Line is Not loaded!: Pipeline is Null!");
        //}

        //// SeedNumber
        //treePipeline.seed = treeCollectionData.seedNumber;

        //// Bark Material
        //string name = treeCollectionData.barkMaterial;
        //Material mat = DataTemporary.treeBarkAssetBundle.LoadAsset<Material>(name);
        //treePipeline._serializedPipeline.barkMappers[0].customMaterial = mat;

        //// Sprout Group Id
        //SproutSeed sproutSeed = new SproutSeed();
        //treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
        //treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[0].groupId = treeCollectionData.sproutGroupId;

        //// Sprout Color
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[0].enabled = treeCollectionData.sproutColor1 == 1 ? true : false;
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[1].enabled = treeCollectionData.sproutColor2 == 1 ? true : false;
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[2].enabled = treeCollectionData.sproutColor3 == 1 ? true : false;
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[3].enabled = treeCollectionData.sproutColor4 == 1 ? true : false;
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[treeCollectionData.sproutGroupId - 1].sproutAreas[4].enabled = treeCollectionData.sproutColor5 == 1 ? true : false;

        //TreePipeLine_Collection treePipeline_Collection = treeCollectionData.treeGrowths[0].treePipeline;
        //// 1. Scale
        //float scaleTo = 1;
        //// 2. Branch Number
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minFrequency = treePipeline_Collection.branch1;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].maxFrequency = treePipeline_Collection.branch1;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].minFrequency = treePipeline_Collection.branch2;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].maxFrequency = treePipeline_Collection.branch2;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].minFrequency = treePipeline_Collection.branch3;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].maxFrequency = treePipeline_Collection.branch3;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].minFrequency = treePipeline_Collection.branch4;
        //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].maxFrequency = treePipeline_Collection.branch4;
        //// 4. Sprout Number
        //treePipeline._serializedPipeline.sproutGenerators[0].minFrequency = treePipeline_Collection.sproutNum;
        //treePipeline._serializedPipeline.sproutGenerators[0].maxFrequency = treePipeline_Collection.sproutNum;
        //// 5. Ratio of Rotten Sprout : 0, 25, 50, 75, 100
        //List<int> groupNum = new List<int>() { 5, 6, 7, 8 };
        //for (int j = 0; j < (treePipeline_Collection.rottenRate / 25); j++) // i < 0, 1, 2, 3, 4
        //{
        //    SproutSeed sproutGroup = new SproutSeed();
        //    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutGroup);
        //    int count = treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Count;
        //    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[count - 1].groupId = groupNum[j];
        //}
        //int c = treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Count;
        //// 6. Sprout Width
        //foreach (SproutMesh s in treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes)
        //{
        //    s.width = treePipeline_Collection.sproutWidth;
        //}
        //// 7. Gravity
        //for (int j = 0; j < 4; j++)
        //{
        //    if (!treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[j].isRoot)
        //    {
        //        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[j].minGravityAlignAtBase = treePipeline_Collection.gravity;
        //        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[j].maxGravityAlignAtBase = treePipeline_Collection.gravity;
        //    }
        //}

        //// Load Tree
        //Pipeline loadedPipeline = DataTemporary.assetBundleTreePipeline.LoadAsset<Pipeline>(pipeName);
        //treeFactory[createPos].LoadPipeline(loadedPipeline.Clone(), true);
        //treeFactory[createPos].UnloadAndClearPipeline();
        //treeFactory[createPos].transform.GetChild(1).localScale = new Vector3(scaleTo, scaleTo, scaleTo);
        //Resources.UnloadAsset(loadedPipeline);

        //if (treeFactory[createPos].transform.parent.childCount > 1)
        //{
        //    treeFactory[createPos].transform.parent.GetChild(1).gameObject.SetActive(true);
        //}
        //if (treeFactory[createPos].transform.parent.childCount > 2)
        //{
        //    treeFactory[createPos].transform.parent.GetChild(2).gameObject.SetActive(false);
        //}

        // soil 비활성화
        treeFactory[createPos].transform.parent.transform.GetChild(0).gameObject.SetActive(false);
        treeFactory[createPos].transform.parent.transform.GetChild(2).gameObject.SetActive(true);
        treeFactory[createPos].transform.parent.transform.GetChild(3).gameObject.SetActive(false);
        // Tree 활성화
        treeFactory[createPos].gameObject.SetActive(true);

        ChoiceRespawnPos.Instance.isComplete = true;
        ChoiceRespawnPos.Instance.isCompleteList[createPos] = true;
    }

    public void SettingChoicePos(int Pos)
    {
        createPos = Pos;
    }
}

