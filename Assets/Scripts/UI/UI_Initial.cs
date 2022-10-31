using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Initial : MonoBehaviour
{
    #region Variable

    public GameObject profile;
    public GameObject TreeList;
    

    #endregion


    /// <summary>
    /// 프로필 UI 활성화
    /// </summary>
    public void onProfile()
    {
        profile.gameObject.SetActive(true);
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
