using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChatItem : MonoBehaviour
{
    //Text 컴포넌트
    XRText chatText;
    //RectTransform 컴포넌트
    RectTransform rt;

    float preferredH;

    private void Awake()
    {
        chatText = GetComponent<XRText>();
        rt = GetComponent<RectTransform>();
        chatText.onChangedSize = OnChangedTextSize;
    }
    private void Update()
    {
        //if(preferredH != chatText.preferredHeight)
        //{
        //    //chatText.text의 크기에 맞게 ContentSize를 변경
        //    //rt.sizeDelta.y는 Height
        //    //rt.sizeDelta.x는 width
        //    //chatText.preferredHeight는 최적화된 크기중 Height를 내놔
        //    rt.sizeDelta = new Vector2(rt.sizeDelta.x, chatText.preferredHeight);
        //    preferredH = chatText.preferredHeight;
        //}
    }
    //Text 셋팅, Text 내용의 크기에 맞게 자신의 ContentSize를 변경하는 함수
    public void SetText(string s)
    {
        chatText.text = s;
    }
    void OnChangedTextSize()
    {
        if (preferredH != chatText.preferredHeight)
        {
            print("크기 변경 완료!!");
            //chatText.text의 크기에 맞게 ContentSize를 변경
            //rt.sizeDelta.y는 Height
            //rt.sizeDelta.x는 width
            //chatText.preferredHeight는 최적화된 크기중 Height를 내놔
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, chatText.preferredHeight);
            preferredH = chatText.preferredHeight;
        }
    }
}
