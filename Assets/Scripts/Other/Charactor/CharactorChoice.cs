using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class CharactorChoice : MonoBehaviour
{
    public Transform content;
    public int selectedIndex;
    public Transform explainText;
    public float textSpeed = 7f;
    public string userDataFileName = "UserData";

    private bool isNextScene = false;
    private float curTime = 0;

    private UserInput userInput;    
    private void Start()
    {
        OnClickPreviewCharactor(0);
        userInput = GetComponent<UserInput>();
        for (int i = 0; i < explainText.childCount; i++)
        {
            StartCoroutine(AlphaText(i));
        }
    }
    private void Update()
    {
        //Rotate를 하면 안내문 끄기
        if (userInput.Rotate != 0 && explainText.gameObject.activeSelf)
        {
            StopAllCoroutines();
            explainText.gameObject.SetActive(false);
        }

        transform.GetChild(selectedIndex).Rotate(transform.up, -userInput.Rotate * 2);
        //다음씬으로 넘어가자
        if (isNextScene)
        {
            curTime += Time.deltaTime;
            if (curTime > 1.2f)
                SceneManager.LoadScene(3);
        }
    }
    public IEnumerator AlphaText(int i)
    {
        Color c = explainText.GetChild(i).name.Contains("Text") ?
            explainText.GetChild(i).GetComponent<Text>().color : explainText.GetChild(i).GetComponent<Image>().color;
        bool isAlphaTrue = c.a == 1 ? true : false;
        while (true)
        {
            while (!isAlphaTrue)
            {
                if (c.a >= 0.99f)
                    isAlphaTrue = true;

                c.a = Mathf.Lerp(c.a, 1, Time.deltaTime * textSpeed);
                if (explainText.GetChild(i).name.Contains("Text"))
                    explainText.GetChild(i).GetComponent<Text>().color = c;
                else
                    explainText.GetChild(i).GetComponent<Image>().color = c;
                yield return null;
            }
            while (isAlphaTrue)
            {
                if (c.a <= 0.01f)
                    isAlphaTrue = false;

                c.a = Mathf.Lerp(c.a, 0, Time.deltaTime * textSpeed);
                if (explainText.GetChild(i).name.Contains("Text"))
                    explainText.GetChild(i).GetComponent<Text>().color = c;
                else
                    explainText.GetChild(i).GetComponent<Image>().color = c;
                yield return null;
            }
            yield return null;
        }
    }
    public void OnClickPreviewCharactor(int index)
    {
        for (int i = 0; i < content.childCount; i++)
        {
            if (i == index)
            {
                transform.GetChild(i).transform.eulerAngles = new Vector3(0, 180, 0);
                transform.GetChild(i).gameObject.SetActive(true);
                content.GetChild(i).GetComponent<Image>().color = Color.gray;
                selectedIndex = i;
            }
            else
            {
                transform.GetChild(i).gameObject.SetActive(false);
                content.GetChild(i).GetComponent<Image>().color = Color.white;
            }
        }
    }
    public void OnClickChoiceButton()
    {
        //승리 애니메이션 재생
        transform.GetChild(selectedIndex).GetComponent<Animator>().SetTrigger("Victory");
        
        //데이터 저장
        DataTemporary.MyUserData.UserAvatar = transform.GetChild(selectedIndex).name;
        string jsonData = FileManager.SaveDataFile<UserData>(userDataFileName, DataTemporary.MyUserData);

        //TODO: 데이터 수정
        //ResultTemp<Token> data = await DataModule.WebRequest<ResultTemp<Token>>(
        //    "/api/v1/users/",
        //    DataModule.NetworkType.PUT, jsonData);
        
        isNextScene = true;
        //if (data.result)
        //{
        //    Debug.Log("선택 완료");
        //}
    }
}
