using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class StartManager : MonoBehaviourPunCallbacks
{
    public InputField friendCode_InputField;
    public InputField createCode_InputField;
    public InputField nickName_InputField;
    private void Start()
    {
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
        //print("OnConnected");
    }
    //마스터 서버에 접속, 로비 생성 및 진입이 가능한 상태
    //이때 로비에 진입해야함
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        //print("OnConnectedToMaster");

        //닉네임 설정
        PhotonNetwork.NickName = nickName_InputField.text;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (nickName_InputField.text != "")
            {
                if (friendCode_InputField?.text == "" && createCode_InputField?.text == "")
                {
                    //나중에 UI로 구현
                    Debug.Log("코드를 입력해주세요.");
                }
                else if (friendCode_InputField?.text != "" && createCode_InputField?.text != "")
                {
                    //나중에 UI로 구현
                    Debug.Log("둘 중 하나의 코드만 입력해주세요");
                }
                else if (friendCode_InputField?.text != "")
                {
                    Debug.Log("내가 갈게 친구야~~");
                    JoinRoom();
                }
                else if (createCode_InputField?.text != "")
                {
                    Debug.Log("가~보자고~~");
                    CreateRoom();
                }
            }
        }
    }
    private void JoinRoom()
    {
        PhotonNetwork.JoinRoom(friendCode_InputField.text);
    }
    //방 입장이 성공했을 때 불리는 함수
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("OnJoinedRoom");
        PhotonNetwork.LoadLevel("SkyLand");
    }
    private void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();

        roomOptions.MaxPlayers = 3;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom(createCode_InputField.text, roomOptions);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        //방생성 실패 UI로 알려주기
    }
    //방 입장 실패시 호출되는 함수
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        //방 참여 실패 UI로 알려주기
    }


}