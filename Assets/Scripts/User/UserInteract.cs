using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UserInteract : MonoBehaviourPun, IPunObservable
{
    public float moveSpeed = 3f;
    
    public bool moveControl;
    private UserInput userInput;
    private Animator animatorUser;
    private Image profileImage;

    private Vector3 receivePos;
    private Quaternion receiveRot;
    private void Awake()
    {
        userInput = GetComponent<UserInput>();
        profileImage = transform.GetChild(3).GetChild(0).GetComponent<Image>();
        animatorUser = transform.GetChild(2).GetComponent<Animator>();
    }
    private void Start()
    {
        if (photonView.IsMine)
        {
            transform.GetChild(4).GetChild(5).gameObject.SetActive(false);
            photonView.RPC("RPC_SettingProfile", RpcTarget.AllBuffered, DataTemporary.MyUserData.profileImg, DataTemporary.MyUserData.nickname);
        }
        
        transform.GetChild(3).gameObject.SetActive(false);

    }
    int prePlayers = 0;
    private void Update()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount != prePlayers)
        {
            prePlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            //혼자 있을 시 프로필 비활성화
            if (prePlayers < 2)
            {
                photonView.RPC("RPC_SetActive_Profile", RpcTarget.All, false);
                //transform.GetChild(3).gameObject.SetActive(false);
            }
            else
            {
                photonView.RPC("RPC_SetActive_Profile", RpcTarget.All, true);
                //transform.GetChild(3).gameObject.SetActive(true);
            }
        }


        if (!moveControl)
        {
            #region Player Move

            if (photonView && photonView.IsMine)
            {
                Vector3 moveDir;
#if UNITY_STANDALONE
                    moveDir = userInput.MoveX * transform.right + userInput.MoveZ * transform.forward;
#elif UNITY_IOS || UNITY_ANDROID
                    moveDir = userInput.MoveX * Vector3.right + userInput.MoveZ * Vector3.forward;
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
                //transform.Rotate(transform.up, userInput.DragX);
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
        Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y +  0.3f , transform.position.z), -transform.up * 100);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        LayerMask layer = 1 << LayerMask.NameToLayer("User");
        if (Physics.Raycast(ray, out hit, 100f,~layer))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                return hit.transform.gameObject;
        }
        return null;
    }
    bool isActiveProfile = false;
    public void OnClickMyProfile()
    {
        if (!isActiveProfile)
        {
            transform.GetChild(4).gameObject.SetActive(true);
            isActiveProfile = true;
        }
        else
        {
            transform.GetChild(4).gameObject.SetActive(false);
            isActiveProfile = false;
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
    public void RPC_SetActive_Profile(bool isActive)
    {
        if(isActive)
            transform.GetChild(3).gameObject.SetActive(true);
        else
            transform.GetChild(3).gameObject.SetActive(false);
    }

    [PunRPC]
    public async void RPC_SettingProfile(string imgURL, string nickName)
    {
        Texture2D texture = await DataModule.WebrequestTextureGet(imgURL, DataModule.NetworkType.GET);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 10);
        profileImage.sprite = sprite;
        transform.GetChild(4).GetChild(1).GetComponent<Image>().sprite = sprite;
        transform.GetChild(4).GetChild(2).GetComponent<Text>().text = nickName;

    }

    [PunRPC]
    public void RPC_WalkAnimation(bool isActive)
    {
        animatorUser.SetBool("Walk", isActive);
    }
    #endregion
}
