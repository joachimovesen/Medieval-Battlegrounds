using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using System;

public class GameSetup : Photon.MonoBehaviour {

    public static GameState gameState = GameState.None;

    void ApplySettings ()
    {
        GameSettings.timedMode = true;
        GameSettings.crateSpawnEnabled = true;
        GameSettings.turnDuration = 30f;
        Gadgets.infiniteGrapple = false;
    }

    void Awake()
    {
        if (!PhotonNetwork.connected)
        {
            Utils.LoadScene(0);
            return;
        }

        ApplySettings();

        gameState = GameState.None;
    }

    void Start()
    {
        gameState = GameState.AssigningTeams;

        //if (!PhotonNetwork.isMasterClient)
        //    return;

        AssignTeams();
    }

    void AssignTeams()
    {
        // Create a list of all player ids
        List<int> ids = new List<int>();
        Array.ForEach<PhotonPlayer>(PhotonNetwork.playerList, x => ids.Add(x.ID));
        // Sort the list to make sure it matches on every client
        ids.Sort( (x, y) => x.CompareTo(y) );

        // Create and populate the team list
        TeamManager.Initialize();
        int i = 0;
        ids.ForEach( x => TeamManager.AddTeam(new Team(PhotonPlayer.Find(x), GameSettings.teamColors[i++])) );

        // Update team text UI
        GeneralUI.SetTeamText();

        // Mark local player as ready to proceed
        Hashtable props = new Hashtable();
        props.Add(RoomStatus.ReadyToSpawn.ToString(), true);
        PhotonNetwork.player.SetCustomProperties(props);
    }

    bool CheckAllReady()
    {
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (player.CustomProperties.ContainsKey(RoomStatus.ReadyToSpawn.ToString()) && (bool)player.CustomProperties[RoomStatus.ReadyToSpawn.ToString()])
            {
                continue;
            }
            return false;
        }
        return true;
    }

    void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
        Hashtable props = playerAndUpdatedProps[1] as Hashtable;

        if (gameState == GameState.AssigningTeams && props.ContainsKey(RoomStatus.ReadyToSpawn.ToString()) && (bool)props[RoomStatus.ReadyToSpawn.ToString()])
        {
            print(player.NickName + " is ready!");
            if (CheckAllReady())
            {
                print("All players are ready. Spawning phase...");
                gameState = GameState.WaitingForSpawn;

                if(PhotonNetwork.isMasterClient)
                    PlayerSpawner.SpawnPlayers();
            }
        }
    }

    void OnMasterClientSwitched (PhotonPlayer newMasterClient)
    {
        if(GameSetup.gameState != GameState.Started)
        {
            Debug.LogWarning("Master client left the game too early, returning to menu :(");
            PhotonNetwork.LeaveRoom();
            Utils.LoadScene("MainMenu");
        }
    }
}

public enum GameState
{
    None,
    Started,
    WaitingForSpawn,
    AssigningTeams
}

public enum RoomStatus
{
    ReadyInLobby,
    ReadyToSpawn,
    SpawnComplete
}
