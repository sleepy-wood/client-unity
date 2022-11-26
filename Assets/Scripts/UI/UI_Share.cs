using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Share : MonoBehaviourPunCallbacks
{
    public void OnClickEnterSharedLand()
    {
        PhotonNetwork.LoadLevel("SharedLand");
    }

}
