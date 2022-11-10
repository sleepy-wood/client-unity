using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class StartManager : MonoBehaviourPunCallbacks
{
    public InputField friendCode_InputField;
    //public InputField createCode_InputField;
    //public InputField nickName_InputField;

    /// <summary>
    /// 친구 방 입장 창 띄우기
    /// </summary>
    public void OnClickActive()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    /// <summary>
    /// 친구 방 입장 창 끄기
    /// </summary>
    public void OnClickNotActive()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
    /// <summary>
    /// 친구방 입장 -> 먼저 서버 Disconnect를 한다.
    /// </summary>
    public void OnClickVisit()
    {
        //if (nickName_InputField.text != "")
        //{
        //if (friendCode_InputField?.text == "" && createCode_InputField?.text == "")
        //{
        //    //나중에 UI로 구현
        //    Debug.Log("코드를 입력해주세요.");
        //}
        //else if (friendCode_InputField?.text != "" && createCode_InputField?.text != "")
        //{
        //    //나중에 UI로 구현
        //    Debug.Log("둘 중 하나의 코드만 입력해주세요");
        //}
        //else 

        if (friendCode_InputField?.text != "")
        {
            PhotonNetwork.Disconnect();
        }
        //else if (createCode_InputField?.text != "")
        //{
        //    Debug.Log("가~보자고~~");
        //    CreateRoom();
        //}
        //}
    }
    /// <summary>
    /// Disconnect할 시 다시 Connect를 하기
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("내가 갈게 친구야~~");
        OnConnect();
    }
    public void OnConnect()
    {
        //마스터 서버에 접속 요청
        PhotonNetwork.ConnectUsingSettings();
    }
    //마스터 서버에 접속 성공, 로비 생성 및 진입을 할 수 없는 상태
    public override void OnConnected()
    {
        base.OnConnected();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    //마스터 서버에 접속, 로비 생성 및 진입이 가능한 상태
    //이때 로비에 진입해야함
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        //닉네임 설정
        PhotonNetwork.NickName = DataTemporary.MyUserData.nickName;

        //Connect 후 친구방 들어가기
        JoinRoom(friendCode_InputField.text);

    }
    private void JoinRoom(string text)
    {
        PhotonNetwork.JoinRoom(text);
    }
    //방 입장이 성공했을 때 불리는 함수
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("OnJoinedRoom");
        PhotonNetwork.LoadLevel("MyWorld");
    }
    //방 입장 실패시 호출되는 함수
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        //방 참여 실패 UI로 알려주기
    }
}