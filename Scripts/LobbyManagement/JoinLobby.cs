using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinLobby : MonoBehaviour {

	public void JoinRandom ()
    {
        PhotonNetwork.JoinRandomRoom();
    }

}
