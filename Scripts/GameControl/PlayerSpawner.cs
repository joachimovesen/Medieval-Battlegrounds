using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExitGames.Client.Photon;

public class PlayerSpawner : Photon.MonoBehaviour {

    public GameObject playerPrefab;
    public Transform playerSpawns;

    static PlayerSpawner instance;

    private void Awake()
    {
        instance = this;
    } 

    public static void SpawnPlayers ()
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        instance.spawnPlayers();
    }

    void spawnPlayers ()
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        List<Vector2> spawnpoints = new List<Vector2>();
        for(int i = 0; i < playerSpawns.childCount; i++)
        {
            spawnpoints.Add(playerSpawns.GetChild(i).position);
        }

        for (int p = 0; p < PhotonNetwork.playerList.Length; p++)
        {
            Vector2[] spawnList = new Vector2[GameSettings.teamSize];
            for(int t = 0; t < GameSettings.teamSize; t++)
            {
                int r = Random.Range(0, spawnpoints.Count);
                if (spawnpoints.Count < 1)
                    Debug.LogError("ERROR: Not enough spawn points on map");
                spawnList[t] = spawnpoints[r];
                spawnpoints.RemoveAt(r);
            }
            photonView.RPC("SpawnUnits", PhotonNetwork.playerList[p], spawnList);
        }
    }

    [PunRPC]
    void SpawnUnits (Vector2[] spawnList)
    {
        foreach (Vector2 spawn in spawnList)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, spawn, Quaternion.identity, 0).GetComponent<Player>();
        }
    }

    bool CheckAllReady()
    {
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (player.CustomProperties.ContainsKey(RoomStatus.SpawnComplete.ToString()) && (bool)player.CustomProperties[RoomStatus.SpawnComplete.ToString()])
            {
                continue;
            }
            return false;
        }
        return true;
    }

    void OnPhotonPlayerPropertiesChanged(object[] playerAndProps)
    {
        PhotonPlayer player = playerAndProps[0] as PhotonPlayer;
        Hashtable props = playerAndProps[1] as Hashtable;

        if (GameSetup.gameState == GameState.WaitingForSpawn && props.ContainsKey(RoomStatus.SpawnComplete.ToString()) && (bool)props[RoomStatus.SpawnComplete.ToString()])
        {
            Debug.Log(player.NickName + " is ready!");
            if (CheckAllReady())
            {
                Debug.Log("All players are ready! Starting game...");
                GameSetup.gameState = GameState.Started;

                FindObjectOfType<GameController>().StartGame();
            }
        }
    }
}
