
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;


//// Tree 관련 데이터 클래스
//// [System.Serializable]
//public class TreeData
//{
//        public string treeName;
//}


//// User가 inputField에 자신의 나무 이름 입력하면 해당 데이터 저장
//public class TreeInfo : MonoBehaviour
//{
//        public InputField inputTreeName;
//        public TreeData tree;
//        public Text txtTreeName;

//        private void Start()
//        {
//                inputTreeName.onSubmit.AddListener(OnSubmit);
//                tree = new TreeData();
//        }

//        public void OnSubmit(string name)
//        {
//                tree.treeName = name;
//                inputTreeName.gameObject.SetActive(false);
//                txtTreeName.text = $"Tree Name : {name}";
//                // 이름 입력하면 씨앗 심기로 넘어가기
//                GetComponent<TreeGrow>().btnPlantSeed.gameObject.SetActive(true);
//                GetComponent<TreeGrow>().day = 1;
//        }

//        //private void Update()
//        //{
//        //        print(tree.treeName);
//        //}

//}
