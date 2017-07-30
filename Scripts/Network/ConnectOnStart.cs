using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectOnStart : Photon.MonoBehaviour {

    private void Start()
    {        
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.offlineMode = false; // Scene reloads again if offlinemode is true because state is not disconnected
            OnJoinedLobby();
            return;
        }

        PhotonNetwork.ConnectUsingSettings(GameSettings.gameVersion);
    }

    public void OnJoinedLobby ()
    {
        print("Joined lobby!");
        PhotonNetwork.automaticallySyncScene = true;
        Utils.LoadScene(1);
    }
}
