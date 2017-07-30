using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTutorial : Photon.MonoBehaviour {

	public void LoadTutorialScene ()
    {
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.Disconnect();
            PhotonNetwork.offlineMode = true;
        }
        Utils.LoadScene("TutorialLevel");
    }
}
