using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Initial : MonoBehaviour
{
    #region Variable

    public GameObject profileUI;
    public GameObject TreeListUI;
    public GameObject screenShotCam;
    public GameObject sleepDataUI;
    public GameObject plantNameUI;

    public Transform previewTreePos;

    public Text txtAge;
    public Text treeName;

    public Button btnPlantName;

    public InputField inputPlantName;

    public TimeManager timeManager;

    #endregion



    private void Start()
    {
        inputPlantName.onValueChanged.AddListener(onValueChanged);
        if (!PhotonNetwork.IsMasterClient)
        {
            gameObject.SetActive(false);
        }

    }

    /// <summary>
    /// 프로필 UI 활성화
    /// </summary>
    public void onProfile()
    {
        profileUI.gameObject.SetActive(true);
        for(int i = 0; i < profileUI.transform.GetChild(1).childCount; i++)
        {
            if (profileUI.transform.GetChild(1).GetChild(i).name == DataTemporary.MyUserData.UserAvatar)
            {
                profileUI.transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                profileUI.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
            }
        }
        // Tree Age 변경
        txtAge.text = $"{timeManager.totalPlantDay}세";
        //screenShotCam.transform.parent = previewTreePos;
        //screenShotCam.transform.localPosition = new Vector3(0.42f, 13.73f, -26.06f);
    }
    /// <summary>
    /// 프로필 UI 비활성화
    /// </summary>
    public void onProfileOff()
    {
        profileUI.gameObject.SetActive(false);
    }


    /// <summary>
    /// TreeList UI 활성화
    /// </summary>
    public void onTreeList()
    {
        TreeListUI.gameObject.SetActive(true);
    }
    /// <summary>
    /// TreeList UI 비활성화
    /// </summary>
    public void onTreeListOff()
    {
        TreeListUI.gameObject.SetActive(false);
    }

    /// <summary>
    /// MyWorld Scene으로 이동
    /// </summary>
    public void OnClickToWorld()
    {
        PhotonNetwork.LoadLevel("MyWorld");
    }

    /// <summary>
    /// 수면데이터 분석 UI 활성화
    /// </summary>
    public void OnSleepData()
    {
        sleepDataUI.SetActive(true);
    }
    /// <summary>
    /// 수면데이터 분석 UI 비활성화
    /// </summary>
    public void OnSleepDataOff()
    {
        sleepDataUI.SetActive(false);
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
        treeName.text = inputPlantName.text;
        plantNameUI.SetActive(false);
    }
}
