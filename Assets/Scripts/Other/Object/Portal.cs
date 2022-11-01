using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;
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
        Debug.Log("1");
        GameObject land = user.OnLand();

        Debug.Log("2");
        SceneManager.LoadScene(land.name);
        Debug.Log("3");
    }
}
