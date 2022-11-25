using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UI_PlantName : MonoBehaviour
{

    public InputField inputPlantName;
    public Button btnPlantName;

    public GameObject treeNameUI;
    public GameObject chatUI;
    public GameObject landCanvas;
    public GameObject treeNameBG;

    private void Start()
    {
        inputPlantName.onValueChanged.AddListener(onValueChanged);
    }

    //bool once2;
    //private void Update()
    //{
    //    if (SceneManager.GetActiveScene().name == "SkyLand" && !once2)
    //    {
    //        // Land 입장 시 UI 비활성화
    //        landCanvas.SetActive(false);
    //        chatUI.SetActive(false);
    //        treeNameUI.SetActive(false);
    //    }
    //}


    /// <summary>
    /// 나무 이름 입력하면 다음 버튼 활성화
    /// </summary>
    /// <param name="s">식물 이름</param>
    void onValueChanged(string s)
    {
        btnPlantName.interactable = true;
    }

    /// <summary>
    /// 나무 이름 결정 버튼 누르면 나무 이름 저장 & UI 비활성화 & 
    /// </summary>
    public void onConfirmPlantName()
    {
        // My Collection
        // 나무 이름
        GameManager.Instance.treeController.treeName = inputPlantName.text;
          
        // TreeData 저장
        GameManager.Instance.treeController.SaveTreeData();
        print("나무 이름 결정 완료");
        // UI 버튼들 활성화
        landCanvas.SetActive(true);
        chatUI.SetActive(true);
        // 나무 이름 결정 UI 비활성화
        treeNameUI.SetActive(false);
        treeNameBG.SetActive(false);
    }
}
