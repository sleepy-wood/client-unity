using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Playables;

using Broccoli.Factory;
using Broccoli.Pipe;
using Broccoli.Generator;
using System.IO;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Networking;

public class TreeController : MonoBehaviour
{
    #region Variable

    // Load할 Pipeline 이름
    string pipeName;

    // treeList


    // 방문 타입
    public enum VisitType
    {
        None,
        First,
        ReVisit
    }
    public VisitType visitType;

    // User가 선택한 Seed
    public SeedType selectedSeed;
    // Seed별 pipeName
    Dictionary<SeedType, string> pipeNameDict = new Dictionary<SeedType, string>()
    {
        { SeedType.Basic, "BasicTree"},
        { SeedType.Oak, "OakTree" },
        { SeedType.Sakura, "SakuraTree" }
    };

    // 선택된 treeGrowDatas
    List<TreeGrowData> selectedtreeGrowDatas;

    #region 기본 5일 성장 변수 저장소
    // Pipeline Element별 dayuency Min/Max값 저장소
    [System.Serializable]
    public class MinMax
    {
        public int min;
        public int max;
    }
    [System.Serializable]
    public class TreeGrowData
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
        Sakura
    }
    // 나무 종류별 관련 변수 클래스
    [System.Serializable]
    public class TreeStore
    {
        public SeedType seedType = SeedType.None;
        // DayCount에 따라 변하는 나무 관련 변수 저장소
        public List<TreeGrowData> treeGrowDatas = new List<TreeGrowData>();
    }
    // 나무 종류별 관련 변수 클래스의 모음 리스트
    public List<TreeStore> treeStores = new List<TreeStore>();
    #endregion

    // tree Factory
    public TreeFactory treeFactory = null;
    // The pipeline
    public Pipeline treePipeline;
    // 나무 자라는 위치
    public Transform growPos;
    // DayCount
    public int dayCount;
    // 씨앗 하강 속도
    public float downSpeed = 0.5f;
    // Daycount Text
    public Text txtDayCount;
    // sprout
    public GameObject sprout;
    //public GameObject sproutFactory;
    // seed
    public GameObject seed;
    //public GameObject seedFactory;
    // soil
    public GameObject soil;
    // FOV
    public float defaultFOV = 10.29f;
    public float targetFOV = 3.11f;
    // TreeData
    public TreeData data;
    // leafTexture
    public Texture2D leafText;
    // user
    public GameObject user;
    // previewTree Scale Value
    float scaleTo = 1;
    // AssetBundle
    AssetBundle assetBundle;
    // 나무 이름 입력 UI
    public GameObject treeNameUI;
    // 나무 이름
    public string treeName;
    #endregion


    void Start()
    {
        assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/newtreebundle");

        #region Build
        // Build mesh 오류 해결 코드
        //print(Application.dataPath);
        //string resPath = Application.dataPath + "/Resources/Tree/NewTreePipeline4.asset";
        //if (!File.Exists(resPath))
        //{
        //        path = Application.dataPath + "Tree/NewTreePipeline4.asset";
        //        byte[] data = File.ReadAllBytes(resPath);
        //        File.WriteAllBytes(path, data);
        //}

        //// treeFactory
        //treeFactory = TreeFactory.GetFactory();
        #endregion

        // TreeData 객체 생성
        data = new TreeData();

        // 방문 타입 결정
        if (visitType == VisitType.None) visitType = VisitType.First;
        else visitType = VisitType.ReVisit;

        // Test : 씨앗이 선택되었다면 그 씨앗에 맞는 pipeline name 저장
        if (selectedSeed != SeedType.None)
        {
            pipeName = pipeNameDict[selectedSeed];
        }
        FindtreeGrowDatas();

        // Tree Pipeline 로드
        treePipeline = assetBundle.LoadAsset<Pipeline>(pipeName);

        #region 기존 코드
        //pipeline = treeFactory.LoadPipeline(runtimePipelineResourcePath);
        //// pipeline에서 positioner 요소 가져오기(위치 동적 할당)
        //if (pipeline != null && pipeline.Validate())
        //{
        //        positionerElement = (PositionerElemeft)pipeline.root.GetDownstreamElement(PipelineElement.ClassType.Positioner);
        //        positionerElement.positions.Clear();
        //}
        #endregion
    }

    bool isOnce;
    bool isOnce2;
    public Texture2D barkTexture;
    void Update()
    {
        #region 썩은잎 만들기 Test
        //if (count == 1 && !isOnce)
        //{
        //    // sprout areas enabled true;
        //    SproutMap.SproutMapArea pipe0 = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[1];
        //    pipe0.enabled = true;
        //    Pipeline loadedPipeline = assetBundle.LoadAsset<Pipeline>("badTree");
        //    treeFactory.UnloadAndClearPipeline();
        //    treeFactory.LoadPipeline(loadedPipeline.Clone(), true);
        //    treeFactory.transform.GetChild(1).localScale = new Vector3(1, 1, 1);
        //    Resources.UnloadAsset(loadedPipeline);
        //    isOnce = true;
        //}
        //if (count == 2 && !isOnce2)
        //{
        //    SproutMap.SproutMapArea pipe01 = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[2];
        //    pipe01.enabled = true;
        //    pipe01.enabled = true;
        //    Pipeline loadedPipeline = assetBundle.LoadAsset<Pipeline>("badTree");
        //    treeFactory.UnloadAndClearPipeline();
        //    treeFactory.LoadPipeline(loadedPipeline.Clone(), true);
        //    treeFactory.transform.GetChild(1).localScale = new Vector3(1, 1, 1);
        //    Resources.UnloadAsset(loadedPipeline);
        //    isOnce2 = true;
        //}
        #endregion

        #region Camera Moving
        // 2. 작은 묘목
        //if (dayCount == 2)
        //{
        //    Camera.main.gameObject.transform.position = Vector3.Lerp(Camera.main.gameObject.transform.position, new Vector3(-0.4f, 3.59f, 9.09f), camMoveSpeed * Time.deltaTime);
        //}
        //// 3. 묘목
        //if (dayCount == 3)
        //{
        //    Camera.main.gameObject.transform.position = Vector3.Lerp(Camera.main.gameObject.transform.position, new Vector3(-0.4f, 4.67f, 11.43f), camMoveSpeed * Time.deltaTime);
        //}
        //// 4. 나무
        //if (dayCount == 4)
        //{
        //    Camera.main.gameObject.transform.position = Vector3.Lerp(Camera.main.gameObject.transform.position, new Vector3(0.44f, 6.89f, 20.19f), camMoveSpeed * Time.deltaTime);
        //}
        //// 5. 열매
        //if (dayCount == 5)
        //{
        //    Camera.main.gameObject.transform.position = Vector3.Lerp(Camera.main.gameObject.transform.position, new Vector3(0.36f, 8.6f, 26.07f), camMoveSpeed * Time.deltaTime);
        //}
        #endregion

        #region 가지 추가  Test Code
        // TreePipeline - 가지 추가
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    print("가지 추가");
        //    treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minFrequency = 20;
        //    treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.maxFrequency = 20;
        //    // Tree 다시 Load
        //    Debug.Log("LoadPipelineAsset");
        //    string pathToAsset = Application.streamingAssetsPath + "/TreePipeline.asset";
        //    Broccoli.Pipe.Pipeline loadedPipeline = treePipeline;
        //    treeFactory.UnloadAndClearPipeline();  // pipeline 초기화
        //    treeFactory.LoadPipeline(loadedPipeline.Clone(), pathToAsset, true, true);
        //    Resources.UnloadAsset(loadedPipeline);
        //    // 이전 Tree 삭제
        //    Destroy(growPos.GetChild(0).gameObject);
        //    // 새로 Load한 Tree 위치시키기
        //    //treeFactory.gameObject.transform.localPosition = new Vector3(0, 0, 0);
        //    //treeFactory.gameObject.transform.Rotate(new Vector3(0, 0, 0));
        //    treeFactory.gameObject.transform.parent = growPos;
        //}
        #endregion
    }

    #region 씨앗 심기 코루틴
    IEnumerator PlantSeed(float targetScale)
    {
        #region 카메라 줌인
        float t = 0;
        //while (t < 1)
        //{
        //        t += Time.deltaTime;
        //        Camera.main.fieldOfView = Mathf.Lerp(defaultFOV, targetFOV, t);
        //        yield return null;
        //}
        //Camera.main.fieldOfView = targetFOV;
        #endregion

        // 처음 심은 시간 저장
        //TimeManager.

        // 씨앗 심기
        seed.transform.localPosition = new Vector3(0, 2.5f, 0);
        seed.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        DestroyImmediate(seed, true);

        // 새싹 자라기
        t = 0;
        sprout.transform.localScale = new Vector3(0, 0, 0);
        sprout.SetActive(true);
        while (t <= targetScale)
        {
            t += Time.deltaTime * 0.5f;
            sprout.transform.localScale = new Vector3(t, t, t);
            yield return null;

        }
        sprout.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        yield return new WaitForSeconds(1);

        #region 카메라 줌 아웃
        //t = 0;
        //while (t < 1)
        //{
        //        t += Time.deltaTime * 0.5f;
        //        Camera.main.fieldOfView = Mathf.Lerp(targetFOV, defaultFOV, t);
        //        yield return null;
        //}
        //Camera.main.fieldOfView = defaultFOV;
        #endregion

        // 식물 이름 UI 띄우기
        treeNameUI.gameObject.SetActive(true);
    }
    #endregion

    /// <summary>
    /// 나무 Pipeline 업데이트
    /// "dayMinMax" > Element Frequency
    /// "rootFreq" > Root Min/Max Freqency
    /// "rootBaseLength" > Min/Max Length At Base
    /// "girthBase" > Min/Max Girth At Base
    /// "scale" > Object scale
    /// </summary>
    /// <param name="dayCount"></param>
    public void PipelineUpdate(int dayCount)
    {
        // 날짜에 맞춘 성장 데이터 정보 지닌 요소
        TreeGrowData element = selectedtreeGrowDatas[dayCount - 2];

        #region 1. Element MinMax
        int idx = element.minMaxList.Count;
        // pipeline element 개수만큼 설정
        for (int i = 0; i < idx; i++)
        {
            // pipeline
            StructureGenerator.StructureLevel pipe1 = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
            // 저장값
            MinMax store1 = element.minMaxList[i];

            pipe1.minFrequency = store1.min;
            pipe1.maxFrequency = store1.max;
        }
        #endregion

        #region 2. Root Min/Max Freqency
        StructureGenerator.StructureLevel pipe2 = treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel;
        int store2 = element.rootFreq;

        // Root Min Freqency
        pipe2.minFrequency = store2;
        // Root Max Freqency
        pipe2.maxFrequency = store2;
        #endregion

        #region 3. Min/Max Length At Base
        StructureGenerator.StructureLevel pipe3 = treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel;
        int store3 = element.rootBaseLength;

        // Root Min Length At Base
        pipe3.minLengthAtBase = store3;
        // Root Max Length At Base
        pipe3.maxLengthAtBase = store3;
        #endregion

        #region 4. Min/Max Girth At Base
        GirthTransformElement pipe4 = treePipeline._serializedPipeline.girthTransforms[0];
        float store4 = element.girthBase;

        // Min Girth At Base
        pipe4.minGirthAtBase = store4;
        // Max Girth At Base
        pipe4.maxGirthAtBase = store4;
        #endregion

        #region 5. Object scale
        scaleTo = element.scale;
        print($"object scale : {scaleTo}");
        #endregion
    }


    /// <summary>
    /// treeStores에서 씨앗 종류에 맞는 treeGrowDatas 찾는 함수
    /// </summary>
    public void FindtreeGrowDatas()
    {
        for (int i = 0; i < treeStores.Count; i++)
        {
            if (treeStores[i].seedType == selectedSeed)
            {
                selectedtreeGrowDatas = treeStores[i].treeGrowDatas;
            }
        }

    }


    /// <summary>
    /// 업데이트한 나무 정보를 기반으로 나무 다시 로드
    /// </summary>
    public void TreeReload()
    {
        Pipeline loadedPipeline = assetBundle.LoadAsset<Pipeline>(pipeName);
        treeFactory.LoadPipeline(loadedPipeline.Clone(), true);
        treeFactory.UnloadAndClearPipeline();
        treeFactory.transform.GetChild(1).localScale = new Vector3(scaleTo, scaleTo, scaleTo);
        Resources.UnloadAsset(loadedPipeline);
    }


    /// <summary>
    /// dayCount에 맞게 Tree 업데이트
    /// </summary>
    public float camMoveSpeed = 0.5f;
    Transform campos;
    int count = 0;
    public void LoadTree()
    {
        // 1일차
        if (dayCount == 1)
        {
            StartCoroutine(PlantSeed(0.5f));
            seed.SetActive(true);
            //TreeReload();
            // 나무 심은 시간 저장
            GameManager.Instance.timeManager.firstPlantDate = DateTime.Now;
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
        }
        // 2일차
        if (dayCount == 2)
        {
            sprout.SetActive(false);
            soil.SetActive(false);
            PipelineUpdate(dayCount);
            TreeReload();
            treeFactory.gameObject.SetActive(true);
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
        }
        // 3일차
        if (dayCount == 3)
        {
            PipelineUpdate(dayCount);
            TreeReload();
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
            campos = Camera.main.gameObject.transform;
        }
        // 4일차
        if (dayCount == 4)
        {
            PipelineUpdate(dayCount);
            TreeReload();
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
        }
        // 5일차
        if (dayCount == 5)
        {
            PipelineUpdate(dayCount);
            TreeReload();
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
            assetBundle.Unload(false);
        }
    }

    /// <summary>
    ///  TimeManager로부터 신호받아 TreeUpdate
    /// </summary>
    public void TreeUpdate()
    {
        dayCount++;
        LoadTree();
    }

    /// <summary>
    /// 현재 User의 Tree Data 저장
    /// </summary>
    public void SaveTreeData()
    {
        TreeData treeData = new TreeData();

        // seed Number
        treeData.seedNumber = treePipeline.seed;
        // Tree Name
        treeData.treeName = treeName;
        // First Plant Day
        treeData.firstPlantDate = GameManager.Instance.timeManager.firstPlantDate;

        // Tree Pipeline Data
        TreePipelineData pipeData = new TreePipelineData();
        #region Tree Pipeline Data
        // Tree Height
        pipeData.baseLength = treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minLengthAtBase;
        // Branch Num
        int levels = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels.Count;
        for (int i = 0; i < levels-1; i++)
        {
            pipeData.branchNums[i] = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].minFrequency;
        }
        // Sprout Num
        pipeData.sproutNum = treePipeline._serializedPipeline.sproutGenerators[0].minFrequency;
        // Thickness
        pipeData.thickness = treePipeline._serializedPipeline.girthTransforms[0].minGirthAtBase;
        // Tree Bending (Noise)
        pipeData.bending = treePipeline._serializedPipeline.branchBenders[0].noiseScaleAtBase;
        // Gravity
        pipeData.gravity = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minGravityAlignAtTop;
        // Root Num
        int idx = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels.Count;
        pipeData.rootNum = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[idx - 1].minFrequency;
        // Bark Texture
        pipeData.barkTexture = treePipeline._serializedPipeline.barkMappers[0].mainTexture;
        // Sprout Enabled
        int n = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps.Count;
        for (int i=0; i<n; i++)
        {
            pipeData.sproutEnabled[i] = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled;
        }
        #endregion
        // Save Pipe Data
        treeData.treePipelineData = pipeData;

        // Land ID
        //treeData.landID = 

    }

    /// <summary>
    /// Tree Data Load하는 함수
    /// </summary>
    public void LoadTreeData()
    {

    }
}
