using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameController : Photon.MonoBehaviour , ITurnCallbacks {

    public static bool gameStarted = false;
    public static Team currentTeam { get { return TeamManager.getTeams[TurnManager.turnIndex]; } }

    static int unitIndex;

    static bool gameFinished = false;

    public static Queue<TurnEvent> eventQueue;

    public static TurnState turnState = TurnState.Standard;

    bool wasQueueEmpty = true; // Was the queue empty last frame?
    float queueEmptiedTime; // The time the queue was emptied

    void Awake ()
    {
        FindObjectOfType<TurnManager>().AddListener(this);
        TurnManager.turnIndex = 0;
        unitIndex = 0;
        eventQueue = new Queue<TurnEvent>();
        gameStarted = false;
        gameFinished = false;
	}

    private void Update()
    {
        if (!gameStarted)
            return;

        if(turnState == TurnState.WaitingForTurnEvents)
        {
            if(eventQueue.Count > 0)
            {
                wasQueueEmpty = false;
                StopAllCoroutines();
                print("waiting for queue event isComplete: " + eventQueue.Peek().isComplete);
                if (eventQueue.Peek().isComplete)
                {
                    print("complete. removing from queue");
                    eventQueue.Dequeue();
                }
            }
            else
            {
                if(!wasQueueEmpty)
                {
                    wasQueueEmpty = true;
                    queueEmptiedTime = Time.time;
                }

                if(queueEmptiedTime + 3f <= Time.time) // The queue has been empty for 3 seconds
                {
                    turnState = TurnState.WaitingNewTurn;
                    if (PhotonNetwork.isMasterClient)
                    {
                        if (GameSettings.crateSpawnEnabled && Random.Range(0, 100) < GameSettings.crateSpawnChance)
                        {
                            GameController.turnState = TurnState.WaitingForTurnEvents;
                            FindObjectOfType<CrateSpawner>().SpawnCrate();
                        }
                        else
                            StartNextTurn();
                    }
                }
            }
        }
    }

    public void StartGame()
    {
        if (gameStarted)
            return;

        GameSetup.gameState = GameState.Started;
        gameStarted = true;

        if (PhotonNetwork.isMasterClient)
        {
            int turnIndex = Random.Range(0, TeamManager.getTeams.Count);
            PhotonNetwork.RaiseEvent(TurnManager.turnBeginEvent, turnIndex, true, new RaiseEventOptions { Receivers = ReceiverGroup.All });
        }
    }

    [PunRPC]
    void CheckWinConditionsRPC ()
    {
        if (gameFinished)
            return;

        if(PhotonNetwork.offlineMode)
        {
            if(TeamManager.aliveTeams.Count < 1)
            {
                EndGame();
            }
        }
        else
        {
            if (TeamManager.aliveTeams.Count < 2)
            {
                EndGame();
            }
        }
    }

    public static void EndTurn ()
    {
        if (gameFinished || !currentTeam.player.IsLocal)
            return;

        print("Ending turn");
        PhotonNetwork.RaiseEvent(TurnManager.turnEndEvent, null,true, new RaiseEventOptions { Receivers = ReceiverGroup.All });
    }

    int GetNextTurnIndex ()
    {
        int newIndex = TeamManager.FindNextAliveIndex(TurnManager.turnIndex);
        return newIndex;
    }

    static void EndGame ()
    {
        if (gameFinished)
            return;
        // END THE GAME HERE
        gameFinished = true;
        TurnTimer.StopTimer();
        if(TeamManager.aliveTeams.Count > 0)
        {
            Team winningTeam = TeamManager.aliveTeams[0];

            string color = winningTeam.color;

            string winText = "<color=" + color.ToLower() + ">" + winningTeam.player.NickName + "</color> WON!";
            GeneralUI.SetWinText(winText);
            GeneralUI.ShowWinText();
            print(winningTeam.player.NickName + " has won the game!");
        }
        else
        {
            GeneralUI.ShowWinText();
            print("GAME OVER - No winner!");
        }
    }

    void OnPhotonPlayerDisconnected (PhotonPlayer other)
    {
        print(other.NickName + " has disconnected!");

        if (gameFinished)
            return;

        TeamManager.getTeamByID(other.ID).ClearUnits();
        CheckWinConditionsRPC();

        if (currentTeam.player.ID == other.ID)
        {
            if(PhotonNetwork.isMasterClient)
            {
                if (turnState == TurnState.TurnInProgress)
                {
                    EndTurn();
                }
                else if (turnState == TurnState.WaitingNewTurn)
                {
                    StartNextTurn();
                }
            }
            else
                turnState = TurnState.WaitingForMaster;
        }
    }

    void OnMasterClientSwitched (PhotonPlayer newMasterClient)
    {
        if(PhotonNetwork.player == newMasterClient)
        {
            if(turnState == TurnState.WaitingForMaster)
            {
                StartNextTurn();
            }
        }
    }

    public void LeaveGame ()
    {
        PhotonNetwork.LeaveRoom();
        Utils.LoadScene(0);
    }

    void StartNextTurn ()
    {
        if (gameFinished)
            return;
        if (PhotonNetwork.isMasterClient)
            photonView.RPC("CheckWinConditionsRPC", PhotonTargets.All);

        int index = GetNextTurnIndex();
        PhotonNetwork.RaiseEvent(TurnManager.turnBeginEvent, index, true, new RaiseEventOptions { Receivers = ReceiverGroup.All });
    }

    public void OnTurnBegin(int index)
    {
        if (gameFinished)
            return;
        print("OnTurnBegin");
        CheckWinConditionsRPC();
        eventQueue = new Queue<TurnEvent>();

        turnState = TurnState.TurnInProgress;
        TurnManager.turnIndex = index;

        GeneralUI.SetTurnText(currentTeam.player.ID);
        if (currentTeam.player.IsLocal)
        {
            if (unitIndex >= TeamManager.getLocalTeam().getUnits.Count)
                unitIndex = 0;

            TeamManager.getLocalTeam().getUnits[unitIndex].Activate();
            unitIndex++;
        }
    }

    public void OnTurnEnd(int index)
    {
        print("OnTurnEnd");
        queueEmptiedTime = Time.time;
        turnState = TurnState.WaitingForTurnEvents;
        TurnTimer.StopTimer();
        GeneralUI.SetTurnText("Finalizing round");
        GeneralUI.SetTimeText("--");
    }

    public void OnTurnTimerFinished(int index)
    {
        print("OnTurnTimerFinished");

        switch(turnState)
        {
            case TurnState.TurnInProgress:
                // The turn should end here
                break;
            case TurnState.WaitingNewTurn:
                // Wait for all turn events to finish here, then start the next turn
                break;
        }
    }
}

public enum TurnState
{
    Standard,
    TurnInProgress,
    WaitingForTurnEvents,
    WaitingNewTurn,
    WaitingForMaster
}
