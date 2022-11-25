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
using System.Threading;



public class TreeController : MonoBehaviour
{
    #region Variable

    [Header("Mode")]
    // Play Mode - Good Grow
    public bool playMode;
    public int playSproutGroupId;  // 1 ~ 4
    int n;  // SproutEnabled에 쓰이는 랜덤 변수 
    public int playSproutEnabledNum; // 1 ~ 5
    public int playBarkIdx; // General : 0~4, Special : 5~8
    // Play Mode - Bad Grow
    public bool badMode;
    // Demo Mode
    public bool demoMode;

    [Space]

    [Header("Tree")]
    // 나무 이름
    public string treeName;
    // 현재 나무의 id
    public int treeId;
    // (여러 트리 데이터 중) 현재 트리 데이터의 인덱스
    public int treeDataIdx;
    // Load할 Pipeline 이름
    public string pipeName;
    // 랜덤으로 선택된 SeedType
    public SeedType selectedSeed = SeedType.None;
    // Tree Bark Material Name
    public string barkMaterial;
    // Sprout Group Id
    public int sproutGroupId;
    // 희귀성 점수
    public int rarityScore;
    // 생명력 점수
    public int vitalityScore;
    
    [Space]

    [Header("Tree Grow")]
    // tree Factory
    public TreeFactory treeFactory = null;
    // The pipeline
    public Pipeline treePipeline;
    // pipeNameList
    List<string> pipeNameList = new List<string>() { "BasicTree", "OakTree", "SakuraTree", "DRTree", "DemoTree_Red" };
    // pipeName별 SeedType
    public Dictionary<string, SeedType> pipeNameDict = new Dictionary<string, SeedType>()
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

    [Space]

    [Header("User")]
    public GameObject user;
    public VisitType visitType;
    public enum VisitType
    {
        None,
        First,
        ReVisit
    }

    [Space]

    [Header("Time")]
    // DayCount
    public int dayCount;

    [Space]

    [Header("Land Objects")]
    public Transform previewTree;
    // 나무 자라는 위치
    public Transform growPos;
    // sprout
    public GameObject sprout;
    // sprout Leaf
    public GameObject sproutLeaf;
    // seed
    public GameObject seed;
    // soil
    public GameObject soil;
    // previewTree Scale Value
    public float scaleTo = 1;
    // Tree Trunk plant
    public GameObject plant;

    [Space]

    [Header("Data")]
    // 로드해야하는 나무의 데이터
    public GetTreeData currentTreeData;
    // AssetBundle
    AssetBundle assetBundle;
    // User의 HealthData
    HealthReport report;
    // Bark Texture AssetBundle
    AssetBundle barkAssetBundle;


    [Space]

    [Header("UI")]
    public UI_Initial uiInitial;
    public GameObject landCanvas;
    public GameObject chatCanvas;
    // 나무 이름 입력 창UI
    public GameObject treeNameWindow;
    public GameObject treeNameBG;
    // SkyLand Main Text
    public Text txtMain;
    public Text txtSub;
    // My Collection Text
    public Text txtTreeName;
    public Text txtTreeBirth;
    public Text txtTreeRarity;
    public Text txtTreeVitality;

    [Space]

    [Header("Camera")]
    public ScreenShot screenShot;
    public ScreenRecorder screenRecorder;
    #endregion

