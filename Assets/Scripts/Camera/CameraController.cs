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

    private void Start()
    {
        user = GameManager.Instance.User;
        userInput = user.GetComponent<UserInput>();
        myCamera = GetComponent<Camera>();
    }
    private void Update()
    {
        //zoom�� ���� ���
        if (userInput.Zoom != 0)
        {
            //ī�޶� ������
            StopAllCoroutines();
            StartCoroutine(CameraMoving(7));
            //ī�޶� Zoom in / out
            myCamera.orthographicSize -= userInput.Zoom * wheelScrollSpeed;
            myCamera.orthographicSize = Mathf.Clamp(myCamera.orthographicSize, 2.5f, 15f);
        }

        //�÷��̾�� ī�޶��� �Ÿ��� 5���� �������� ���󰡱�
        if(Vector3.Distance(camPos.position, transform.position) > 5)
        {
            StartCoroutine(CameraMoving());
        }
        transform.parent.Rotate(user.transform.up, userInput.Rotate);
    }
    //ī�޶� ������ �ڷ�ƾ
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
