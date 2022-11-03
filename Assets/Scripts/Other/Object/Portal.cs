using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    private UserInteract user;
    private void Start()
    {
        user = GameManager.Instance.User.GetComponent<UserInteract>();
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.transform.name == "User")
        {
            transform.GetChild(0).gameObject.SetActive(true);

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.name == "User")
        {
            transform.GetChild(0).gameObject.SetActive(false);

        }
    }
    public void OnClickNextSceneButton()
    {
        //한개의 땅으로 데이터를 로드해서 하는 방식
        //문제점: Land 입장 시 딜레이가 발생하긴한다.
        SceneManager.LoadScene("SkyLand");
    }
}
