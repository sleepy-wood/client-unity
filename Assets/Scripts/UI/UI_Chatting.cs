using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Chatting : MonoBehaviour
{
    public Transform windowContent;

    private Vector3 startPos;
    private Vector3 endPos;
    //private void Start()
    //{
    //    if(PhotonNetwork.PlayerList.Length <= 1)
    //    {
    //        transform.GetChild(0).gameObject.SetActive(false);
    //    }
    //}
    private void Awake()
    {
        startPos = transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position + new Vector3(-140, 0, 0);
        endPos = transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position;
    }
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

}
