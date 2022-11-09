using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Chatting : MonoBehaviourPun
{
    public Transform windowContent;

    private Vector3 startPos;
    private Vector3 endPos;
    private void Update()
    {
        if (PhotonNetwork.PlayerList.Length <= 1)
        {
            //transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            //transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    private void Awake()
    {
        startPos = transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position + new Vector3(-140, 0, 0);
        endPos = transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position;
    }
    public void OnClickEmojiButton(int i)
    {
        string model = "";
        UserInteract[] users = GameObject.FindObjectsOfType<UserInteract>();
        for (int j = 0; j < users.Length; j++)
        {
            if (users[j].GetComponent<PhotonView>().IsMine)
                model = PhotonNetwork.PlayerList[(int)users[j].GetComponent<PhotonView>().ViewID.ToString()[0] - 49].NickName.Split('/')[1]; 
        }
        //if (PhotonNetwork.PlayerList.Length <= 1) return;
        photonView.RPC("RPC_EmojiButton", RpcTarget.All, i, model);

    }
    bool isChatActive = false;
    /// <summary>
    /// Chatting Button을 눌렀을 때, 판넬을 활성화
    /// </summary>
    public void OnClickChat()
    {
        if (isChatActive)
        {
            isChatActive = false;
            StopAllCoroutines();
            //오른쪽으로 140정도 이동
            StartCoroutine(ChatActive(endPos));
        }
        else
        {
            isChatActive = true;
            StopAllCoroutines();
            //왼쪽으로 140정도 이동
            StartCoroutine(ChatActive(startPos));
        }
    }
    private IEnumerator ChatActive(Vector3 endPosition)
    {
        while (true)
        {
            transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position = 
                Vector3.Lerp(transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position, endPosition, Time.deltaTime * 5f);
            
            if( Vector3.Distance(transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position,endPosition) < 0.1f)
            {
                yield break;
            }

            yield return null;
        }
    }

    #region RPC
    [PunRPC]
    public void RPC_EmojiButton(int i, string model)
    {
        GameObject emojiResource = Resources.Load<GameObject>("Emoji");
        GameObject emojiPrefab = Instantiate(emojiResource);

        emojiPrefab.transform.parent = windowContent.transform;

        Sprite emojiImgResource = Resources.Load<Sprite>("Emoji_image/Emoji_" + i);
        Sprite emoji = Instantiate(emojiImgResource);

        Sprite emoji_ProfileResource = Resources.Load<Sprite>("Charactor_Img/" + model);
        Sprite emoji_Profile = Instantiate(emoji_ProfileResource);

        emojiPrefab.transform.GetChild(0).GetComponent<Image>().sprite = emoji;
        emojiPrefab.GetComponent<Image>().sprite = emoji_Profile;
    }

    #endregion
}
