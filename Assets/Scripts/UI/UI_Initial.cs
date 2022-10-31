using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Initial : MonoBehaviour
{
    #region Variable

    public GameObject profile;
    public GameObject TreeList;
    public GameObject screenShotCam;
    public Transform previewTreePos;

    #endregion


    /// <summary>
    /// 프로필 UI 활성화
    /// </summary>
    public void onProfile()
    {
        profile.gameObject.SetActive(true);
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

}
