using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
using Unity.VisualScripting;

public class UserInteract : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float moveSpeed = 3f;
    
    public bool moveControl;
    private UserInput userInput;
    private Animator animatorUser;
    private Image profileImage;

    private Vector3 receivePos;
    private Quaternion receiveRot;
    private Dictionary<string, Sprite> dic = new Dictionary<string, Sprite>();
    private int userNum = 0;
    private void Awake()
    {
        userInput = GetComponent<UserInput>();
        profileImage = transform.GetChild(3).GetChild(0).GetComponent<Image>();
        animatorUser = transform.GetChild(2).GetComponent<Animator>();
    }
    private void Start()
    {
        if(photonView.IsMine)
            photonView.RPC("RPC_SettingProfile", RpcTarget.AllBuffered, DataTemporary.MyUserData.profileImg);

    }
    private void Update()
    {
        if (!moveControl)
        {
            #region Player Move

            if (photonView && photonView.IsMine)
            {
#if UNITY_STANDALONE
            Vector3 moveDir = userInput.MoveX * transform.right + userInput.MoveZ * transform.forward;
#elif UNITY_IOS || UNITY_ANDROID
            Vector3 moveDir = userInput.MoveX * Vector3.right + userInput.MoveZ * Vector3.forward;
#endif
                moveDir.Normalize();

                if (moveDir.magnitude != 0)
                {
                    photonView.RPC("RPC_WalkAnimation", RpcTarget.All, true);
                }
                else
                {
                    photonView.RPC("RPC_WalkAnimation", RpcTarget.All, false);
                }

                transform.GetChild(2).LookAt(transform.position + moveDir * 10);
                transform.position += moveSpeed * moveDir * Time.deltaTime;

                //회전
                transform.Rotate(transform.up, userInput.RotateX);
            }
            else
            {
                transform.position =
                    Vector3.Lerp(transform.position, receivePos, Time.deltaTime * 5);
                transform.GetChild(2).rotation =
                    Quaternion.Lerp(transform.GetChild(2).rotation, receiveRot, Time.deltaTime * 5);
            }
            #endregion
        }

        #region Player Click
        if (userInput.Interact)
        {
            if (LandDataManager.Instance)
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.GetChild(2).rotation);
        }
        else
        {
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRot = (Quaternion)stream.ReceiveNext();
        }
    }
    #region RPC

    //[PunRPC]
    //public void RPC_ChoiceCharactorLoad()
    //{
    //    if (transform.childCount == 2)
    //    {
    //        string model = PhotonNetwork.PlayerList[(int)photonView.ViewID.ToString()[0] - 49].NickName.Split('/')[1];
    //        //userAvatar 생성
    //        GameObject userAvatarResource = Resources.Load<GameObject>("Charactor/" + model);
    //        //Debug.Log(DataTemporary.MyUserData.UserAvatar);

    //        GameObject userAvatar = Instantiate(userAvatarResource);
    //        userAvatar.name = userAvatar.name.Split("(")[0];
    //        userAvatar.transform.parent = transform;
    //        userAvatar.transform.localPosition = Vector3.zero;
    //        userAvatar.transform.localEulerAngles = Vector3.zero;
    //        animator = transform.GetChild(2).GetComponent<Animator>();
    //    }
    //}

    [PunRPC]
    public async void RPC_SettingProfile(string imgURL)
    {
        Texture2D texture = await DataModule.WebrequestTexture(imgURL);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 10);
        profileImage.sprite = sprite;
    }

    [PunRPC]
    public void RPC_WalkAnimation(bool isActive)
    {
        animatorUser.SetBool("Walk", isActive);
    }
    #endregion
}
