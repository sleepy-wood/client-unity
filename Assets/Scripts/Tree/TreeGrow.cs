//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Experimental.Rendering;
//using UnityEngine.Profiling;
//using System.IO;
//using System;

//public class TreeGrow : MonoBehaviour
//{
//        public GameObject tree;
//        private Tree t;
//        public int day;
//        public Text dayText;
//        DateTime startTime;
//        public Button btnPlantSeed;
//        public GameObject seedPrefab;
//        public float downSpeed = 0.5f;


//        void Start()
//        {
//                t = GetComponent<Tree>();
//                //startTime = DateTime.Parse("2022-10-12 PM 5:07:00");
//        }

//        public bool isOnce = true;
//        void Update()
//        {
//                // Day에 따라 단계 함수 실행 (씨앗 심기/뿌리기 - 발아 - 묘목 - 나무 - 개화)
//                if (day == 1 && isOnce) btnPlantSeed.interactable = true;
//                if (day == 2 && !isOnce) Second();
//                if (day == 3 && isOnce) Third();
//                if (day == 4 && !isOnce) Fourth();
//                if (day == 5 && isOnce) Fifth();

//                // TimeSpan timeDif = DateTime.Now - startTime;
//                // print($"지난 시간 : {timeDif.Days}날");
//                dayText.text = $"Day{day}";
//        }

//        public void OnPlantSeed()
//        {
//                btnPlantSeed.gameObject.SetActive(false);
//                StartCoroutine(PlantSeed());
//        }
//        IEnumerator PlantSeed()
//        {
//                GameObject s = Instantiate(seedPrefab);
//                yield return new WaitForSeconds(0.5f);
//                while (true)
//                {
//                        s.transform.position += Vector3.down * downSpeed * Time.deltaTime;
//                        if (s.transform.position.y < -3)
//                        {
//                                Destroy(s);
//                                isOnce = false;
//                                yield break;
//                        }
//                        yield return null;
//                }
//        }
//        public void Second()
//        {
//                var tData = t.data as TreeEditor.TreeData;
//                var root = tData.root;
//                transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
//                root.seed = UnityEngine.Random.Range(0, 999999);
//                //tData.UpdateMesh(tree.transform.worldToLocalMatrix, out m);
//                Debug.Log("Current Seed: " + root.seed);
//                isOnce = true;
//        }
//        public void Third()
//        {

//        }
//        public void Fourth()
//        {

//        }
//        public void Fifth()
//        {

//        }
//}