    int i;
    private void Start()
    {
        treeFactory.gameObject.SetActive(false);
        assetBundle = DataTemporary.assetBundleTreePipeline;
        barkAssetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundles/barktexturebundle");


        // 방문 타입에 따라 다른 시나리오 구현
        if (visitType == VisitType.First)
        {
            // Mode에 따른 Pipeline 선택
            //if (playMode)
            //{
            //    pipeName = "BasicTree";
            //    selectedSeed = SeedType.Demo;
            //    treePipeline = assetBundle.LoadAsset<Pipeline>(pipeName);
            //}
            if (demoMode)
            {
                pipeName = "DemoTree_Cherry";
                treePipeline = assetBundle.LoadAsset<Pipeline>(pipeName);
            }
            else
            {
                // Pipeline 랜덤 선택 ( Tree Shape )
                i = UnityEngine.Random.Range(0, pipeNameList.Count - 1);  //Demo 제외
                pipeName = pipeNameList[2];
                selectedSeed = pipeNameDict[pipeName];
                treePipeline = assetBundle.LoadAsset<Pipeline>(pipeName);


                // Sprout Texture 확률적 랜덤 선택
                // 1. Leaf Shape Group(A, B, C, D) 4개 중 랜덤 선택해서 해당 Group을 Sprout Generator - Sprout Seeds에 추가
                if (!playMode)
                {
                    sproutGroupId = UnityEngine.Random.Range(1, 5);
                    SproutSeed sproutSeed = new SproutSeed();
                    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
                    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[0].groupId = sproutGroupId;
                }
                else if (playMode)
                {
                    sproutGroupId = playSproutGroupId;
                    SproutSeed sproutSeed = new SproutSeed();
                    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
                    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[0].groupId = playSproutGroupId;
                }


                // 2. 해당 Group 안의 Textures Area Enabled 개수 확률적으로 enabled=true 시켜주기 (50%-1개, 20%-2개, 15%-3개, 10%-4개, 5%-5개)
                if (!playMode)
                {
                    n = UnityEngine.Random.Range(0, 100);
                }
                else if (playMode)
                {
                    if (playSproutEnabledNum == 1) n = 40;
                    else if (playSproutEnabledNum == 2) n = 60;
                    else if (playSproutEnabledNum == 3) n = 80;
                    else if (playSproutEnabledNum == 4) n = 90;
                    else if (playSproutEnabledNum == 5) n = 98;
                }
                
                
                if (n < 50)
                {
                    int random = UnityEngine.Random.Range(0, treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas.Count - 1);
                    treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas[random].enabled = true;
                    rarityScore += 10;
                }
                else if (n >= 50 && n < 70)
                {
                    rarityScore += 20;
                    for (int j = 0; j < 2; j++)
                    {
                        int random = UnityEngine.Random.Range(0, treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas.Count-1);
                        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas[random].enabled = true;
                    }
                }
                else if (n >= 70 && n < 85)
                {
                    rarityScore += 30;
                    for (int j = 0; j < 3; j++)
                    {
                        int random = UnityEngine.Random.Range(0, treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas.Count - 1);
                        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas[random].enabled = true;
                    }
                }
                else if (n >= 85 && n < 95)
                {
                    rarityScore += 40;
                    for (int j = 0; j < 4; j++)
                    {
                        int random = UnityEngine.Random.Range(0, treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas.Count - 1);
                        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas[random].enabled = true;
                    }
                }
                else
                {
                    rarityScore += 50;
                    for (int j = 0; j < 5; j++)
                    {
                        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas[j].enabled = true;
                    }
                }
                print("sprout 선택 완료");


                // Bark Material 확률적 랜덤 선택
#if UNITY_STANDALONE
                string path = Application.dataPath + "/Resources/Tree/Materials";
#elif UNITY_IOS
                string path = Application.persistentDataPath + "/Resources/Tree/Materials";
#endif
                Material[] mat = barkAssetBundle.LoadAllAssets<Material>();
                if (!playMode)
                {
                    int randNum = UnityEngine.Random.Range(0, 10);
                    // General Material (70%)
                    if (randNum < 7)
                    {
                        int r1 = UnityEngine.Random.Range(0, 5);
                        Material selectedMat = Instantiate(mat[r1]);
                        print("Selected General Bark Material: " + mat[r1].name);
                        treePipeline._serializedPipeline.barkMappers[0].customMaterial = selectedMat;
                        barkMaterial = selectedMat.name.Replace("(Clone)", "");
                        rarityScore += 30;
                    }
                    // Special Material (30%)
                    else
                    {
                        int r2 = UnityEngine.Random.Range(5, 9);
                        Material selectedMat = Instantiate(mat[r2]);
                        print("Selected Special Bark Material: " + mat[r2].name);
                        treePipeline._serializedPipeline.barkMappers[0].customMaterial = selectedMat;
                        barkMaterial = selectedMat.name.Replace("(Clone)", "");
                        rarityScore += 50;
                    }
                }
                // PlayMode의 경우 텍스처 선택
                else if (playMode)
                {
                    Material selectedMat2 = Instantiate(mat[playBarkIdx]);
                    print("Selected General Bark Material: " + mat[playBarkIdx].name);
                    treePipeline._serializedPipeline.barkMappers[0].customMaterial = selectedMat2;
                    barkMaterial = selectedMat2.name.Replace("(Clone)", "");
                    rarityScore += 50;
                }
                
            }
            print("Selected pipeline = " + pipeName);

            // Tree Grow 1일차 기본 세팅
            if (!demoMode)
            {
                // 랜덤 선택된 Seed로 기본 세팅값 찾기
                for (int i = 0; i < treeStores.Count; i++)
                {
                    if (treeStores[i].seedType == selectedSeed)
                    {
                        selectedTreeSetting = treeStores[i].treeSettings;
                        print($"나무 기본 세팅 : {treeStores[i].seedType}");
                    }
                }
                // 1일차 기본 세팅
                PipelineSetting(0);
                // 씨앗심기
                SetTree(1);
            }
        }
        else if (visitType == VisitType.ReVisit)
        {
            // 캔버스 활성화
            landCanvas.SetActive(true);
            chatCanvas.SetActive(true);

            // 로드한 이전 나무 데이터 세팅
            LoadDataSetting();

            // firstPlantDate와 dayCount에 따라 그에 맞는 HealthData 반영
            if (dayCount > 1)
            {
                soil.SetActive(false);
                plant.SetActive(true);
                // ReVisit했는데 해당 DayCount와 저장한 나무 데이터 수가 동일하지 않을 경우 (= 24H 지나고 처음 들어온 경우)
                if (dayCount != currentTreeData.treeGrowths.Count)
                {
                    print("24H 지나고 첫방문으로 Daycount가 바뀌었음");
                    // 이전 날의 HealthData 반영 + 데이터 저장 + 나무 변경 Text
                    ApplyHealthData();
                    SaveTreeData();
                    txtMain.text = "나무가 성장했어요!";
                    txtSub.text = "나만의 아이템으로 랜드를 꾸며보세요.";
                    // Tree 로드
                    PipelineReload();
                    changeParticle.Play();
                    treeFactory.gameObject.SetActive(true);
                }
                // ReVisit했는데 해당 DayCount와 저장한 나무 데이터 수가 동일할 경우
                else
                {
                    // 해당 데이터 나무에 반영
                    ApplyHealthData();
                    // Tree 로드
                    PipelineReload();
                    treeFactory.gameObject.SetActive(true);
                }

                if (dayCount == 5)
                {
                    // My Collection Text 활성화
                    txtTreeName.GetComponent<Text>().enabled = true;
                    txtTreeBirth.GetComponent<Text>().enabled = true;
                    txtTreeRarity.GetComponent<Text>().enabled = true;
                    txtTreeVitality.GetComponent<Text>().enabled = true;

                    // Image Capture & Save
                    screenShot.SaveCameraView();
                    screenShot.SaveTreeImg();
                    // Video Capture & Save

                    // My Collection 항목 추가
                    //uiInitial.MakeMyCollection();
                }
            }
            // 1일차의 경우
            else if (dayCount == 1)
            {
                sprout.SetActive(true);
                sproutLeaf.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    /// <summary>
    /// 나무 처음 심은 날을 기반으로 해당 dayCount에 맞는 헬스 데이터 가져온 뒤 나무에 적용
    /// </summary>
    public void ApplyHealthData()
    {
        DateTime date = GameManager.Instance.timeManager.firstPlantDate;

        // 헬스 데이터 받아오기
        if (HealthDataStore.GetStatus() == HealthDataStoreStatus.Loaded)
        {
            report = HealthDataAnalyzer.GetDailyReport(
                new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, DateTimeKind.Local),
                dayCount
            );
            if (report != null)
            {
                Debug.Log(date + " 의 HealthData");
                Debug.Log(JsonUtility.ToJson(report, true));
            }
        }

        // 헬스데이터 나무에 반영
        // Sleep Data
        int sleepAmount = (int)report.SleepReport.SleepAmount;
        SleepAmountToTree(sleepAmount);
        int sleepRiseTimeVariance = (int)report.SleepReport.SleepRiseTimeVariance;
        SleepRiseToTree(sleepRiseTimeVariance);
        int sleepDaytimeNap = (int)report.SleepReport.SleepDaytimeNap;
        NapToTree(sleepDaytimeNap);

        // Activity Data
        float activeEnergyBurnedGoalAchieved = (float)report.ActivityReport.ActiveEnergyBurnedGoalAchieved;
        float exerciseTimeGoalAchieved = (float)report.ActivityReport.ExerciseTimeGoalAchieved;
        float standHoursGoalAchieved = (float)report.ActivityReport.StandHoursGoalAchieved;
        float average = (activeEnergyBurnedGoalAchieved + exerciseTimeGoalAchieved + standHoursGoalAchieved) / 3;
        ScaleChange(average);
    }

    /// <summary>
    /// 로드한 데이터로 나무 세팅
    /// </summary>
    public void LoadDataSetting()
    {
        // Tree Id
        treeId = currentTreeData.id;
        pipeName = currentTreeData.treePipeName;
        // Tree Pipeline
        treePipeline = assetBundle.LoadAsset<Pipeline>(pipeName);
        print(treePipeline == null);
        print(treePipeline);
        // Tree Name
        txtTreeName.text = currentTreeData.treeName;
        print("treePipeline.seed = " + treePipeline.seed);
        print("currentTreeData.seedNumber = " + currentTreeData.seedNumber);
        // Seed Number
        treePipeline.seed = currentTreeData.seedNumber;
        // Seed Type
        selectedSeed = pipeNameDict[currentTreeData.treePipeName];
        // First Plant Date
        GameManager.Instance.timeManager.firstPlantDate = DateTime.Parse(currentTreeData.treeGrowths[0].createdAt);
        // 현재 랜드 나무의 dayCount에 맞는 Tree Pipeline Data
        TreePipeline pipeData = currentTreeData.treeGrowths[dayCount - 1].treePipeline;
        // bark Material
        string name = currentTreeData.barkMaterial;
        Material mat = barkAssetBundle.LoadAsset<Material>(name);
        treePipeline._serializedPipeline.barkMappers[0].customMaterial = mat;
        // sproutGroup
        SproutSeed sproutSeed = new SproutSeed();
        treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
        treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[0].groupId = currentTreeData.sproutGroupId;
        // sproutColor
        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[0].enabled = currentTreeData.sproutColor1 == 1 ? true : false;
        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[1].enabled = currentTreeData.sproutColor2 == 1 ? true : false;
        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[2].enabled = currentTreeData.sproutColor3 == 1 ? true : false;
        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[3].enabled = currentTreeData.sproutColor4 == 1 ? true : false;
        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId].sproutAreas[4].enabled = currentTreeData.sproutColor5 == 1 ? true : false;

        #region TreeGrowth
        // 1. Scale
        scaleTo = pipeData.scale;
        // 2. Branch Number
        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minFrequency = pipeData.branch1;
        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].maxFrequency = pipeData.branch1;
        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].minFrequency = pipeData.branch2;
        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].maxFrequency = pipeData.branch2;
        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].minFrequency = pipeData.branch3;
        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].maxFrequency = pipeData.branch3;
        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].minFrequency = pipeData.branch4;
        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[3].maxFrequency = pipeData.branch4;
        // 4. Sprout Number
        treePipeline._serializedPipeline.sproutGenerators[0].minFrequency = pipeData.sproutNum;
        treePipeline._serializedPipeline.sproutGenerators[0].maxFrequency = pipeData.sproutNum;
        // 5. Ratio of Rotten Sprout : 0, 25, 50, 75, 100
        List<int> groupNum = new List<int>() { 5, 6, 7, 8 };
        for (int i = 0; i < (pipeData.rottenRate/25); i++) // i < 0, 1, 2, 3, 4
        {
            SproutSeed sproutGroup = new SproutSeed();
            treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutGroup);
            treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[^1].groupId = groupNum[i];
        }
        // 6. Sprout Width
        foreach (SproutMesh s in treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes)
        {
            s.width = pipeData.sproutWidth;
        }
        // 7. Gravity
        for (int i = 0; i < 4; i++)
        {
            if (!treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].isRoot)
            {
                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].minGravityAlignAtBase = pipeData.gravity;
                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].maxGravityAlignAtBase = pipeData.gravity;
            }
        }
    }

    //bool once = false;
    //bool once2 = false;
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
        //// 2. 작은 묘목
        //if (dayCount == 2)
        //{
        //    Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.7f, -19.7f, 3.2f), 2 * Time.deltaTime);
        //}
        //// 3. 묘목
        //if (dayCount == 3)
        //{
        //    Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.7f, -29.8f, 2.6f), 2 * Time.deltaTime);
        //}
        //// 4. 나무
        //if (dayCount == 4)
        //{
        //    if (badMode)
        //    {
        //        Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.7f, -37.8f, 3.2f), 2 * Time.deltaTime);
        //    }
        //    else
        //    {
        //        Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.9f, -44.2f, 6.2f), 2 * Time.deltaTime);
        //    }
        //}
        //// 5. 열매
        //if (dayCount == 5)
        //{
        //    if (badMode)
        //    {
        //        Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.3f, -40.9f, 3.7f), 2 * Time.deltaTime);
        //    }
        //    else
        //    {
        //        Camera.main.gameObject.transform.localPosition = Vector3.Lerp(Camera.main.gameObject.transform.localPosition, new Vector3(-0.2f, -54.7f, 9.1f), 2 * Time.deltaTime);
        //    }
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
        //            PipelineReload();
        //            print(i);
        //        }
        //    }
        //}
        #endregion

        #region Sprout Group Test
        //if (Input.GetMouseButtonDown(0) && !once2)
        //{
        //    once2 = true;
        //    print("Mouse Click");
        //    SproutSeed sproutSeed = new SproutSeed();
        //    treePipeline = assetBundle.LoadAsset<Pipeline>("BasicTree");
        //    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
        //    treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[^1].groupId = 2;
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps.Count
        //treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[0].enabled


        //for (int i = 0; i < treeStores.Count; i++)
        //{
        //    if (treeStores[i].seedType == SeedType.Basic)
        //    {
        //        selectedTreeSetting = treeStores[i].treeSettings;
        //        print($"나무 기본 세팅 : {treeStores[i].seedType}");
        //    }
        //}
        //PipelineSetting(2);
        //PipelineReload();

        //print("sproutSeeds Count : " + treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Count);
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

        // 씨앗 심기
        seed.SetActive(true);
        yield return new WaitForSeconds(2);
        sproutParticle.Play();
        sproutLeaf.transform.localScale = new Vector3(0, 0, 0);
        sprout.SetActive(true);
        

        // 새싹 자라기
        t = 0;
        while (t <= 1f)
        {
            t += Time.deltaTime * 2f;
            sprout.transform.localScale = new Vector3(t, t, t);
            yield return null;
        }
        sprout.transform.localScale = new Vector3(1, 1, 1);

        // 새싹잎 자라기
        t = 0;
        sproutLeaf.transform.localScale = new Vector3(0, 0, 0);
        while (t <= targetScale)
        {
            t += Time.deltaTime * 2f;
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

        // 식물 이름 UI Animation
        treeNameBG.SetActive(true);
        treeNameWindow.transform.localScale = new Vector3(0, 0, 0);
        t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime * 2f;
            treeNameWindow.transform.localScale = new Vector3(t, t, t);
            yield return null;
        }
        treeNameWindow.transform.localScale = new Vector3(1, 1, 1);
        yield return new WaitForSeconds(1);
    }
    #endregion

    /// <summary>
    /// 나무 Pipeline 업데이트
    /// "dayMinMax" > Element Frequency
    /// "rootFreq" > Root Min/Max Freqency
    /// "rootBaseLength" > Min/Max Length At Base
    /// "girthBase" > Min/Max Girth At Base
    /// "scale" > Object scale
    /// </sumary>
    public void PipelineSetting(int index)
    {
        // 기본 세팅 성장 데이터 정보 지닌 요소
        TreeSetting element = selectedTreeSetting[index];

        //if (playMode)
        //{
        //    #region 1. Branch Element MinMax Frequency
        //    for (int i = 0; i < 4; i++)
        //    {
        //        pipeline
        //        StructureGenerator.StructureLevel pipe1 = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
        //        저장값
        //       MinMax store1 = element.minMaxList[i];

        //        if (badMode && dayCount > 3)
        //        {
        //            pipe1.minFrequency = store1.min - 2;
        //            pipe1.maxFrequency = store1.max - 2;
        //        }
        //        else
        //        {
        //            pipe1.minFrequency = store1.min;
        //            pipe1.maxFrequency = store1.max;
        //        }
        //    }
        //    #endregion

        //    #region 2. Min/Max Girth At Base
        //    GirthTransformElement pipe4 = treePipeline._serializedPipeline.girthTransforms[0];
        //    float store4 = element.girthBase;

        //    if (badMode && dayCount > 3)
        //    {
        //        Min Girth At Base
        //        pipe4.minGirthAtBase = store4 - 1f;
        //        Max Girth At Base
        //        pipe4.maxGirthAtBase = store4 - 1f;
        //    }
        //    else
        //    {
        //        Min Girth At Base
        //        pipe4.minGirthAtBase = store4;
        //        Max Girth At Base
        //        pipe4.maxGirthAtBase = store4;
        //    }

        //    #endregion

        //    #region 3. Object scale
        //    if (badMode && dayCount > 3)
        //    {
        //        scaleTo += 0.1f;
        //    }
        //    else
        //    {
        //        scaleTo = element.scale;
        //    }

        //    #endregion
        //}
        //else
        //{
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
            GirthTransformElement pipe4 = treePipeline._serializedPipeline.girthTransforms[0];
            float store4 = element.girthBase;

            // Min Girth At Base
            pipe4.minGirthAtBase = store4;
            // Max Girth At Base
            pipe4.maxGirthAtBase = store4;
            #endregion

            #region 5. Object scale
            scaleTo = element.scale;
            #endregion
        //}

    }



    /// <summary>
    /// 업데이트한 나무 정보를 기반으로 나무 다시 로드
    /// </summary>
    public void PipelineReload()
    {
        Pipeline loadedPipeline = assetBundle.LoadAsset<Pipeline>(pipeName);
        treeFactory.LoadPipeline(loadedPipeline.Clone(), true);
        treeFactory.UnloadAndClearPipeline();
        // previewTree
        if (previewTree == null) previewTree = GameObject.Find("previewTree").transform;
        treeFactory.transform.GetChild(1).gameObject.layer = 11;  
        treeFactory.transform.GetChild(1).localScale = new Vector3(scaleTo, scaleTo, scaleTo);
        Resources.UnloadAsset(loadedPipeline);
    }


    /// <summary>
    /// dayCount에 맞게 Tree 업데이트
    /// </summary>

    Transform campos;
    /// <summary>
    /// 입력한 day로 tree 세팅 (HealthSetting, Pipline 기본 세팅 중 선택 가능)
    /// </summary>
    /// <param name="day"> 일차 수 </param>
    /// <param name="healthSetting"> 1이면 HealthSetting 진행</param>
    public void SetTree(int day, int healthSetting=0)
    {
        // 씨앗 심기
        if (day == 1)
        {
            print("1일차");
            StartCoroutine(PlantSeed(1.02f));
            PipelineReload();
        }
        // 2일차
        else if (day == 2)
        {
            print("2일차");
            sprout.SetActive(false);
            soil.SetActive(false);
            plant.SetActive(true);
            if (healthSetting == 0) PipelineSetting(0);
            treeFactory.gameObject.SetActive(true);
            PipelineReload();
        }
        // 3일차
        else if (day == 3)
        {
            print("3일차");
            if (healthSetting == 0) PipelineSetting(1);
            else if (healthSetting == 1) ApplyHealthData();
            PipelineReload();
            campos = Camera.main.gameObject.transform;
        }
        // 4일차
        else if (day == 4)
        {
            print("4일차");
            if (healthSetting == 0) PipelineSetting(2);
            else if (healthSetting == 1) ApplyHealthData();
            PipelineReload();
        }
        // 5일차
        else if (day == 5)
        {
            print("5일차");
            if (healthSetting == 0) PipelineSetting(3);
            else if (healthSetting == 1) ApplyHealthData();
            PipelineReload();
            assetBundle.Unload(false);
            // Tree & Land 이미지 캡처
            screenShot.SaveCameraView();
            // Tree 캡처 이미지 업로드
            screenShot.SaveTreeImg();
            // Tree Video 캡쳐 + 압축 + 웹에 올리기
            screenRecorder.threadIsProcessing = true;
            screenRecorder.encoderThread = new Thread(screenRecorder.EncodeAndSave);
            screenRecorder.encoderThread.Start();

        }
        if (day>1) SaveTreeData();
    }


    /// <summary>
    /// 데모할 때의 SetTree 함수
    /// </summary>
    /// <param name="day"></param>
    /// <param name="demo"></param>
    [Space]
    [Header("Demo Variable")]
    public SkyController sky;
    public Transform fire;
    public ParticleSystem sproutParticle;
    public ParticleSystem changeParticle;
    public ParticleSystem snow;
    public ParticleSystem rain;
    public ParticleSystem rottenLeafParticle;
    public GameObject day2CustomObj;
    public GameObject day3CustomObj;
    public GameObject day4CustomObj;

    public void SetTree(int day, bool demo)
    {
        // 1일차 (씨앗 심기)
        if (day == 1)
        {
            StartCoroutine(PlantSeed(1.02f));
            // 나무 심은 시간 저장
            //GameManager.Instance.timeManager.firstPlantDate = DateTime.Now;
            treeFactory.transform.GetChild(0).gameObject.layer = 11;
            PipelineReload();

        }
        // 2일차
        else if (day == 2)
        {
            seed.SetActive(false);
            sprout.SetActive(false);
            soil.SetActive(false);
            //day2CustomObj.SetActive(true);
            // Sprout
            SproutNumChange(true, 10);
            // 트리 기본 세팅 값 로드
            PipelineReload();
            treeFactory.gameObject.SetActive(true);
            // Change Particle
            changeParticle.Play();
            // 조금 뒤에 비내리게 하기
            StartCoroutine(Delay(25));
            // Weather - Rain
            rain.Play();
            SaveTreeData();
        }
        // 3일차
        else if (day == 3)
        {
            rain.gameObject.SetActive(false);
            //day3CustomObj.SetActive(true);
            // SkyBox
            sky.Sunset();
            // Fire
            fire.GetChild(0).gameObject.SetActive(true);
            fire.GetChild(1).gameObject.SetActive(true);
            // Sprout
            SproutNumChange(true, 5);
            // Tree Pipeline - Branch MinMax, Girth, Scale
            PipelineSetting(1);
            PipelineReload();
            // Change Particle
            changeParticle.Play();
            SaveTreeData();
        }
        // 4일차
        else if (day == 4)
        {
            //day4CustomObj.SetActive(true);
            // SkyBox
            sky.Night();
            // Tree Pipeline - Branch MinMax, Girth, Scale
            if (badMode)
            {
                SproutNumChange(false, 10);
                // Tree Pipeline - Branch MinMax, Girth, Scale
                PipelineSetting(2);
                BadChange(true);
                rottenLeafParticle.Play();
            }
            else
            {
                SproutNumChange(true, 10);
                // Tree Pipeline - Branch MinMax, Girth, Scale
                PipelineSetting(4);
            }
            PipelineReload();
            // Weather - Snow
            snow.Play();
            // Change Particle
            if (badMode) changeParticle.transform.position = new Vector3(0, 5.32f, 0);
            else changeParticle.transform.position = new Vector3(0, 6.65f, 0);
            changeParticle.Play();
            SaveTreeData();
        }
        // 5일차
        else if (day == 5)
        {
            rottenLeafParticle.gameObject.SetActive(false);
            snow.gameObject.SetActive(false);
            // SkyBox
            sky.Day();
            // Fire
            fire.GetChild(0).gameObject.SetActive(false);
            fire.GetChild(1).gameObject.SetActive(false);
            if (badMode)
            {
                SproutNumChange(false, 10);
                // Tree Pipeline - Branch MinMax, Girth, Scale
                PipelineSetting(3);
                BadChange(true);
                rottenLeafParticle.gameObject.SetActive(true);
                rottenLeafParticle.Play();
            }
            else
            {
                // Sprout
                SproutNumChange(true, 10);
                // Tree Pipeline - Branch MinMax, Girth, Scale
                PipelineSetting(5);
            }
            PipelineReload();
            // Change Particle
            if (!badMode) changeParticle.transform.position = new Vector3(0, 10.46f, 0);
            changeParticle.Play();
            SaveTreeData();
            assetBundle.Unload(false);

        }
    }
    IEnumerator Delay(float second)
    {
        yield return new WaitForSeconds(second);
    }

    #region Tree Data
    /// <summary>
    /// Tree Data 저장
    /// </summary>
    string saveUrl;
    public async void SaveTreeData()
    {
        // Day1 - 2일차 기본 세팅 저장
        if (dayCount == 1 && visitType == VisitType.First)
        {
            saveUrl = "/api/v1/trees";
            TreeData treeData = new TreeData();
            List<TreeData> treeDatas = new List<TreeData>();

            // Tree Name
            treeData.treeName = treeName;
            // seed Number
            treeData.seedNumber = treePipeline.seed;
            // treePipeName
            treeData.treePipeName = pipeName;
            // Bark Material Name
            treeData.barkMaterial = barkMaterial;
            // Land ID 
            treeData.landId = 49;//DataTemporary.MyUserData.currentLandId;
            // Sprout Group Id
            treeData.sproutGroupId = sproutGroupId;
            // Sprout Texture Enabled (Sprout Grop 4가지)
            treeData.sproutColor1 = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas[0].enabled ? 1 : 0;
            treeData.sproutColor2 = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas[1].enabled ? 1 : 0;
            treeData.sproutColor3 = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas[2].enabled ? 1 : 0;
            treeData.sproutColor4 = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[sproutGroupId - 1].sproutAreas[3].enabled ? 1 : 0;
            // 희귀성
            treeData.rarity = rarityScore;
            // 생명력
            treeData.vitality = vitalityScore;

            // Tree Pipeline Data //
            // 1. Scale
            treeData.scale = previewTree.localScale.x;
            // 2. Branch Numbers
            List<StructureGenerator.StructureLevel> level = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels;
            treeData.branch1 = level[0].minFrequency;
            treeData.branch2 = level[1].minFrequency;
            treeData.branch3 = level[2].minFrequency;
            treeData.branch4 = level[3].minFrequency;
            // 3. Sprout Number
            treeData.sproutNum = treePipeline._serializedPipeline.sproutGenerators[0].minFrequency;
            // 5. Rotten Rate
            int rate = 0;
            foreach (SproutSeed s in treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds)
            {
                if (s.groupId == 5 | s.groupId == 6 | s.groupId == 7 | s.groupId == 8)
                {
                    rate += 25;
                }
            }
            treeData.rottenRate = rate;
            // 6. Sprout Width
            treeData.sproutWidth = treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes[sproutGroupId].width;
            // 7. Gravity
            treeData.gravity = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minGravityAlignAtTop;
            
            string treeJsonData = JsonUtility.ToJson(treeData);
            Debug.Log(JsonUtility.ToJson(treeData, true));

            // PUT Tree Data
            ResultPost<TreeData> resultPost = await DataModule.WebRequestBuffer<ResultPost<TreeData>>(
                saveUrl,
                DataModule.NetworkType.POST,
                DataModule.DataType.BUFFER,
                treeJsonData);

            if (!resultPost.result)
            {
                Debug.LogError("WebRequestError : NetworkType[Post]");
            }
            else
            {
                Debug.Log($"{dayCount}일차 Tree Data Save 성공");
            }
            treeDatas.Add(treeData);

            ArrayTreeData arrayTreeData = new ArrayTreeData();
            arrayTreeData.TreeDataList = treeDatas;

            // DataTemporary
            DataTemporary.TreeData = arrayTreeData;

            // File 형식으로 Update or Save
            FileManager.SaveDataFile("TreeData", arrayTreeData);
        }
        // Day2~5 - 헬스데이터 적용한 나무 데이터 저장
        else if (dayCount > 1 && dayCount < 6) //&& DataTemporary.GetTreeData.getTreeDataList[treeDataIdx].treeGrowths.Count < dayCount)
        {
            saveUrl = "/api/v1/tree-growths";
            TreePipeline treeData = new TreePipeline();
            List<TreePipeline> treeDatas = new List<TreePipeline>();

            // Tree Id
            treeData.treeId = treeId;

            // Tree Pipeline Data //
            // 1. Scale
            treeData.scale = previewTree.localScale.x;
            // 2. Branch Numbers
            List<StructureGenerator.StructureLevel> level = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels;
            treeData.branch1 = level[0].minFrequency;
            treeData.branch2 = level[1].minFrequency;
            treeData.branch3 = level[2].minFrequency;
            treeData.branch4 = level[3].minFrequency;
            // 3. Sprout Number
            treeData.sproutNum = treePipeline._serializedPipeline.sproutGenerators[0].minFrequency;
            // 4. Rotten Rate
            int rate = 0;
            foreach (SproutSeed s in treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds)
            {
                if (s.groupId == 5 | s.groupId == 6 | s.groupId == 7 | s.groupId == 8)
                {
                    rate += 25;
                }
            }
            treeData.rottenRate = rate;
            // 5. Sprout Width
            treeData.sproutWidth = treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes[sproutGroupId].width;
            // 6. Gravity
            treeData.gravity = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minGravityAlignAtTop;
            // 7. Rarity
            treeData.rarity = rarityScore;
            // 8. Vitality
            treeData.vitality = vitalityScore;

            string treeJsonData = JsonUtility.ToJson(treeData);
            Debug.Log(JsonUtility.ToJson(treeData, true));

            // PUT Tree Data
            ResultPost<TreeData> resultPost = await DataModule.WebRequestBuffer<ResultPost<TreeData>>(
                saveUrl,
                DataModule.NetworkType.POST,
                DataModule.DataType.BUFFER,
                treeJsonData);

            if (!resultPost.result)
            {
                Debug.LogError("WebRequestError : NetworkType[Post]");
            }
            else
            {
                Debug.Log($"{dayCount}일차 Tree Data Save 성공");
            }
            treeDatas.Add(treeData);

            ArrayTreeData2 arrayTreeData = new ArrayTreeData2();
            arrayTreeData.TreeDataList2 = treeDatas;

            // DataTemporary
            DataTemporary.TreeData2 = arrayTreeData;

            // File 형식으로 Update or Save
            FileManager.SaveDataFile("TreeData2", arrayTreeData);
        }
    }
    #endregion

    #region User Data to Tree

    /// <summary>
    /// 수면양 enum 타입에 따른 나무 데이터 변경
    /// </summary>
    public void SleepAmountToTree(int sleepAmount)
    {
        if (sleepAmount == 0)
        {
            BadChange(true);
        }
        else if (sleepAmount == 1)
        {
            BranchNumChange(false, 2);
        }
        else if (sleepAmount == 2)
        {
            BranchNumChange(false, 1);
        }
        else if (sleepAmount == 3)
        {
            BranchNumChange(true, 1);
        }
        else if (sleepAmount == 4)
        {
            BranchNumChange(true, 2);
        }
    }
    /// <summary>
    /// 기상 시간 오차에 따른 나무 데이터 변경
    /// </summary>
    public void SleepRiseToTree(int riseTime)
    {
        if (riseTime == 0)
        {
            SproutNumChange(false, 10);
        }
        else if (riseTime == 1)
        {
            SproutNumChange(true, 10);
        }
    }
    /// <summary>
    /// Daytime 낮잠 여부에 따른 나무 데이터 변경
    /// </summary>
    public void NapToTree(int isNap)
    {
        if (isNap == 0)
        {
            BadChange(false);
        }
        else if (isNap == 1)
        {
            BadChange(true);
        }
    }
    /// <summary>
    /// Activity 달성 퍼센트에 따라 나무 Scale 조절하는 함수
    /// </summary>
    /// <param name="activityAverage"> Activity 달성 퍼센트 평균 </param>
    public void ScaleChange(float activityAverage, float lerpSpeed = 2f)
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
    public void SproutNumChange(bool addSprout, int sproutNum)
    {
        if (addSprout)
        {
            treePipeline._serializedPipeline.sproutGenerators[0].minFrequency += sproutNum;
            treePipeline._serializedPipeline.sproutGenerators[0].maxFrequency += sproutNum;
        }
        else
        {
            treePipeline._serializedPipeline.sproutGenerators[0].minFrequency -= sproutNum;
            treePipeline._serializedPipeline.sproutGenerators[0].maxFrequency -= sproutNum;
        }
    }
    /// <summary>
    /// 나무의 상한잎, 중력, 나뭇잎 너비, 나무 가지 두께
    /// </summary>
    /// <param name="yesBad"> 나쁜 영향을 줄 것인지, 나쁜 영향을 완화시킬 것인지 </param>
    public void BadChange(bool yesBad)
    {
        // 나쁜 영향을 줄 경우
        if (yesBad)
        {
            #region 1. 상한 잎
            // 1. 상한 잎 => 상한 잎 Particle 색 변경 필요
            List<int> groupId = new List<int>();
            SproutSeed sproutSeed = new SproutSeed();
            // Sprout Generator에 상한 잎 Group 5~8 있는지 확인 후 만약에 없으면 순서대로 하나 추가
            foreach (SproutSeed s in treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds)
            {
                groupId.Add(s.groupId);
            }
            if (!groupId.Contains(5) && !groupId.Contains(6) && !groupId.Contains(7) && !groupId.Contains(8))
            {
                treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
                treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[^1].groupId = 5;
            }
            if (groupId.Contains(5))
            {
                treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
                treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[^1].groupId = 6;
            }
            if (groupId.Contains(5) && groupId.Contains(6))
            {
                treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
                treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[^1].groupId = 7;
            }
            if (groupId.Contains(5) && groupId.Contains(6) && groupId.Contains(7))
            {
                treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
                treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[^1].groupId = 8;
            }
            #endregion

            #region 2. 가지 중력
            // 2. 가지 중력 (상태 안 좋을수록 마이너스)
            for (int i = 0; i < 4; i++)
            {
                StructureGenerator.StructureLevel gravityPipe = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                gravityPipe.minGravityAlignAtTop -= 0.2f;
                gravityPipe.maxGravityAlignAtTop -= 0.2f;
            }
            #endregion

            #region 3. 나뭇잎 너비
            treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes[sproutGroupId].width -= 0.5f;
            #endregion

            #region 4. 나무 가지 두께
            treePipeline._serializedPipeline.girthTransforms[0].minGirthAtBase -= 0.2f;
            treePipeline._serializedPipeline.girthTransforms[0].maxGirthAtBase -= 0.2f;
            #endregion

            #region 5. 나뭇잎 처짐
            treePipeline._serializedPipeline.sproutGenerators[0].minGravityAlignAtTop -= 0.3f;
            treePipeline._serializedPipeline.sproutGenerators[0].maxGravityAlignAtTop -= 0.3f;
            #endregion
        }
        // 나쁜 영향을 완화시킬 경우
        else
        {
            #region 1. 상한 잎
            int sproutTextures = treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas.Count;
            for (int i = 5; i < sproutTextures; i++)
            {
                if (treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled)
                {
                    treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[0].sproutAreas[i].enabled = false;
                    break;
                }
            }
            #endregion

            #region 2. 가지 중력 (상태 좋을수록 플러스)
            for (int i = 0; i < 4; i++)
            {
                StructureGenerator.StructureLevel gravityPipe = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                gravityPipe.minGravityAlignAtTop += 0.2f;
                gravityPipe.maxGravityAlignAtTop += 0.2f;
            }
            #endregion
             
            #region 3. 나뭇잎 너비
            if (treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes[sproutGroupId].width < 3.2f)
            {
                treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes[sproutGroupId].width += 0.5f;
            }
            #endregion

            #region 4. 나무 가지 두께
            treePipeline._serializedPipeline.girthTransforms[0].minGirthAtBase += 0.2f;
            treePipeline._serializedPipeline.girthTransforms[0].maxGirthAtBase += 0.2f;
            #endregion

            #region 5. 나뭇잎 처짐
            if (treePipeline._serializedPipeline.sproutGenerators[0].maxGravityAlignAtTop < 0.6f)
            {
                treePipeline._serializedPipeline.sproutGenerators[0].minGravityAlignAtTop += 0.3f;
                treePipeline._serializedPipeline.sproutGenerators[0].maxGravityAlignAtTop += 0.3f;
            }
            
            #endregion
        }

    }
    #endregion

}
#endregion