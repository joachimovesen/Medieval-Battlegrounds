using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class Lobby : Photon.MonoBehaviour {

    public GameObject startButton;
    public GameObject readyButton;
    public Text statusText;

    void Start()
    {
        if (!PhotonNetwork.connected)
        {
            Utils.LoadScene(0);
            return;
        }

        print("Local is MasterClient: " + PhotonNetwork.isMasterClient);
        UpdateStatusText();

        if (PhotonNetwork.isMasterClient)
        {
            // MASTER CLIENT ONLY CODE
            startButton.SetActive(true);
            readyButton.SetActive(false);
            ToggleReady(true);
        }
        else
        {
            // NOT MASTER CLIENT
            
        }
    } 

    public void StartGame ()
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        if(PhotonNetwork.room.PlayerCount > 1 && CheckReady())
        {
            PhotonNetwork.room.IsVisible = false;
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.LoadLevel("Game00");
        }
    }

    public void LeaveLobby ()
    {
        PhotonNetwork.LeaveRoom();
        Utils.LoadScene("MainMenu");
    }

    void OnMasterClientSwitched (PhotonPlayer player)
    {
        PhotonNetwork.LeaveRoom();
        Utils.LoadScene("MainMenu");
    }

    void OnPhotonPlayerConnected (PhotonPlayer other)
    {
        UpdateStatusText();
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        UpdateStatusText();
    }

    bool CheckReady ()
    {
        foreach(PhotonPlayer player in PhotonNetwork.playerList)
        {
            if(player.CustomProperties.ContainsKey(RoomStatus.ReadyInLobby.ToString()) && (bool)player.CustomProperties[RoomStatus.ReadyInLobby.ToString()])
            {
                continue;
            }
            return false;
        }
        return true;
    }

    public void ToggleReady (bool status)
    {
        Hashtable props = new Hashtable();
        props.Add(RoomStatus.ReadyInLobby.ToString(), status);
        PhotonNetwork.player.SetCustomProperties(props);
    }

    void OnPhotonPlayerPropertiesChanged (object[] playerAndProps)
    {
        UpdateStatusText();
    }

    void UpdateStatusText ()
    {
        if (PhotonNetwork.room.PlayerCount > 1)
        {
            if (CheckReady())
                statusText.text = "<color=green>All players are ready</color>";
            else
                statusText.text = "<color=red>Not all players are ready</color>";
        }
        else
        {
            statusText.text = "<color=red>Not enough players</color>";
        }
    }
}
