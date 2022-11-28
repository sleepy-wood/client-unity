using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        Editor
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
    private Camera cam;
    private float initialOrthographicSize = 0;
    private void Start()
    {
        userInput = GetComponent<UserInput>();
        cam = GetComponent<Camera>();
        initialOrthographicSize = cam.fieldOfView;
    }
    private LayerMask preLayer;
    private void Update()
    {
        if (selectedObject != null)
        {
            if (selectedObject.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                selectedObject.gameObject.layer = preLayer;
                selectedObject = null;
                selectState = SelectState.None;
            }
        }
        //선택하기 전
        if (selectState == SelectState.None)
        {
            //선택
            editButton.transform.GetChild(1).gameObject.SetActive(false);
            if (userInput.Interact)
            {

                Vector3 mousePos = Input.mousePosition;
                Ray ray = cam.ScreenPointToRay(mousePos);
                RaycastHit hit;
                LayerMask layer = 1 << LayerMask.NameToLayer("Ground");
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layer))
                {
                    selectedObject = hit.transform.parent.name != "landDecorations" ? hit.transform.parent.gameObject : hit.transform.transform.gameObject;
                    preLayer = hit.transform.gameObject.layer;
                    if (hit.transform.parent.name != "landDecorations")
                    {
                        hit.transform.parent.gameObject.layer = LayerMask.NameToLayer("Selected");
                        for (int i = 0; i < hit.transform.parent.childCount; i++)
                        {
                            hit.transform.parent.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Selected");
                        }
                        //아웃라인 켜기
                        if (hit.transform.parent.GetComponent<Outline>())
                            hit.transform.parent.GetComponent<Outline>().enabled = true;

                        for (int i = 0; i < hit.transform.parent.childCount; i++)
                        {
                            if (hit.transform.parent.GetChild(i).GetComponent<Outline>())
                                hit.transform.parent.GetChild(i).GetComponent<Outline>().enabled = true;
                        }
                       
                    }
                    else
                    {
                        hit.transform.gameObject.layer = LayerMask.NameToLayer("Selected");

                        if (hit.transform.GetComponent<Outline>())
                        {
                            hit.transform.GetComponent<Outline>().enabled = true;
                        }

                        for (int i = 0; i < hit.transform.childCount; i++)
                        {
                            if (hit.transform.GetChild(i).GetComponent<Outline>())
                                hit.transform.GetChild(i).GetComponent<Outline>().enabled = true;
                        }

                    }
                    editType = EditType.Editor;
                    selectState = SelectState.Selected;

                }
            }
        }
        else
        {
            //if (userInput.Interact)
            //{
            //    Vector3 mousePos = Input.mousePosition;
            //    Ray ray = cam.ScreenPointToRay(mousePos);
            //    RaycastHit hit;
            //    LayerMask layer = 1 << LayerMask.NameToLayer("Ground");
            //    if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layer))
            //    {
            //        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Selected"))
            //        {
            //            selectedObject.gameObject.layer = preLayer;
            //            selectedObject = null;
            //            //아웃라인 끄기
            //            if (hit.transform.parent.GetComponent<Outline>())
            //                hit.transform.parent.GetComponent<Outline>().enabled = false;

            //            for (int i = 0; i < hit.transform.parent.childCount; i++)
            //            {
            //                if (hit.transform.parent.GetChild(i).GetComponent<Outline>())
            //                    hit.transform.parent.GetChild(i).GetComponent<Outline>().enabled = false;
            //            }
            //            selectState = SelectState.None;
            //            //editButton.transform.GetChild(1).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            //        }
            //    }
            //}

            switch (editType)
            {
                case EditType.Editor:
                    //editButton.transform.GetChild(1).GetComponent<Image>().color = new Color32(150, 150, 150, 255);
                    editButton.transform.GetChild(1).gameObject.SetActive(true);
                    RotateMode();
                    MoveMode();
                    ScaleMode();
                    break;
                default:
                    break;
            }
        }
        switch (editType)
        {
            case EditType.Camera:
                //editButton.transform.GetChild(1).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                CameraMode();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 오브젝트 확정 
    /// </summary>
    public void OnClickCheck()
    {
        Debug.Log("Click");
        if (selectedObject.transform.GetComponent<Outline>())
        {
            selectedObject.transform.GetComponent<Outline>().enabled = false;
        }
        for (int i = 0; i < selectedObject.transform.childCount; i++)
        {
            selectedObject.transform.GetChild(i).gameObject.layer = preLayer;
            if (selectedObject.transform.GetChild(i).GetComponent<Outline>())
            {
                selectedObject.transform.GetChild(i).GetComponent<Outline>().enabled = false;
            }
        }
        selectedObject.layer = preLayer;
        selectedObject = null;
        selectState = SelectState.None;
        editType = EditType.Camera;
    }
    /// <summary>
    /// 오브젝트 삭제
    /// </summary>
    public void OnClickDelete()
    {
        Debug.Log("Delete");
        Destroy(selectedObject);
        selectedObject = null;
        isActiveMove = false;
        selectState = SelectState.None;
        editType = EditType.Camera;
    }

    float preZoom = 0;
    /// <summary>
    /// 카메라 모드
    /// </summary>
    void CameraMode()
    {
        transform.parent.Rotate(new Vector3(0, 1, 0), userInput.DragX * 10);
        transform.Rotate(new Vector3(transform.parent.right.x, 0, 0), userInput.DragY * 3);
        //zoom을 했을 경우
        if (userInput.Zoom != 0 && preZoom != userInput.Zoom)
        {
            preZoom = userInput.Zoom;
            //카메라 Zoom in / out
            cam.fieldOfView -= userInput.Zoom * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, initialOrthographicSize - 30, initialOrthographicSize + 30);
        }
    }


    Vector3 preScale = Vector3.zero;
    /// <summary>
    /// 오브젝트 스케일 조정
    /// </summary>
    void ScaleMode()
    {
        if (preScale != new Vector3(userInput.Zoom, userInput.Zoom, userInput.Zoom))
        {
            preScale = new Vector3(userInput.Zoom, userInput.Zoom, userInput.Zoom);
            selectedObject.transform.localScale += new Vector3(userInput.Zoom, userInput.Zoom, userInput.Zoom) * 0.5f;
        }
    }

    /// <summary>
    /// 오브젝트 회전
    /// </summary>
    void RotateMode()
    {
        if(selectedObject != null)
            selectedObject.transform.Rotate(selectedObject.transform.up, -userInput.DragX * 2);
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
            Ray ray = cam.ScreenPointToRay(mousePos);
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
                    Vector3 mousePos = Input.mousePosition;
                    Ray ray = GetComponent<Camera>().ScreenPointToRay(mousePos);
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
#endif
    }

    /// <summary>
    /// Tool Tip 활성화 
    /// </summary>
    public void OnClickToolTipActive()
    {
        editButton.transform.GetChild(3).gameObject.SetActive(true);
    }

    /// <summary>
    /// Tool Tip 비활성화 
    /// </summary>
    public void OnClickToolTipNotActive()
    {
        editButton.transform.GetChild(3).gameObject.SetActive(false);
    }

    /// <summary>
    /// Edit 모드 변경 
    /// </summary>
    /// <param name="i"></param>
    public void OnClickEditSelect(int i)
    {
        //상태 변경
        switch (i)
        {
            case 1:
                editType = EditType.Camera;
                break;
            case 2:
                editType = EditType.Editor;
                isActiveMove = true;
                break;
            default:
                break;

        }

    }

}
