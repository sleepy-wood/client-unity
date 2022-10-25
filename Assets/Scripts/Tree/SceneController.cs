using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Broccoli.Factory;
using Broccoli.Pipe;
using Broccoli.Generator;
using System.IO;
using UnityEngine.EventSystems;

public class SceneController : MonoBehaviour
{
        #region 변수
        // pipeline load path
        string path;
        // tree Factory
        public TreeFactory treeFactory = null;
        // The pipeline
        public Pipeline treePipeline;
        // 나무 자라는 위치
        public Transform growPos;
        // DayCount
        public int dayCount;
        // 씨앗 Prefab
        public GameObject seedPrefab;
        // 씨앗 하강 속도
        public float downSpeed = 0.5f;
        // 새싹 Prefab
        public GameObject sproutPrefab;
        // Daycount Text
        public Text txtDayCount;
        // sprout
        public GameObject sprout;
        // FOV
        public float defaultFOV = 64.0f;
        public float targetFOV = 20.0f;
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

        public GameObject user;

        bool isControl = false;
        #endregion


        void Start()
        {
                //user = GameManager.Instance.User;
                //user.GetComponent<UserInput>().InputControl = true;

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

                inputPlantName.onEndEdit.AddListener(onEndEdit);

                path = "Tree/NewTreePipeline5";
                treePipeline = Resources.Load<Pipeline>(path);

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
        void Update()
        {
                // Test용
                if (Input.GetMouseButtonDown(0) && dayCount < 5 && !isControl && !plantNameUI.activeSelf)
                {
                        dayCount ++;
                        data.treeDay = dayCount;
                        txtDayCount.text = $"Day{dayCount}";
                        LoadTree();
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


        IEnumerator PlantSeed(GameObject sproutGo, float targetScale)
        {
                // 카메라 줌인
                float t = 0;
                while (t < 1)
                {
                        t += Time.deltaTime;
                        Camera.main.fieldOfView = Mathf.Lerp(defaultFOV, targetFOV, t);
                        yield return null;
                }
                Camera.main.fieldOfView = targetFOV;

                // 씨앗 심기
                GameObject s = Instantiate(seedPrefab);
                s.transform.position = growPos.position + new Vector3(0, 4, 0);
                s.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.5f);
                while (s.transform.position.y >= -1)
                {
                        s.transform.position += Vector3.down * downSpeed * Time.deltaTime;
                        yield return null;
                }
                Destroy(s);

                // 새싹 나타나기
                t = 0;
                GameObject go = Instantiate(sproutGo);
                sprout = go;
                go.transform.position = new Vector3(0, 1, 0);
                while (t <= targetScale)
                {
                        t += Time.deltaTime * 0.5f;
                        go.transform.localScale = new Vector3(t, t, t);
                        yield return null;
                }
                go.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
                yield return new WaitForSeconds(1);

                // 카메라 줌 아웃
                t = 0;
                while (t < 1)
                {
                        t += Time.deltaTime * 0.5f;
                        Camera.main.fieldOfView = Mathf.Lerp(targetFOV, defaultFOV, t);
                        yield return null;
                }
                Camera.main.fieldOfView = defaultFOV;

                // 식물 이름 UI 띄우기
                plantNameUI.gameObject.SetActive(true);

        }


        /// <summary>
        /// 나무 정보 업데이트
        /// </summary>
        /// <param name="branchNum">가지 개수</param>
        /// <param name="sproutFreq">잎 Freq</param>
        /// <param name="rootChild">자식 개수</param>
        /// <param name="length">나무 길이</param>
        /// <param name="thick">나무 굵기</param>
        public void TreeDataUpdate(int branchNum, int sproutFreq, int rootChild, int length, float thick)
        {
                if (treePipeline == null) treePipeline = Resources.Load<Pipeline>(path);

                // 가지 개수
                int levelCount = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels.Count;
                for (int i = 0; i < levelCount; i++)
                {
                        if (dayCount == 3)
                        {
                                if (treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].enabled == false)
                                {
                                        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].enabled = true;
                                }
                        }
                        //if (dayCount == 5)
                        //{
                        //        // 꽃 텍스처 추가
                        //        treePipeline._serializedPipeline.sproutMappers[0].sproutMaps[1].sproutAreas[2].enabled = true;
                        //}
                        else
                        {
                                // Sprout Level
                                if (treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].isSprout)
                                {
                                        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].minFrequency = sproutFreq;
                                        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].maxFrequency = sproutFreq * 2;
                                }
                                else
                                {
                                        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].minFrequency = branchNum;
                                        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].maxFrequency = branchNum;
                                        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].maxLengthAtBase = branchNum;
                                        treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i].minLengthAtBase = branchNum;
                                }
                        }
                }

                // 나무 Root 자식 가지
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minFrequency = rootChild;
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.maxFrequency = rootChild * 2;

                // 나무 길이
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.maxLengthAtBase = length;
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minLengthAtBase = length;

                // 나무 굵기
                treePipeline._serializedPipeline.girthTransforms[0].minGirthAtBase = thick;
                treePipeline._serializedPipeline.girthTransforms[0].maxGirthAtBase = thick;
        }

        /// <summary>
        /// 업데이트한 나무 정보를 기반으로 나무 다시 로드
        /// </summary>
        public void TreeReload()
        {
                Debug.Log("TreeReload");
                Broccoli.Pipe.Pipeline loadedPipeline = treePipeline;
                treeFactory.UnloadAndClearPipeline();
                treeFactory.LoadPipeline(loadedPipeline.Clone(), path, true, true);
                treePipeline = Resources.Load<Pipeline>(path);
        }

        /// <summary>
        /// 식물 이름 입력한 뒤 Enter
        /// </summary>
        /// <param name="s">식물 이름</param>
        void onEndEdit(string s)
        {
                btnPlantName.interactable = true;
                data.treeName = s;
        }

        /// <summary>
        /// 식물 이름 결정 버튼
        /// </summary>
        public void onConfirmPlantName()
        {
                isControl = false;
                //user.GetComponent<UserInput>().InputControl = false;
                plantNameUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadTree()
        {
                // 1. 씨앗심기 & 새싹
                if (dayCount == 1)
                {
                        isControl = true;
                        StartCoroutine(PlantSeed(sproutPrefab, 0.5f));
                }
                // 2. 작은 묘목
                if (dayCount == 2)
                {
                        sprout.SetActive(false);
                        treeFactory.gameObject.SetActive(true);
                }
                // 3. 묘목
                if (dayCount == 3)
                {
                        TreeDataUpdate(2, 3, 5, 10, 0.5f);
                        TreeReload();
                }
                // 4. 나무
                if (dayCount == 4)
                {
                        TreeDataUpdate(5, 10, 10, 20, 0.7f);
                        TreeReload();
                }
                // 5. 개화
                if (dayCount == 5)
                {
                        TreeDataUpdate(8, 15, 15, 25, 0.8f);
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
