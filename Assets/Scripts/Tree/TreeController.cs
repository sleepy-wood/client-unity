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
        #region 변수
        // Pipeline Element별 FlatFrequency Min/Max값 저장소
        [System.Serializable]
        public class minMax
        {
                public int min;
                public int max;
        }
        [System.Serializable]
        public class ElementsList { public List<minMax> minMaxList = new List<minMax>(); };
        [System.Serializable]
        public class flatFreq
        {
                public ElementsList flatFreqMinMax;
                public int rootFreq;
                public int rootBaseLength;
                public float girthBase;
                public float scale;
        }
        // DayCount에 따라 변하는 나무 관련 변수 저장소
        public List<flatFreq> flatFreqencyList = new List<flatFreq>();
        // pipeline load path
        //string path;/
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
        //public float defaultFOV = 64.0f;
        //public float targetFOV = 20.0f;
        // TreeData
        public TreeData data;
        // leafTexture
        public Texture2D leafText;
        // 식물 이름 결정 UI
        public GameObject plantNameUI;
        // 식물 이름 InputField
        public InputField inputPlantName;
        // 식물 이름 결정 Button
        public Button btnPlantName;
        // 방문 타입
        public VisitType visitType;
        // user
        public GameObject user;
        // previewTree Scale Value
        float scaleTo;
        // Playable Director
        public PlayableDirector pd;
        
        #endregion

        public enum VisitType
        {
                None,
                First,
                ReVisit
        }

        AssetBundle assetBundle;
        void Start()
        {
                //user = GameManager.Instance.User;
                //user.GetComponent<UserInput>().InputControl = true;

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

                // TreeData  객체 생성
                data = new TreeData();
                data.leafTexture = leafText;
                //data.landID = growPos.parent.gameObject.name;

                // TreePipeline path
                //path = "Tree/MyTreePipeline_2";

                // treePipeline 로드
                //treePipeline = Resources.Load<Pipeline>(path);
                treePipeline = assetBundle.LoadAsset<Pipeline>("MyTreePipeline_2");
                // TextAsset b = Resources.Load<TextAsset>(path);
                
                // 방문 타입 결정
                if (visitType == VisitType.None) visitType = VisitType.First;
                else visitType = VisitType.ReVisit;

                inputPlantName.onValueChanged.AddListener(onValueChanged);

                // 방문 타입 &  DayCount에 따라 다르게 나무 Load
                //LoadTree();
                pd.Play();


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


        void Update()
        {
                if (Input.GetKeyDown(KeyCode.Alpha1)&& dayCount < 5 && !plantNameUI.activeSelf)
                {
                        dayCount++;
                        data.treeDay = dayCount;
                        txtDayCount.text = $"Day{dayCount}";
                        LoadTree();
                }


                // Test용
                if (Input.touchCount > 0)
                {
                        for (int i = 0; i < Input.touches.Length; i++)
                        {
                                if (Input.touches[i].phase == TouchPhase.Ended && dayCount < 5 && !plantNameUI.activeSelf)
                                {
                                        dayCount++;
                                        data.treeDay = dayCount;
                                        txtDayCount.text = $"Day{dayCount}";
                                        LoadTree();
                                }
                        }
                }

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
        //IEnumerator PlantSeed(float targetScale)
        //{
        //        #region 카메라 줌인
        //        float t = 0;
        //        //while (t < 1)
        //        //{
        //        //        t += Time.deltaTime;
        //        //        Camera.main.fieldOfView = Mathf.Lerp(defaultFOV, targetFOV, t);
        //        //        yield return null;
        //        //}
        //        //Camera.main.fieldOfView = targetFOV;
        //        #endregion

        //        // 씨앗 심기
        //        GameObject s = Instantiate(seedFactory);
        //        s.transform.position = growPos.position + new Vector3(0, 2, 0);
        //        s.gameObject.SetActive(true);
        //        yield return new WaitForSeconds(0.5f);
        //        while (s.transform.position.y >= -1)
        //        {
        //                s.transform.position += Vector3.down * downSpeed * Time.deltaTime;
        //                yield return null;
        //        }
        //        DestroyImmediate(s, true);

        //        // 새싹 나타나기
        //        t = 0;
        //        s = Instantiate(sproutFactory);
        //        sprout = s;
        //        s.transform.parent = growPos;
        //        s.transform.localPosition = new Vector3(0,0.15f, 0);
        //        while (t <= targetScale)
        //        {
        //                t += Time.deltaTime * 0.5f;
        //                s.transform.localScale = new Vector3(t, t, t);
        //                yield return null;
        //        }
        //        s.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        //        yield return new WaitForSeconds(1);

        //        #region 카메라 줌 아웃
        //        //t = 0;
        //        //while (t < 1)
        //        //{
        //        //        t += Time.deltaTime * 0.5f;
        //        //        Camera.main.fieldOfView = Mathf.Lerp(targetFOV, defaultFOV, t);
        //        //        yield return null;
        //        //}
        //        //Camera.main.fieldOfView = defaultFOV;
        //        #endregion
        //}
        #endregion

        /// <summary>
        /// 나무 Pipeline 업데이트
        /// "flatFreqMinMax" > Element Frequency
        /// "rootFreq" > Root Min/Max Freqency
        /// "rootBaseLength" > Min/Max Length At Base
        /// "girthBase" > Min/Max Girth At Base
        /// "scale" > Object scale
        /// </summary>
        /// <param name="dayCount"></param>
        public void TreeUpdate(int dayCount)  
        {
                // 날짜에 맞춘 정보를 가지고 있는 요소
                flatFreq element = flatFreqencyList[dayCount-2];
                
                #region 1. Element FlatFreqMinMax
                int idx = element.flatFreqMinMax.minMaxList.Count;
                // 각 요소 for문 돌리며 세팅
                for (int i=0; i < idx; i++)
                {
                        // pipeline
                        StructureGenerator.StructureLevel pipe1 = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                        // 저장값
                        minMax store1 = flatFreqencyList[dayCount - 2].flatFreqMinMax.minMaxList[i];

                        // Min Frequenc
                        pipe1.minFrequency = store1.min;
                        // Max Frequency
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
        }

        /// <summary>
        /// 업데이트한 나무 정보를 기반으로 나무 다시 로드
        /// </summary>
        public void TreeReload()
        {
                Debug.Log("TreeReload");
                //Pipeline loadedPipeline = Resources.Load<Pipeline>(path);
                Pipeline loadedPipeline = assetBundle.LoadAsset<Pipeline>("MyTreePipeline_2"); ;
                treeFactory.UnloadAndClearPipeline();
                treeFactory.LoadPipeline(loadedPipeline.Clone(), true);
                Debug.Log("2");

                //#if UNITY_STANDALONE
                //                string filePath = Application.dataPath + "/TreeTest.asset";
                //#elif UNITY_IOS || UNITY_ANDROID
                //                string filePath = Application.persistentDataPath + "/TreeTest.asset";
                //#endif
                //                Debug.Log(filePath);
                //                 TreeSystem.Save(treePipeline, filePath);

                //                treePipeline = TreeSystem.Load(filePath);

                Resources.UnloadAsset(loadedPipeline);

                //treePipeline = assetBundle.LoadAsset<Pipeline>("MyTreePipeline_2");
                treeFactory.transform.GetChild(0).localScale = new Vector3(scaleTo, scaleTo, scaleTo);
                Debug.Log(treeFactory.transform.GetChild(0).localScale);
        }

        /// <summary>
        /// 식물 이름 입력하면 다음 버튼 활성화
        /// </summary>
        /// <param name="s">식물 이름</param>
        void onValueChanged(string s)
        {
                btnPlantName.interactable = true;
        }

        /// <summary>
        /// 식물 이름 결정 버튼 누르면 나무 이름 저장 & UI 비활성화
        /// </summary>
        public void onConfirmPlantName()
        {
                //user.GetComponent<UserInput>().InputControl = false;
                inputPlantName.text = data.treeName;
                plantNameUI.SetActive(false);
        }

        /// <summary>
        /// dayCount에 맞게 Tree 업데이트
        /// </summary>
        public void LoadTree()
        {
                // 1. 씨앗심기 & 새싹
                if (dayCount == 1)
                {
                        //StartCoroutine(PlantSeed(0.5f));
                        seed.SetActive(true);
                        pd.Play();
                        // 식물 이름 UI 띄우기
                        //plantNameUI.gameObject.SetActive(true);
                        TreeReload();
                }
                // 2. 작은 묘목
                if (dayCount == 2)
                {
                        sprout.SetActive(false);
                        soil.SetActive(false);
                        TreeUpdate(dayCount);
                        TreeReload();
                        treeFactory.gameObject.SetActive(true);
                }
                // 3. 묘목
                if (dayCount == 3)
                {
                        TreeUpdate(dayCount);
                        TreeReload();
                }
                // 4. 나무
                if (dayCount == 4)
                {
                        TreeUpdate(dayCount);
                        TreeReload();
                }
                // 5. 개화
                if (dayCount == 5)
                {
                        TreeUpdate(dayCount);
                        TreeReload();
                }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveTreeData()
        {

        }
}