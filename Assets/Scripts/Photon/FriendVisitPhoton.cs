using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class FriendVisitPhoton : MonoBehaviourPunCallbacks
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
    /// 친구방 입장 -> 먼저 LeaveRoom를 한다.
    /// </summary>
    public void OnClickVisit()
    {
        if (friendCode_InputField?.text != "")
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    //public void OnConnect()
    //{
    //    //마스터 서버에 접속 요청
    //    PhotonNetwork.ConnectUsingSettings();
    //}
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
        PhotonNetwork.NickName = DataTemporary.MyUserData.nickname;

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
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