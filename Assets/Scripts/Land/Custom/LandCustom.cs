using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LandCustom : MonoBehaviour
{
    enum SelectState
    {
        None,
        Selected
    }
    public enum EditType
    {
        Camera,
        Move,
        Rotate,
        Scale
    }

    [Header("Zoom In / Out")]
    [SerializeField] private float zoomSpeed = 20;
    [Header("Edit Custom Tool")]
    [SerializeField] private GameObject editButton;

    //현재 고른 상태인가?
    private SelectState selectState = SelectState.None;
    //Edit type 설정
    public EditType editType = EditType.Camera;
    private GameObject selectedObject;
    private UserInput userInput;
    private Camera camera;
    private float initialOrthographicSize = 0;
    private void Start()
    {
        userInput = GetComponent<UserInput>();
        camera = GetComponent<Camera>();
        initialOrthographicSize = camera.fieldOfView;
    }
    private LayerMask preLayer;
    private void Update()
    {
        //선택하기 전
        if (selectState == SelectState.None)
        {
            //선택
            editButton.SetActive(false);
            if (userInput.Interact)
            {
                Vector3 mousePos = Input.mousePosition;
                Ray ray = camera.ScreenPointToRay(mousePos);
                RaycastHit hit;
                LayerMask layer = 1 << LayerMask.NameToLayer("Ground");
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layer))
                {
                    selectedObject = hit.transform.parent != null ? hit.transform.parent.gameObject : hit.transform.transform.gameObject;
                    preLayer = hit.transform.gameObject.layer;
                    if (hit.transform.parent != null)
                    {
                        hit.transform.parent.gameObject.layer = LayerMask.NameToLayer("Selected");
                        for (int i = 0; i < hit.transform.parent.childCount; i++)
                        {
                            hit.transform.parent.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Selected");
                        }
                        //아웃라인 켜기
                        hit.transform.parent.GetComponent<Outline>().enabled = true;
                        for (int i = 0; i < hit.transform.parent.childCount; i++)
                        {
                            if (hit.transform.parent.GetChild(i).GetComponent<Outline>())
                            {
                                hit.transform.parent.GetChild(i).GetComponent<Outline>().enabled = true;
                            }
                        }
                    }
                    else
                    {
                        hit.transform.gameObject.layer = LayerMask.NameToLayer("Selected");
                        if (hit.transform.GetComponent<Outline>())
                        {
                            hit.transform.GetComponent<Outline>().enabled = true;
                        }
                    }
                    Debug.Log("Select = " + hit.transform);
                    selectState = SelectState.Selected;
                }
            }
        }
        else
        {
            editButton.SetActive(true);
            if (userInput.Interact)
            {
                if (!isActiveMove)
                {
                    //선택 취소
                    Vector3 mousePos = Input.mousePosition;
                    Ray ray = camera.ScreenPointToRay(mousePos);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        if (hit.transform.gameObject.layer == selectedObject.layer)
                        {
                            Debug.Log("Not Selected = " + selectedObject.transform);
                            if (selectedObject.transform.parent != null)
                            {
                                selectedObject.transform.parent.gameObject.layer = preLayer;
                                //부모 아웃라인 끄기
                                if (selectedObject.transform.parent.GetComponent<Outline>())
                                {
                                    selectedObject.transform.parent.GetComponent<Outline>().enabled = false;
                                }
                                for (int i = 0; i < selectedObject.transform.parent.childCount; i++)
                                {
                                    selectedObject.transform.parent.GetChild(i).gameObject.layer = preLayer;
                                    if (selectedObject.transform.parent.GetChild(i).GetComponent<Outline>())
                                    {
                                        selectedObject.transform.parent.GetChild(i).GetComponent<Outline>().enabled = false;
                                    }
                                }
                            }
                            //아웃라인 끄기
                            if (selectedObject.transform.GetComponent<Outline>())
                            {
                                selectedObject.transform.GetComponent<Outline>().enabled = false;
                            }
                            selectedObject.layer = preLayer;
                            selectedObject = null;
                            selectState = SelectState.None;
                        }
                    }
                }
                else if (isActiveMove)
                {
                    isActiveMove = false;
                }
            }
            else
            {
                switch (editType)
                {
                    case EditType.Move:
                        MoveMode();
                        break;
                    case EditType.Rotate:
                        RotateMode();
                        break;
                    case EditType.Scale:
                        ScaleMode();
                        break;
                    default:
                        break;
                }
            }
        }

        switch (editType)
        {
            case EditType.Camera:
                CameraMode();
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 카메라 모드
    /// </summary>
    void CameraMode()
    {
        transform.parent.Rotate(new Vector3(0, 1, 0), userInput.RotateX * 10);
        transform.Rotate(new Vector3(transform.parent.right.x, 0, 0), userInput.RotateY * 3);
        //zoom을 했을 경우
        if (userInput.Zoom != 0)
        {
            //카메라 Zoom in / out
            camera.fieldOfView -= userInput.Zoom * zoomSpeed;
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, initialOrthographicSize - 30, initialOrthographicSize + 30);
        }
    }


    /// <summary>
    /// 오브젝트 스케일 조정
    /// </summary>
    void ScaleMode()
    {
        selectedObject.transform.localScale += new Vector3(userInput.Zoom, userInput.Zoom, userInput.Zoom);
    }

    /// <summary>
    /// 오브젝트 회전
    /// </summary>
    void RotateMode()
    {
        selectedObject.transform.Rotate(selectedObject.transform.up, -userInput.RotateX * 3);
    }

    bool isActiveMove = false;
    /// <summary>
    /// 오브젝트 움직이기
    /// </summary>
    void MoveMode()
    {
#if UNITY_STANDALONE
        if (isActiveMove)
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = camera.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    Vector3 pos = hit.point;
                    selectedObject.transform.position = pos;
                }
            }
        }

#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount == 1)
        {
            for (int i = 0; i < Input.touches.Length; i++)
            {
                if (Input.touches[i].phase == TouchPhase.Moved)
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        Vector3 mousePos = Input.mousePosition;
                        Ray ray = camera.ScreenPointToRay(mousePos);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                            {
                                Vector3 pos = hit.point;
                                selectedObject.transform.position = pos;
                            }
                        }
                    }
                }
            }
        }
#endif
    }

    public void OnClickEditSelect(int i)
    {
        switch (i)
        {
            case 1:
                editType = EditType.Camera;
                break;
            case 2:
                editType = EditType.Move;
                isActiveMove = true;
                break;
            case 3:
                editType = EditType.Rotate;
                break;
            case 4:
                editType = EditType.Scale;
                break;
            default:
                break;

        }
    }

}
