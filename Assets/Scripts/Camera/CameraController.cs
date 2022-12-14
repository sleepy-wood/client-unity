using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviourPun
{
    [Header("Follow Player CamPos")]
    private Transform camPos;

    [Header("Zoom In / Out")]
    [SerializeField] private float wheelScrollSpeed;

    private GameObject user;
    private UserInput userInput;
    private Camera myCamera;
    private float initialOrthographicSize = 0;

    private void Start()
    {
        user = GameManager.Instance.User;
        myCamera = GetComponent<Camera>();
        initialOrthographicSize = myCamera.fieldOfView;
    }
    private bool isOnce = false;
    public float preZoom = 0;
    private void Update()
    {
        if (!user)
        {
            user = GameManager.Instance.User;
        }
        else
        {
            if (!isOnce)
            {
                camPos = user.transform.GetChild(0);
                userInput = user.GetComponent<UserInput>();
                isOnce = true;
            }
            if (photonView.IsMine)
            {
                if (user && !user.GetComponent<UserInteract>().moveControl)
                {
                    Vector3 moveDir = userInput.MoveX * Vector3.right + userInput.MoveZ * Vector3.forward;
                    ////zoom을 했을 경우
                    //if (userInput.Zoom != 0 && preZoom != userInput.Zoom)
                    //{
                    //    preZoom = userInput.Zoom;
                    //    //카메라 재조정
                    //    StopAllCoroutines();
                    //    StartCoroutine(CameraMoving(7));

                    //    //카메라 Zoom in / out
                    //    myCamera.fieldOfView -= userInput.Zoom * wheelScrollSpeed;
                    //    myCamera.fieldOfView = Mathf.Clamp(myCamera.fieldOfView, initialOrthographicSize - 15, initialOrthographicSize + 30);
                    //}

                    transform.position = camPos.position;
                    //플레이어와 카메라의 거리가 5정도 떨어지면 따라가기
                    //if (Vector3.Distance(camPos.position, transform.position) > 1)
                    //{
                    //    StartCoroutine(CameraMoving());
                    //}
                    //transform.forward = camPos.forward;
                    //transform.parent.Rotate(user.transform.up, userInput.RotateX);

                    //transform.position += user.GetComponent<UserInteract>().moveSpeed * moveDir * Time.deltaTime;

                }
                else
                {
                    StopAllCoroutines();
                }
            }

        }
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
