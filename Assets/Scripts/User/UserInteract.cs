using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInteract : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    public bool moveControl;
    private UserInput userInput;
    private Animator animator;

    private void Start()
    {
        userInput = GetComponent<UserInput>();

        if (!moveControl)
        {
            //userAvatar 생성
            GameObject userAvatarResource = Resources.Load<GameObject>("Charactor/" + DataTemporary.MyUserData.UserAvatar);
            GameObject userAvatar = Instantiate(userAvatarResource);
            userAvatar.name = userAvatar.name.Split("(")[0];
            userAvatar.transform.parent = transform;
            userAvatar.transform.localPosition = Vector3.zero;
            userAvatar.transform.localEulerAngles = Vector3.zero;
            animator = transform.GetChild(2).GetComponent<Animator>();
        }
    }
    private void Update()
    {
        if (!moveControl)
        {
            #region Player Move

#if UNITY_STANDALONE
            Vector3 moveDir = userInput.MoveX * transform.right + userInput.MoveZ * transform.forward;
#elif UNITY_IOS || UNITY_ANDROID
        Vector3 moveDir = userInput.MoveX * Vector3.right + userInput.MoveZ * Vector3.forward;
#endif
            moveDir.Normalize();
            
            if(moveDir.magnitude != 0)
            {
                animator.SetBool("Walk", true);
            }
            else
            {
                animator.SetBool("Walk", false);
            }

            transform.GetChild(2).LookAt(transform.position + moveDir * 10);
            transform.position += moveSpeed * moveDir * Time.deltaTime;

            //회전
            transform.Rotate(transform.up, userInput.Rotate);
            #endregion
        }

        #region Player Click
        if (userInput.Interact)
        {
            if (LandDataManager.Instance.buildMode == LandDataManager.BuildMode.Bridge)
            {
                LayerMask layerMask = 1 << LayerMask.NameToLayer("Bridge");
                ScreenToRayClick(LandDataManager.Instance.buildBridgeCamera.GetComponent<Camera>(), layerMask);
            }
            else
            {
                LayerMask layerMask = 1 << LayerMask.NameToLayer("Ground");
                ScreenToRayClick(Camera.main, ~layerMask);
            }
        }
        #endregion
    }
    /// <summary>
    /// ClickMe 호출
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="layer"></param>
    public void ScreenToRayClick(Camera camera, LayerMask layer)
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = camera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer))
        {
            if (hit.transform.GetComponent<IClickedObject>() != null)
            {
                hit.transform.GetComponent<IClickedObject>().ClickMe();
            }
        }
    }
    /// <summary>
    /// StairMe 호출
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="layer"></param>
    public void ScreenToRayStair(Camera camera, LayerMask layer)
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = camera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer))
        {
            if (hit.transform.GetComponent<IClickedObject>() != null)
            {
                hit.transform.GetComponent<IClickedObject>().StairMe();
            }
        }
    }

    /// <summary>
    /// Player가 서있는 땅을 return
    /// </summary>
    /// <returns></returns>
    public GameObject OnLand()
    {
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y * 2, transform.position.z), -transform.up * 10);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);
        if (Physics.Raycast(ray, out hit, 10f))
        {
            return hit.transform.gameObject;
        }
        else
        {
            return null;
        }
    }
}
