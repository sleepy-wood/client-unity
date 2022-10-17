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
    private UserInput userInput;
    private Camera myCamera;

    private void Start()
    {
        userInput = GameManager.Instance.User.GetComponent<UserInput>();
        myCamera = GetComponent<Camera>();
    }
    private void Update()
    {
        myCamera.orthographicSize -= userInput.Zoom * wheelScrollSpeed;
        myCamera.orthographicSize = Mathf.Clamp(myCamera.orthographicSize, 2.5f, 15f);

        if(Vector3.Distance(camPos.position, transform.position) > 5)
        {
            StartCoroutine(CameraMoving());
        }
    }
    IEnumerator CameraMoving()
    {
        while (Vector3.Distance(camPos.position, transform.position) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, camPos.position, Time.deltaTime);
            yield return null;
        }
        yield return null;
    }

}
