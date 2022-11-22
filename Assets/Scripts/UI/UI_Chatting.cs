using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Chatting : MonoBehaviourPun
{

    [SerializeField] private float chatMoveDistance = 836f;
    public RectTransform emoji_content;

    //InputChat
    private InputField chatting;
    //ChatItem 공장
    private GameObject chatPrefab;
    private GameObject emojiPrefab;
    //ScrollView의 Content
    private RectTransform content;
    //EmojiChatting의 Content
    private float prevContentH;
    private RectTransform trScrollView;
    //내 아이디 색
    private GameObject user;
    private Dictionary<string, Sprite> profileDic = new Dictionary<string, Sprite>();
    private string[] stopwords;
    
    private void Start()
    {
        photonView.RPC("RPC_ProfileList", RpcTarget.AllBuffered, DataTemporary.MyUserData.profileImg, DataTemporary.MyUserData.nickname);
        TextAsset textFile = DataTemporary.stopwordsAsset.LoadAsset<TextAsset>("stopwords");

#if UNITY_STANDALONE
        stopwords = textFile.text.Split("\r\n");
        string path = Application.dataPath + "/TextureImg";
#elif UNITY_IOS
        stopwords = textFile.text.Split("\n");
        string path = Application.persistentDataPath + "/TextureImg";
#endif
        user = GameManager.Instance.User;
        trScrollView = transform.GetChild(0).GetChild(0).GetChild(3).GetChild(0).GetComponent<RectTransform>();
        chatting = transform.GetChild(0).GetChild(0).GetChild(3).GetChild(1).GetComponent<InputField>();
        chatPrefab = Resources.Load<GameObject>("Chatting_Text");
        emojiPrefab = Resources.Load<GameObject>("Chatting_Emoji");

        content = transform.GetChild(0).GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>();

        DirectoryInfo di = new DirectoryInfo(path);
        string[] fileEntries = Directory.GetFiles(path, "*.png");

        List<string> filenames = new List<string>();
        //썸네일 넣기
        foreach (string fileName in fileEntries)
        {
#if UNITY_STANDALONE
            filenames.Add(fileName.Split("/TextureImg\\")[1].Split('.')[0]);
#elif UNITY_IOS || UNITY_ANDROID
            filenames.Add(fileName.Split("/TextureImg/")[1].Split('.')[0]);
#endif
        }
        filenames.Sort((x, y) => int.Parse(x.Split("Market_Emoji_")[1]).CompareTo(int.Parse(y.Split("Market_Emoji_")[1])));
        for (int k = 0; k < filenames.Count; k++)
        {
            int temp = k;
            //GameObject resource = fileName.Split("/TextureImg/")[1].Split('.')[0];
            GameObject resource = Resources.Load<GameObject>("Emoji_");
            GameObject prefab = Instantiate(resource, emoji_content);

            prefab.name = "Custom_" + filenames[k];
            byte[] byteTexture = File.ReadAllBytes(path + "/" + filenames[k] + ".png");
            if (byteTexture.Length > 0)
            {
                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(byteTexture);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                prefab.GetComponent<Image>().sprite = sprite;
            }
            prefab.GetComponent<Button>().onClick.AddListener(
                () => OnClickEmojiButton(temp + 15));
        }
        emoji_content.offsetMax = new Vector2(110 * (15 + filenames.Count), emoji_content.offsetMax.y);
    }
    private void Update()
    {
        if (user)
        {
            //사용자 입력 제어
            if (isActiveChat)
            {
                user.GetComponent<UserInput>().InputControl = true;
            }
            else
            {
                user.GetComponent<UserInput>().InputControl = false;
            }
        }
        //if (PhotonNetwork.PlayerList.Length <= 1)
        //{
        //    transform.GetChild(1).gameObject.SetActive(false);
        //}
        //else
        //{
            transform.GetChild(1).gameObject.SetActive(true);

            if (!transform.GetChild(1).GetChild(0).gameObject.activeSelf)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                    }
                }
            }
        //}
    }
    /// <summary>
    /// Emoji Button을 눌렀을 경우
    /// </summary>
    /// <param name="i"></param>
    public void OnClickEmojiButton(int i)
    {
        Debug.Log("Cnt = " + DataTemporary.emoji_Url.Count);
        string url = i - 15 < 0 ? null : DataTemporary.emoji_Url[i - 15];
        Debug.Log("i = " + i);
        photonView.RPC("RPC_EmojiButtonAsync", RpcTarget.All, i, DataTemporary.MyUserData.nickname, url);
    }
    public void OnSubmit()
    {
        string s = chatting.text;

        for(int i = 0;  i < stopwords.Length; i++)
        {
            if (s.Contains(stopwords[i]))
            {
                string[] texts = s.Split(stopwords[i]);
                string stopword = "";
                for (int j = 0; j < stopwords[i].Length; j++)
                    stopword += "*";
                s = texts[0];
                for(int j = 1; j < texts.Length; j++)
                {
                    s += stopword;
                    s += texts[j];
                }
            }
        }
        string chat = ": " + s;
        if(s != "")
            photonView.RPC("RpcAddChat", RpcTarget.All, chat, DataTemporary.MyUserData.nickname);

        chatting.text = "";

    }

    bool isActiveChat = false;
    public void OnClickActiveChat()
    {
        if (!isActiveChat)
        {
            if (transform.GetChild(1).GetChild(1).gameObject.activeSelf)
            {
                transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
            }
            transform.GetChild(1).GetChild(0).gameObject.SetActive(false);

            Vector2  endPos =
                new Vector2(transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition.x, 
                transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition.y + chatMoveDistance);

            Vector2 endPos2 =
                new Vector2(transform.GetChild(1).GetChild(0).GetComponent<RectTransform>().anchoredPosition.x,
                transform.GetChild(1).GetChild(0).GetComponent<RectTransform>().anchoredPosition.y + chatMoveDistance);

            StartCoroutine(ChatActive(transform.GetChild(0).GetChild(0), endPos));
            StartCoroutine(ChatActive(transform.GetChild(1).GetChild(0), endPos2));
            isActiveChat = true;
        }
        else
        {
            transform.GetChild(1).GetChild(0).gameObject.SetActive(true);

            Vector2 endPos =
                new Vector2(transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition.x,
                transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition.y- chatMoveDistance);

            Vector2 endPos2 =
                new Vector2(transform.GetChild(1).GetChild(0).GetComponent<RectTransform>().anchoredPosition.x,
                transform.GetChild(1).GetChild(0).GetComponent<RectTransform>().anchoredPosition.y - chatMoveDistance);

            StartCoroutine(ChatActive(transform.GetChild(0).GetChild(0), endPos));
            StartCoroutine(ChatActive(transform.GetChild(1).GetChild(0), endPos2));
            isActiveChat = false;
        }

    }
    private IEnumerator ChatActive(Transform activeObject ,Vector2 endPosition)
    {
        float t = 0;
        while (t < 1f)
        {
            t += 2 * Time.deltaTime;
            activeObject.GetComponent<RectTransform>().anchoredPosition =
                Vector2.Lerp(activeObject.GetComponent<RectTransform>().anchoredPosition, endPosition, t);
            yield return null;
        }
    }
    IEnumerator AutoScrollBottom()
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
    public void RpcAddChat(string rpcChat, string nickname)
    {
        StopAllCoroutines();

        //0.바뀌기 전의 Content H값을 넣자
        prevContentH = content.sizeDelta.y;

        //Chat 추가한다!
        //1. ChatItem을 만든다(부모를 ScrollView의 Content)
        GameObject chat = Instantiate(chatPrefab, content);
        //2. 만든 ChatItem에서 ChatItem 컴포넌트를 가져온다.
        //3. 가져온 컴포넌트에 s를 셋팅
        chat.transform.GetChild(0).GetComponent<ChatItem>().SetText(rpcChat);
        chat.transform.GetChild(1).GetComponent<Image>().sprite = profileDic[nickname];
        StartCoroutine(AutoScrollBottom());
        if (!isActiveChat)
        {
            transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
        }
    }

    [PunRPC]
    public async Task RPC_EmojiButtonAsync(int i, string nickname, string emoji_url)
    {
        StopAllCoroutines();
        //0.바뀌기 전의 Content H값을 넣자
        prevContentH = content.sizeDelta.y;

        GameObject emojiPre = Instantiate(emojiPrefab, content);
        Debug.Log("i = " + i);

        if (i >= 15)
        {
            Texture2D texture = await DataModule.WebrequestTextureGet(emoji_url, DataModule.NetworkType.GET);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            emojiPre.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
        }
        else
        {
            Sprite emojiImgResource = Resources.Load<Sprite>("Emoji_image/Emoji_" + i);
            Sprite emoji = Instantiate(emojiImgResource);

            emojiPre.transform.GetChild(0).GetComponent<Image>().sprite = emoji;
        }
        emojiPre.transform.GetChild(1).GetComponent<Image>().sprite = profileDic[nickname];
        if (!isActiveChat)
        {
            transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
        }

        StartCoroutine(AutoScrollBottom());
    }
    [PunRPC]
    public async void RPC_ProfileList(string url, string nickname)
    {
        Texture2D texture = await DataModule.WebrequestTextureGet(url, DataModule.NetworkType.GET);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 10);
        profileDic[nickname] = sprite;
    }
#endregion
}