using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Chatting : MonoBehaviour
{
    public Transform windowContent;
    //private void Start()
    //{
    //    if(PhotonNetwork.PlayerList.Length <= 1)
    //    {
    //        transform.GetChild(0).gameObject.SetActive(false);
    //    }
    //}
    public void OnClickEmojiButton(int i)
    {
        //if (PhotonNetwork.PlayerList.Length <= 1) return;

        GameObject emojiResource = Resources.Load<GameObject>("Emoji");
        GameObject emojiPrefab = Instantiate(emojiResource);
        emojiPrefab.transform.parent = windowContent.transform;
        Sprite emojiImgResource = Resources.Load<Sprite>("Emoji_image/Emoji_" + i);
        Sprite emoji = Instantiate(emojiImgResource);
        emojiPrefab.GetComponent<Image>().sprite = emoji;
    }

    public void OnClickChatActive()
    {
        //transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position =
        //왼쪽으로 140정도 이동
    }
}
