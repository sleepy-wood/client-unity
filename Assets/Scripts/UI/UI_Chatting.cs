using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using static Autodesk.Fbx.FbxTime;

public class UI_Chatting : MonoBehaviourPun
{

    [SerializeField] private float chatMoveDistance = 836f;
    public Transform windowContent;
    public GameObject menuBar;

    //InputChat
    private InputField chatting;
    //ChatItem 공장
    private GameObject chatPrefab;
    //ScrollView의 Content
    private RectTransform content;
    //EmojiChatting의 Content
    private RectTransform chat_content;
    //이전 Content의 H (멘토님 설명 중 H2역할)
    private float prevContentH;
    //ScrollView의 H (멘토님 설명 중 H1역할)
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
        content = transform.GetChild(0).GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>();

        DirectoryInfo di = new DirectoryInfo(path);
        string[] fileEntries = Directory.GetFiles(path, "*.png");

        int i = 15;
        //썸네일 넣기
        foreach (string fileName in fileEntries)
        {
            //GameObject resource = fileName.Split("/TextureImg/")[1].Split('.')[0];
            GameObject resource = Resources.Load<GameObject>("Emoji_");
            GameObject prefab = Instantiate(resource);
            prefab.name = "Custom_" + fileName.Split("/TextureImg/")[1].Split('.')[0];
            byte[] byteTexture = File.ReadAllBytes(path + fileName.Split("/TextureImg/")[1].Split('.')[0] + ".png");
            if (byteTexture.Length > 0)
            {
                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(byteTexture);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                prefab.GetComponent<Image>().sprite = sprite;
            }
            int temp = i;
            prefab.GetComponent<Button>().onClick.AddListener(
                () => OnClickEmojiButton(temp, DataTemporary.image_Url[i - 15]));
            i++;
        }


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
    public void OnClickEmojiButton(int i, string img_url = null)
    {
        photonView.RPC("RPC_EmojiButton", RpcTarget.All, i, DataTemporary.MyUserData.nickname, img_url);
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
    public void RpcAddChat(string rpcChat, string nickname)
    {
        //0.바뀌기 전의 Content H값을 넣자
        prevContentH = content.sizeDelta.y;

        //Chat 추가한다!
        //1. ChatItem을 만든다(부모를 ScrollView의 Content)
        GameObject chat = Instantiate(chatPrefab, content);
        //2. 만든 ChatItem에서 ChatItem 컴포넌트를 가져온다.
        //3. 가져온 컴포넌트에 s를 셋팅
        chat.transform.GetChild(0).GetComponent<ChatItem>().SetText(rpcChat);
        chat.transform.GetChild(1).GetComponent<Image>().sprite = profileDic[nickname];
        StartCoroutine(AutoScrollBotton());
        if (!isActiveChat)
        {
            transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
        }
    }

    [PunRPC]
    public async Task RPC_EmojiButtonAsync(int i, string nickname, string url = null)
    {
        GameObject emojiResource = Resources.Load<GameObject>("Emoji");
        GameObject emojiPrefab = Instantiate(emojiResource);

        emojiPrefab.transform.parent = windowContent.transform;

        if (i >= 15)
        {
            Texture2D texture = await DataModule.WebrequestTexture(url, DataModule.NetworkType.GET);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            emojiPrefab.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
        }
        else
        {
            Sprite emojiImgResource = Resources.Load<Sprite>("Emoji_image/Emoji_" + i);
            Sprite emoji = Instantiate(emojiImgResource);

            emojiPrefab.transform.GetChild(0).GetComponent<Image>().sprite = emoji;
        }
        emojiPrefab.GetComponent<Image>().sprite = profileDic[nickname];
    }
    [PunRPC]
    public async void RPC_ProfileList(string url, string nickname)
    {
        Texture2D texture = await DataModule.WebrequestTexture(url, DataModule.NetworkType.GET);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 10);
        profileDic[nickname] = sprite;
    }
#endregion
}
