using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        GameObject land = user.OnLand();
        SceneManager.LoadScene(land.name);
    }
}
