using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterServerPhoton : MonoBehaviourPunCallbacks
{
    //private void Start()
    //{
    //    OnConnect();
    //}
    //public void OnConnect()
    //{
    //    //마스터 서버에 접속 요청
    //    PhotonNetwork.ConnectUsingSettings();
    //}
    ////마스터 서버에 접속 성공, 로비 생성 및 진입을 할 수 없는 상태
    //public override void OnConnected()
    //{
    //    base.OnConnected();
    //    print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    //    //print("OnConnected");
    //}
    ////마스터 서버에 접속, 로비 생성 및 진입이 가능한 상태
    ////이때 로비에 진입해야함
    //public override void OnConnectedToMaster()
    //{
    //    base.OnConnectedToMaster();
    //    print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    //    //print("OnConnectedToMaster");

    //    //닉네임 설정
    //    PhotonNetwork.NickName = "1";

    //    CreateRoom();
    //}
    //private void CreateRoom()
    //{
    //    RoomOptions roomOptions = new RoomOptions();

    //    roomOptions.MaxPlayers = 3;
    //    roomOptions.IsVisible = true;
    //    PhotonNetwork.CreateRoom(PhotonNetwork.NickName, roomOptions);
    //    PhotonNetwork.LoadLevel(1);
    //}
    //public override void OnCreateRoomFailed(short returnCode, string message)
    //{
    //    base.OnCreateRoomFailed(returnCode, message);
    //    //방생성 실패 UI로 알려주기
    //}
}
