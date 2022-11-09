using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Chatting : MonoBehaviourPun
{
    public Transform windowContent;

    //InputChat
    private InputField chatting;
    //ChatItem 공장
    private GameObject chatPrefab;
    //ScrollView의 Content
    private RectTransform content;
    //이전 Content의 H (멘토님 설명 중 H2역할)
    private float prevContentH;
    //ScrollView의 H (멘토님 설명 중 H1역할)
    private RectTransform trScrollView;
    //내 아이디 색
    Color32 idColor;

    private Vector3 startPos;
    private Vector3 endPos;
    private void Awake()
    {
        trScrollView = transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<RectTransform>();
        chatting = transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<InputField>();
        chatPrefab = Resources.Load<GameObject>("Chat_Text");
        content = transform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>();
        startPos = transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position + new Vector3(-140, 0, 0);
        endPos = transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().position;
        
        //InputField에서 엔터를 쳤을 때 호출되는 함수 등록
        //chatting.onSubmit.AddListener(OnSubmit);
        //idColor를 랜덤하게
        idColor = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), 255);
    }
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
    public void OnSubmit()
    {
        string s = chatting.text;
        //<color=#색깔코드>닉네임</color>
        string chat = "<color=#" + ColorUtility.ToHtmlStringRGB(idColor) + ">"
            + PhotonNetwork.NickName
            + "</color>"
            + ": " + s;
        photonView.RPC("RpcAddChat", RpcTarget.All, chat);

        //방법1. InputField 내용 초기화 하는법
        //chatting.text = "";

        //방법2. InputField 계속해서 쓸 수 있게 Focus하는법
        chatting.ActivateInputField();
    }

    IEnumerator AutoScrollBotton()
    {
        yield return null;
        //trScrollView H보다 Content H값이 커지면(스크롤 상태)
        if (content.sizeDelta.y > trScrollView.sizeDelta.y)
        {
            //4. Content가 바닥에 닿아 있었다면 => 누가 끝에서 채팅을 쳤단 얘기
            if (content.anchoredPosition.y >= prevContentH - trScrollView.sizeDelta.y)
            {
                //5. Content의 y값을 다시 설정해주자
                content.anchoredPosition = new Vector2(0, content.sizeDelta.y - trScrollView.sizeDelta.y);
            }
        }
    }
    #region RPC
    [PunRPC]
    public void RpcAddChat(string rpcChat)
    {
        //0.바뀌기 전의 Content H값을 넣자
        prevContentH = content.sizeDelta.y;

        //Chat 추가한다!
        //1. ChatItem을 만든다(부모를 ScrollView의 Content)
        GameObject chat = Instantiate(chatPrefab, content);
        //2. 만든 ChatItem에서 ChatItem 컴포넌트를 가져온다.
        //3. 가져온 컴포넌트에 s를 셋팅
        chat.GetComponent<ChatItem>().SetText(rpcChat);
        StartCoroutine(AutoScrollBotton());
    }

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
