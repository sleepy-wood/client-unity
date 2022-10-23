using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Broccoli.Factory;
using Broccoli.Pipe;
using Broccoli.Generator;
using System.IO;


public class SceneController : MonoBehaviour
{
        #region 변수
        /// 나무 생성하는 파이프라인 경로 (Reosurces 폴더 내에 있어야함)
        private static string runtimePipelineResourcePath = "BroccoliRuntimeExamplePipeline";
        /// tree Factory
        private TreeFactory treeFactory = null;
        /// The pipeline
        // private Pipeline pipeline = null;
        public Pipeline treePipeline;
        /// The positioner element 담는 변수
        private PositionerElement positionerElement = null;
        /// 나무 자라는 위치
        public  Transform growPos;
        /// DayCount
        public int dayCount;
        #endregion


        void Start()
        {
                // Build mesh 오류 해결 코드
                string resPath = Application.dataPath + "/Resources/TreePipeline.asset";
                if (!File.Exists(resPath))
                {
                        string path = Application.streamingAssetsPath + "/TreePipeline.asset";
                        byte[] data = File.ReadAllBytes(path);
                        File.WriteAllBytes(resPath, data);
                }

                // treeFactory
                treeFactory = TreeFactory.GetFactory();
                // pipeline 로드
                treePipeline = Resources.Load<Pipeline>("TreePipeline");
                #region 기존 코드
                //pipeline = treeFactory.LoadPipeline(runtimePipelineResourcePath);
                //// pipeline에서 positioner 요소 가져오기(위치 동적 할당)
                //if (pipeline != null && pipeline.Validate())
                //{
                //        positionerElement = (PositionerElement)pipeline.root.GetDownstreamElement(PipelineElement.ClassType.Positioner);
                //        positionerElement.positions.Clear();
                //}
                #endregion

        }

        void Update()
        {
                // 2. 발아
                //if (dayCount == 2) TreeDataUpdate()
                // 3. 묘목
                //if (dayCount ==3) TreeDataUpdate()
                // 4. 나무
                //if (dayCount == 4) TreeDataUpdate()
                // 5. 개화
                if (dayCount == 5) TreeDataUpdate(10, 30, 40, 2, 10);



                // TreePipeline - 가지 추가
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                        print("가지 추가");
                        treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minFrequency = 20;
                        treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.maxFrequency = 20;
                        // Tree 다시 Load
                        Debug.Log("LoadPipelineAsset");
                        string pathToAsset = Application.streamingAssetsPath + "/TreePipeline.asset";
                        Broccoli.Pipe.Pipeline loadedPipeline = treePipeline;
                        treeFactory.UnloadAndClearPipeline();  // pipeline 초기화
                        treeFactory.LoadPipeline(loadedPipeline.Clone(), pathToAsset, true, true);
                        Resources.UnloadAsset(loadedPipeline);
                        // 이전 Tree 삭제
                        Destroy(growPos.GetChild(0).gameObject);
                        // 새로 Load한 Tree 위치시키기
                        //treeFactory.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                        //treeFactory.gameObject.transform.Rotate(new Vector3(0, 0, 0));
                        treeFactory.gameObject.transform.parent = growPos;

                }

                // TreePipeline -  잎 추가
        }


        /// <summary>
        /// 나무 정보 업데이트
        /// </summary>
        /// /// <param name="branchNum">가지 개수</param>
        /// <param name="lengthBase">Base나무 길이</param>
        /// /// <param name="lengthTop">Top 나무 길이</param>
        /// <param name="thick">나무 굵기</param>
        /// <param name="abundance">나무 풍성함</param>
        public void TreeDataUpdate(float branchNum, float lengthBase, float lengthTop, float thick, float abundance)
        {
                // 가지 개수
                int levelCount = treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels.Count;
                for (int i=0; i< levelCount; i++)
                {
                        if (i < 2)
                        {
                                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minFrequency = (int)branchNum;
                                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].maxFrequency = (int)branchNum;
                                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].maxLengthAtBase = (int)branchNum;
                                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minLengthAtBase = (int)branchNum;
                        }
                        else
                        {
                                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minFrequency = (int)branchNum/ 2;
                                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].maxFrequency = (int)branchNum / 2;
                                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].maxLengthAtBase = (int)branchNum / 2;
                                treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[0].minLengthAtBase = (int)branchNum / 2;
                        }
                }
                
                // 나무 길이
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.maxLengthAtBase = lengthBase;
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minLengthAtBase = lengthBase;
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minLengthAtTop = lengthTop;
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minLengthAtTop = lengthTop;

                // 나무 굵기
                treePipeline._serializedPipeline.girthTransforms[0].minGirthAtBase = thick;
                treePipeline._serializedPipeline.girthTransforms[0].maxGirthAtBase = thick;

                // 나무의 풍성함
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.minFrequency = (int) abundance;
                treePipeline._serializedPipeline.structureGenerators[0].rootStructureLevel.maxFrequency = (int) abundance;


        }
}
