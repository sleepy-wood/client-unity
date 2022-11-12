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

    #region 방문 타입
    public enum VisitType
    {
        None,
        First,
        ReVisit
    }
    public VisitType visitType;
    #endregion

    // 랜덤으로 선택된 SeedType
    public SeedType selectedSeed = SeedType.None;
    // pipeNameList
    List<string> pipeNameList = new List<string>() { "BasicTree", "OakTree", "SakuraTree", "DRTree", "DemoTree_Red" };
    // pipeName별 SeedType
    Dictionary<string, SeedType> pipeNameDict = new Dictionary<string, SeedType>()
    {
        { "BasicTree", SeedType.Basic },
        { "OakTree", SeedType.Oak },
        { "SakuraTree", SeedType.Sakura },
        { "DRTree", SeedType.DR },
        { "DemoTree_Red", SeedType.Demo }
    };
    // 선택된 TreeSetting List
    List<TreeSetting> selectedTreeSetting;

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
    // sprout Leaf
    public GameObject sproutLeaf;
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
    public float scaleTo = 1;
    // AssetBundle
    AssetBundle assetBundle;
    // 나무 이름 입력 UI
    public GameObject treeNameUI;
    // 나무 이름
    public string treeName;
    // 현재 나무의 DB id
    public int dbId;
    // Demo Mode
    public bool demoMode;
    #endregion

    #region 가상 수면 데이터 변수
    //// 총 수면 시간
    //public enum SleepAmount
    //{
    //    Zero,
    //    VeryInadequateBad,
    //    Inadequate,
    //    AdequateGood,
    //    Excessive,
    //}
    //public SleepAmount sleepAmount;

    //// 기상 시간의 오차
    //public enum SleepRiseTimeVariance
    //{
    //    SmallGood,
    //    LargeBad,
    //}
    //public SleepRiseTimeVariance sleepRiseTimeVariance;

    //// daytime에 낮잠 여부
    //public enum SleepyDayTimeNap
    //{
    //    YesBad,
    //    NoGood,
    //}
    //public SleepyDayTimeNap sleepyDayTimeNap;

    //// Activity 목표 달성 확률
    //public float activityPercent;

    #endregion

    void Start()
    {
        treeFactory.gameObject.SetActive(false);
        assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundles/newtreebundle");

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

        // 방문 타입에 따라 시나리오 선택
        //if (visitType == VisitType.First)
        //{
        // 나무 형태 Random 선택
        if (demoMode)
        {
            pipeName = "DemoTree_Red";
            selectedSeed = SeedType.Demo;
        }
        else
        {
            int i = UnityEngine.Random.Range(0, pipeNameList.Count-1);  //Demo Tree 빼고 랜덤 선택
            pipeName = pipeNameList[i];
            selectedSeed = pipeNameDict[pipeName];
        }
        print(pipeName + " Selected");
        treePipeline = assetBundle.LoadAsset<Pipeline>(pipeName);



        // sprout Texture 랜덤 선택
        if (!demoMode)
        {
            int s = UnityEngine.Random.Range(0, 4);
            treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[s].enabled = true;
            print("sprout texture index : " + s);
            // bark Texture 랜덤 선택
            List<string> name = new List<string>() { "A", "B", "C", "D", "E" };
            int b = UnityEngine.Random.Range(0, 5);
            Texture2D texture = Resources.Load("Tree/Sprites/Tree_Bark_" + name[b]) as Texture2D;
            treePipeline._serializedPipeline.barkMappers[0].mainTexture = texture;
        }
        
        // 랜덤 선택된 Seed로 기본 세팅값 찾기
        FindtreeSetting();

        // Pipeline 기본 세팅
        PipelineSetting(2);
        //}
        //else if (visitType == VisitType.ReVisit)
        //{
        //    // DB에 저장해놓은 나무 변수 가져와 로드
        //}

        // 헬스데이터 불러오기 ( 로딩바에서 )
        HealthDataStore.Init();

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
    bool isOnce3;
    bool once = false;
    bool once2 = false;
    bool once3 = false;
    public float camMoveSpeed = 1f;
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
        if (dayCount == 2)
        {
            Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.7f, -19.7f, 3.2f), 2 * Time.deltaTime);
        }
        // 3. 묘목
        if (dayCount == 3)
        {
            Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.7f, -29.8f, 2.6f), 2 * Time.deltaTime);
        }
        // 4. 나무
        if (dayCount == 4)
        {
            Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.9f, -44.2f, 6.2f), 2 * Time.deltaTime);
        }
        // 5. 열매
        if (dayCount == 5)
        {
            Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.2f, -54.7f, 9.1f), 2 * Time.deltaTime);
        }
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

        #region 자연스러운 나무 성장 Test
        //if (Input.GetKeyDown(KeyCode.Alpha5) && !isOnce3)
        //{
        //    isOnce3 = true;
        //    print("55555555555");
        //    treeFactory.gameObject.SetActive(true);
        //    for (int j=0; j< 10; j++)
        //    {
        //        for (int i = 0; i < 4; i++)
        //        {
        //            StructureGenerator.StructureLevel branchPipe = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
        //            branchPipe.minFrequency += 1;
        //            branchPipe.maxFrequency += 1;
        //            TreeReload();
        //            print(i);
        //        }
        //    }
        //}
        #endregion

        if (HealthDataStore.GetStatus() == HealthDataStoreStatus.Loaded && !once)
        {
            once = true;
            Debug.Log("SleepSamples: " + HealthDataStore.SleepSamples.Length);
            Debug.Log("ActivitySamples: " + HealthDataStore.ActivitySamples.Length);
            // 올해 10월 날짜
            HealthReport report = HealthDataAnalyzer.GetDailyReport(
                new DateTime(2022, 10, 06, 17, 0, 0, 0, DateTimeKind.Local),
                6
            );
            Debug.Log(JsonUtility.ToJson(report, true));
        }
        //if (!GameManager.Instance.timeManager.enabled)
        //{
        //    GameManager.Instance.timeManager.enabled = true;
        //}
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

        // 씨앗 심기
        //seed.transform.localPosition = new Vector3(0, 2.5f, 0);
        seed.SetActive(true);
        yield return new WaitForSeconds(2);
        sproutParticle.Play();
        sprout.transform.localScale = new Vector3(0f, 0f, 0f);
        sprout.SetActive(true);
        

        t = 0.4f;
        while (t <= 0.6f)
        {
            t += Time.deltaTime * 0.5f;
            sprout.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            yield return null;
        }
        sprout.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        


        // 새싹잎 자라기
        t = 0;
        sproutLeaf.transform.localScale = new Vector3(0, 0, 0);
        while (t <= targetScale)
        {
            t += Time.deltaTime * 0.5f;
            sproutLeaf.transform.localScale = new Vector3(t, t, t);
            yield return null;
        }
        sproutLeaf.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        sproutParticle.Stop();
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
        if (!demoMode) treeNameUI.gameObject.SetActive(true);
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
    public void PipelineSetting(int day)
    {
        // 기본 세팅 성장 데이터 정보 지닌 요소
        TreeSetting element = selectedTreeSetting[day - 2];

        if (demoMode)
        {
            #region 1. Branch Element MinMax Frequency
            for (int i = 0; i < 4; i++)
            {
                // pipeline
                StructureGenerator.StructureLevel pipe1 = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                // 저장값
                MinMax store1 = element.minMaxList[i];

                pipe1.minFrequency = store1.min;
                pipe1.maxFrequency = store1.max;
            }
            #endregion

            #region 2. Min/Max Girth At Base
            GirthTransformElement pipe4 = treePipeline._serializedPipeline.girthTransforms[0];
            float store4 = element.girthBase;

            // Min Girth At Base
            pipe4.minGirthAtBase = store4;
            // Max Girth At Base
            pipe4.maxGirthAtBase = store4;
            #endregion

            #region 3. Object scale
            scaleTo = element.scale;
            #endregion
        }
        else
        {
            #region 1. Branch Element MinMax Frequency
            //int idx = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels.Count;
            // pipeline element 개수만큼 설정
            for (int i = 0; i < 4; i++)
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
            //GirthTransformElement pipe4 = treePipeline._serializedPipeline.girthTransforms[0];
            //float store4 = element.girthBase;

            //// Min Girth At Base
            //pipe4.minGirthAtBase = store4;
            //// Max Girth At Base
            //pipe4.maxGirthAtBase = store4;
            #endregion

            #region 5. Object scale
            scaleTo = element.scale;
            #endregion
        }

    }


    /// <summary>
    /// treeStores에서 씨앗 종류에 맞는 Tree Settings 찾는 함수
    /// </summary>
    public void FindtreeSetting()
    {
        for (int i = 0; i < treeStores.Count; i++)
        {
            if (treeStores[i].seedType == selectedSeed)
            {
                selectedTreeSetting = treeStores[i].treeSettings;
                print($"기본 세팅 : {treeStores[i].seedType}");
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
        if (!demoMode)
        {
            treeFactory.transform.GetChild(1).localScale = new Vector3(scaleTo, scaleTo, scaleTo);
        }
        else
        {
            treeFactory.transform.GetChild(0).localScale = new Vector3(scaleTo, scaleTo, scaleTo);
        }
        
        Resources.UnloadAsset(loadedPipeline);
    }


    /// <summary>
    /// dayCount에 맞게 Tree 업데이트
    /// </summary>
    
    Transform campos;
    public void LoadTree(int day)
    {
        // 씨앗 심기
        if (day == 1)
        {
            StartCoroutine(PlantSeed(1.02f));
            // 나무 심은 시간 저장
            //GameManager.Instance.timeManager.firstPlantDate = DateTime.Now;
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
            //SaveTreeData();
            TreeReload();
        }
        // 2일차
        else if (day == 2)
        {
            sprout.SetActive(false);
            soil.SetActive(false);
            // 사용자 데이터로 나무 변화
            //SleepAmountToTree(sleepAmount);
            //SleepRiseToTree(sleepRiseTimeVariance);
            //NapToTree(sleepyDayTimeNap);
            
            //불러오기
            TreeReload();
            treeFactory.gameObject.SetActive(true);
            //ScaleChange(activityPercent);
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
            //SaveTreeData();
        }
        // 3일차
        else if (day == 3)
        {
            PipelineSetting(3);
            TreeReload();
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
            campos = Camera.main.gameObject.transform;
            //SaveTreeData();
        }
        // 4일차
        else if (day == 4)
        {
            PipelineSetting(4);
            TreeReload();
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
            //SaveTreeData();
        }
        // 5일차
        else if (day == 5)
        {
            PipelineSetting(5);
            TreeReload();
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
            assetBundle.Unload(false);
            //SaveTreeData();
        }
    }

    /// <summary>
    /// 데모할 때의 LoadTree 함수
    /// </summary>
    /// <param name="day"></param>
    /// <param name="demo"></param>
    
    public ChangeSky sky;
    public Transform fire;
    public ParticleSystem sproutParticle;
    public ParticleSystem changeParticle;
    public ParticleSystem snow;
    public ParticleSystem rain;

    public void LoadTree(int day, bool demo)
    {
        // 1일차 (씨앗 심기)
        if (day == 1)
        {
            StartCoroutine(PlantSeed(1.02f));
            // 나무 심은 시간 저장
            //GameManager.Instance.timeManager.firstPlantDate = DateTime.Now;
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
            TreeReload();
        }
        // 2일차
        else if (day == 2)
        {
            seed.SetActive(false);
            sprout.SetActive(false);
            soil.SetActive(false);
            // Sprout
            SproutNumChange(true, 10);
            // 트리 기본 세팅 값 로드
            TreeReload();
            treeFactory.gameObject.SetActive(true);
            // Change Particle
            changeParticle.Play();
            // 조금 뒤에 비내리게 하기
            StartCoroutine(Delay(25));
            // Weather - Rain
            rain.Play();
        }
        // 3일차
        else if (day == 3)
        {
            rain.Stop();
            // SkyBox
            sky.Sunset();
            // Fire
            fire.GetChild(0).gameObject.SetActive(true);
            fire.GetChild(1).gameObject.SetActive(true);
            // Sprout
            SproutNumChange(true, 5);
            // Tree Pipeline - Branch MinMax, Girth, Scale
            PipelineSetting(3);
            TreeReload();
            // Change Particle
            changeParticle.Play();


        }
        // 4일차
        else if (day == 4)
        {
            // SkyBox
            sky.Night();
            // Sprout
            SproutNumChange(true, 10);
            // Tree Pipeline - Branch MinMax, Girth, Scale
            PipelineSetting(4);
            TreeReload();
            // Weather - Snow
            snow.Play();
            // Change Particle
            changeParticle.transform.position = new Vector3(0, 6.65f, 0);
            changeParticle.Play();

        }
        // 5일차
        else if (day == 5)
        {
            snow.Stop();
            // SkyBox
            sky.Day();
            // Fire
            fire.GetChild(0).gameObject.SetActive(false);
            fire.GetChild(1).gameObject.SetActive(false);
            // Sprout
            SproutNumChange(true, 10);
            // Tree Pipeline - Branch MinMax, Girth, Scale
            PipelineSetting(5);
            TreeReload();
            // Change Particle
            changeParticle.transform.position = new Vector3(0, 10.46f, 0);
            changeParticle.Play();
            assetBundle.Unload(false);
        }
    }
    IEnumerator Delay(float second)
    {
        yield return new WaitForSeconds(second);
    }

    #region DB
    /// <summary>
    /// 일차 수 별로 User의 Tree Data 저장
    /// </summary>
    public Transform previewTree;
    public async void SaveTreeData()
    {
        List<TreeData> treeDatas = new List<TreeData>();
        TreeData treeData = new TreeData();

        // Tree Name
        treeData.treeName = treeName;
        // seed Number
        treeData.seedNumber = treePipeline.seed;
        // Seed Type
        treeData.seedType = selectedSeed.ToString();
        // Land ID
        treeData.landId = 3;  // 변경 필요

        // Tree Pipeline Data //
        // 1. Scale
        if (previewTree == null) previewTree = GameObject.Find("previewTree").transform;
        treeData.scale = previewTree.localScale.x;
        // 2. Branch Numbers
        List<StructureGenerator.StructureLevel> level = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels;
        treeData.branch1 = level[0].minFrequency;
        treeData.branch2 = level[1].minFrequency;
        treeData.branch3 = level[2].minFrequency;
        treeData.branch4 = level[3].minFrequency;
        // 3. Trunk Length
        treeData.trunkLength = treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minLengthAtBase;
        // 4. Sprout Number
        treeData.sproutNum = treePipeline._serializedPipeline.sproutGenerators[0].minFrequency;
        // 5. Rotten Rate
        treeData.rottenRate = 20;
        // 6. Gravity
        treeData.gravity = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minGravityAlignAtTop;
        // 7. Root Num                                                                                                                                                                                                                                          -000000000000000000
        int idx = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels.Count;
        treeData.rootNum = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[idx - 1].minFrequency;
        // 8. Bark Texture Name
        treeData.barkTexture = treePipeline._serializedPipeline.barkMappers[0].mainTexture.name;
        // 9. Sprout Index
        int id = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas.Count;
        for (int i = 0; i < id; i++)
        {
            if (treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled)
            {
                treeData.sproutIndex = i;
            }
        }

        string treeJsonData = JsonUtility.ToJson(treeData);

        // Web
        ResultPost<TreeData> resultPost = await DataModule.WebRequestBuffer<ResultPost<TreeData>>(
            "/api/v1/trees",
            DataModule.NetworkType.POST,
            DataModule.DataType.BUFFER,
            treeJsonData);

        if (!resultPost.result)
        {
            Debug.LogError("WebRequestError : NetworkType[Post]");
        }
        else
        {
            Debug.Log("Tree Save 성공");
        }
        treeDatas.Add(treeData);

        treeDatas.Add(treeData);
        ArrayTreeData arrayTreeData = new ArrayTreeData();
        arrayTreeData.treeDataList = treeDatas;

        // DataTemporary
        DataTemporary.MyTreeData = arrayTreeData;

        // File 형식으로 Update or Save
        FileManager.SaveDataFile("TreeData", arrayTreeData);
    }

    /// <summary>
    /// 내 나무 목록 가져오기
    /// </summary>
    public Text txtTreeName;
    public int rotten;
    //public async void LoadTreeData()
    //{
    //    ArrayTreeData arrayTreeData = DataTemporary.MyTreeData;
    //    assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/newtreebundle");

    //    // 기본 세팅값 찾기
    //    FindtreeSetting();

    //    // Tree Pipeline 로드
    //    TreeData treeData = arrayTreeData.treeDataList[0];
    //    pipeName = treeData.seedType;
    //    treePipeline = assetBundle.LoadAsset<Pipeline>(pipeName);
    //    // Pipeline 기본 세팅
    //    //PipelineSetting();

    //    // seed Number
    //    treePipeline.seed = treeData.seedNumber;
    //    // Tree Name
    //    txtTreeName.text = treeData.treeName;
    //    // seedType
    //    selectedSeed = (SeedType)System.Enum.Parse(typeof(SeedType), treeData.seedType);
    //    // First Plant Date
    //    //GameManager.Instance.timeManager.firstPlantDate = treeData.createdAt;
    //    // Tree Pipeline Data
    //    //TreePipelineData pipeData = treeData.treePipelineData;
    //    //#region Pipeline Data
    //    //// 1. Scale
    //    //float p = pipeData.scale;
    //    //previewTree.localScale = new Vector3(p, p, p);
    //    //// 2. Branch Number
    //    //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minFrequency = pipeData.branch1;
    //    //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].maxFrequency = pipeData.branch1;
    //    //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].minFrequency = pipeData.branch2;
    //    //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].maxFrequency = pipeData.branch2;
    //    //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].minFrequency = pipeData.branch3;
    //    //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].maxFrequency = pipeData.branch3;
    //    //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].minFrequency = pipeData.branch4;
    //    //treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].maxFrequency = pipeData.branch4;
    //    //// 3. Trunk Length
    //    //treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.maxLengthAtBase = pipeData.trunkLength;
    //    //treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minLengthAtBase = pipeData.trunkLength;
    //    //// 4. Sprout Number
    //    //treePipeline._serializedPipeline.sproutGenerators[0].minFrequency = pipeData.sproutNum;
    //    //treePipeline._serializedPipeline.sproutGenerators[0].maxFrequency = pipeData.sproutNum;
    //    //// 5. Ratio of Rotten Sprout

    //    //// 6. Gravity
    //    //for (int i = 0; i < 4; i++)
    //    //{
    //    //    if(!treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].isRoot)
    //    //    {
    //    //        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].minGravityAlignAtBase = pipeData.gravity;
    //    //        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].maxGravityAlignAtBase = pipeData.gravity;
    //    //    }
    //    //}
    //    //// 7. Root Number
    //    //treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minFrequency = pipeData.rootNum;
    //    //treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.maxFrequency = pipeData.rootNum;
    //    //// 8. Bark Texture
    //    //string name = pipeData.barkTexture;
    //    //Texture2D texture = Resources.Load("Tree/Sprites/" + name) as Texture2D;
    //    //treePipeline._serializedPipeline.barkMappers[0].mainTexture = texture;
    //    //// 9. Sprout Texture
    //    //for (int i=0; i < 5; i++)
    //    //{
    //    //    if (i == pipeData.sproutIndex)
    //    //    {
    //    //        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled = true;
    //    //    }
    //    //    else
    //    //    {
    //    //        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled = false;
    //    //    }
    //    //}
    //    //#endregion

    //}
    #endregion

    #region User Data
    /// <summary>
    /// 수면양 타입에 따른 나무 데이터 변경
    /// </summary>
    public void SleepAmountToTree(SleepAmount sleep)
    {
        if (sleep == SleepAmount.Zero)
        {
            BadChange(true);
        }
        else if (sleep == SleepAmount.VeryInadequateBad)
        {
            BranchNumChange(false, 2);
        }
        else if (sleep == SleepAmount.Inadequate)
        {
            BranchNumChange(false, 1);
        }
        else if (sleep == SleepAmount.AdequateGood)
        {
            BranchNumChange(true, 1);
        }
        else if (sleep == SleepAmount.Excessive)
        {
            BranchNumChange(true, 2);
        }
    }
    /// <summary>
    /// 기상 시간 오차에 따른 나무 데이터 변경
    /// </summary>
    public void SleepRiseToTree(SleepRiseTimeVariance riseTime)
    {
        if (riseTime == SleepRiseTimeVariance.LargeBad)
        {
            SproutNumChange(false, 10);
        }
        else if (riseTime == SleepRiseTimeVariance.SmallGood)
        {
            SproutNumChange(true, 10);
        }
    }
    /// <summary>
    /// Daytime 낮잠 여부에 따른 나무 데이터 변경
    /// </summary>
    //public void NapToTree (SleepyDayTimeNap nap)
    //{
    //    if (nap == SleepyDayTimeNap.NoGood)
    //    {
    //        BadChange(false);
    //    }
    //    else if (nap == SleepyDayTimeNap.YesBad)
    //    {
    //        BadChange(true);
    //    }
    //}
    /// <summary>
    /// Activity 달성 퍼센트에 따라 나무 Scale 조절하는 함수
    /// </summary>
    /// <param name="activityRate"> Activity 달성 퍼센트 </param>
    public void ScaleChange(float activityRate, float lerpSpeed = 2f)
    {
        if (selectedSeed == SeedType.Oak)
        {
            Vector3 newScale = new Vector3(previewTree.localScale.x + 0.8f, previewTree.localScale.y + 0.8f, previewTree.localScale.z + 0.8f);
            previewTree.localScale = Vector3.Lerp(previewTree.localScale, newScale, lerpSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 newScale = new Vector3(previewTree.localScale.x + 0.18f, previewTree.localScale.y + 0.18f, previewTree.localScale.z + 0.18f);
            previewTree.localScale = Vector3.Lerp(previewTree.localScale, newScale, lerpSpeed * Time.deltaTime);
        }
    }


    /// <summary>
    /// 나무가지 개수 조절하는 함수
    /// </summary>
    /// <param name="addBranch"> 나무가지를 추가하는지 빼는지 </param>
    /// <param name="branchNum"> 나무가지 몇개를 추가할 것인지 </param>
    public void BranchNumChange(bool addBranch, int branchNum)
    {
        // + 나무가지 
        if (addBranch)
        {
            for (int i = 0; i < 4; i++)
            {
                StructureGenerator.StructureLevel branchPipe = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                branchPipe.minFrequency += branchNum;
                branchPipe.maxFrequency += branchNum;
            }
        }
        // - 나무가지 
        else
        {
            for (int i = 0; i < 4; i++)
            {
                StructureGenerator.StructureLevel branchPipe = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                branchPipe.minFrequency -= branchNum;
                branchPipe.maxFrequency -= branchNum;
            }
        }
    }
    /// <summary>
    /// 나뭇잎 개수 조절하는 함수
    /// </summary>
    /// <param name="addSprout"> 나뭇잎 개수 더할 것인지, 뺄 것인지 </param>
    public void SproutNumChange(bool addSprout, int branchNum)
    {
        if (addSprout)
        {
            treePipeline._serializedPipeline.sproutGenerators[0].minFrequency += branchNum;
            treePipeline._serializedPipeline.sproutGenerators[0].maxFrequency += branchNum;
        }
        else
        {
            treePipeline._serializedPipeline.sproutGenerators[0].minFrequency -= branchNum;
            treePipeline._serializedPipeline.sproutGenerators[0].maxFrequency -= branchNum;
        }
    }
    /// <summary>
    /// 나무의 상한잎, 중력 조절하는 함수
    /// </summary>
    /// <param name="yesBad"> 나쁜 영향을 줄 것인지, 나쁜 영향을 완화시킬 것인지 </param>
    public void BadChange(bool yesBad)
    {
        // 나쁜 영향을 줄 경우
        if (yesBad)
        {
            // 상한 잎
            int sproutTextures = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas.Count;
            for (int i = 5; i < sproutTextures; i++)
            {
                if (!treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled)
                {
                    treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled = true;
                    break;
                }
            }
            // 가지 중력 (상태 안 좋을수록 마이너스)
            for (int i = 0; i < 4; i++)
            {
                StructureGenerator.StructureLevel gravityPipe = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                gravityPipe.minGravityAlignAtTop -= 0.1f;
                gravityPipe.maxGravityAlignAtTop -= 0.1f;
            }
        }
        // 나쁜 영향을 완화시킬 경우
        else
        {
            // 상한 잎
            int sproutTextures = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas.Count;
            for (int i = 5; i < sproutTextures; i++)
            {
                if (treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled)
                {
                    treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled = false;
                    break;
                }
            }
            // 가지 중력 (상태 좋을수록 플러스)
            for (int i = 0; i < 4; i++)
            {
                StructureGenerator.StructureLevel gravityPipe = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                gravityPipe.minGravityAlignAtTop += 0.1f;
                gravityPipe.maxGravityAlignAtTop += 0.1f;
            }
        }

    }
    #endregion
}
