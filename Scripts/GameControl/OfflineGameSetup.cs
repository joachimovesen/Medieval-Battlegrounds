using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public class OfflineGameSetup : MonoBehaviour {

    [Header("Game Settings")]
    public bool timedMode = true;
    public float turnDuration = 999f;
    public bool crateSpawnEnabled = false;

    [Header("Gadget Settings")]
    public bool allowArrow = true;
    public bool allowSword = true;
    public bool allowGrapple = true;
    public bool infiniteGrapple = false;

    [Header("Other Settings")]
    public bool autoStartGame = true;
    public bool usePointingArrow = false;
    public string playerColor = "Red";

    private void Awake()
    {
        PhotonNetwork.offlineMode = true;
        PhotonNetwork.playerName = PhotonNetwork.playerName;
        GameSettings.timedMode = timedMode;
        GameSettings.turnDuration = turnDuration;
        GameSettings.crateSpawnEnabled = crateSpawnEnabled;
        GameSettings.usePointingArrow = usePointingArrow;

        Gadgets.canUseArrow = allowArrow;
        Gadgets.canUseSword = allowSword;
        Gadgets.canUseGrapple = allowGrapple;
        Gadgets.infiniteGrapple = infiniteGrapple;
    }

    private void Start()
    {
        PhotonNetwork.CreateRoom("OfflineRoom");
    }

    void OnJoinedRoom()
    {
        TeamManager.Initialize();
        TeamManager.AddTeam(new Team(PhotonNetwork.player,playerColor));

        GeneralUI.SetTeamText();

        PhotonNetwork.Instantiate("Player", transform.position, Quaternion.identity, 0);

        if (autoStartGame)
            FindObjectOfType<GameController>().StartGame();
    }

}
