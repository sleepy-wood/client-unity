using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Player CamPos")]
    [SerializeField] private Transform camPos;

    [Header("Zoom In / Out")]
    [SerializeField] private float wheelScrollSpeed;

    private GameObject user;
    private UserInput userInput;
    private Camera myCamera;
    private float initialOrthographicSize = 0;

    private void Start()
    {
        user = GameManager.Instance.User;
        userInput = user.GetComponent<UserInput>();
        myCamera = GetComponent<Camera>();
        initialOrthographicSize = myCamera.orthographicSize;
    }
    private void Update()
    {
        //zoom을 했을 경우
        if (userInput.Zoom != 0)
        {
            //카메라 재조정
            StopAllCoroutines();
            StartCoroutine(CameraMoving(7));
           
            //카메라 Zoom in / out
            myCamera.orthographicSize -= userInput.Zoom * wheelScrollSpeed;
            myCamera.orthographicSize = Mathf.Clamp(myCamera.orthographicSize, initialOrthographicSize - 12, initialOrthographicSize + 12);
        }

        //플레이어와 카메라의 거리가 5정도 떨어지면 따라가기
        if(Vector3.Distance(camPos.position, transform.position) > 5)
        {
            StartCoroutine(CameraMoving());
        }
        transform.parent.Rotate(user.transform.up, userInput.Rotate);
    }
    //카메라 움직임 코루틴
    IEnumerator CameraMoving(float speed = 1)
    {
        while (Vector3.Distance(camPos.position, transform.position) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, camPos.position, Time.deltaTime);
            yield return null;
        }
        yield return null;
    }

}
