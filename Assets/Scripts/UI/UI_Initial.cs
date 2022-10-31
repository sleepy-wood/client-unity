using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Initial : MonoBehaviour
{
    #region Variable

    public GameObject profile;
    public GameObject TreeList;
    public GameObject screenShotCam;
    public Transform previewTreePos;
    public GameObject SleepData;
    public Text sleepDataText;
    public Text txtAge;
    public TimeManager timeManager;
    #endregion


    /// <summary>
    /// 프로필 UI 활성화
    /// </summary>
    public void onProfile()
    {
        profile.gameObject.SetActive(true);
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
        profile.gameObject.SetActive(false);
    }


    /// <summary>
    /// TreeList UI 활성화
    /// </summary>
    public void onTreeList()
    {
        TreeList.gameObject.SetActive(true);
    }
    /// <summary>
    /// TreeList UI 비활성화
    /// </summary>
    public void onTreeListOff()
    {
        TreeList.gameObject.SetActive(false);
    }


    /// <summary>
    /// SleepData UI 활성화
    /// </summary>
    public void OnClickSleepDataOn()
    {
            sleepDataText.text = DataTemporary.MyUserData.SleepData.ToString();
            SleepData.SetActive(true);
    }
    /// <summary>
    /// SleepData UI 비활성화
    /// </summary>
    public void OnClickSleepDataOff()
    {
        SleepData.SetActive(false);
    }

    /// <summary>
    /// MyWorld Scene으로 이동
    /// </summary>
    public void OnClickToWorld()
    {
        SceneManager.LoadScene("MyWorld");
    }
}
