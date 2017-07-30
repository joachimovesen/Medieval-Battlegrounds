using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public class HostLobby : MonoBehaviour {

    void Awake ()
    {
		if(!PhotonNetwork.connected)
        {
            Utils.LoadScene(0);
            return;
        }
    }

    public void StartLobby ()
    {
        Hashtable props = new Hashtable();
        foreach (RoomStatus status in System.Enum.GetValues(typeof(RoomStatus)))
        {
            props.Add(status.ToString(), false);
        }
        PhotonNetwork.player.SetCustomProperties(props);

        bool success = PhotonNetwork.CreateRoom(PhotonNetwork.player.NickName + "'s lobby", new RoomOptions { MaxPlayers = 4, IsVisible = true, IsOpen = true }, TypedLobby.Default);

        if (!success)
        {
            print("Failed to create room");
            Utils.LoadScene(0);
            return;
        }
    }

    void OnJoinedRoom ()
    {
        print("Created room!");
        PhotonNetwork.LoadLevel("Lobby");
    }
}
