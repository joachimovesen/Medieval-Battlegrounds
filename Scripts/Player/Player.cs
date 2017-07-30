using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Player : Photon.MonoBehaviour {

    public bool playerControlled = true;
    public bool isActiveTurn { get{ return playerControlled ? active : false; } }
    bool active = false;

    PlayerMovement movementController;
    PlayerCombatController combatController;

    [HideInInspector]
    public GrapplingHook attatchedHook;

    void Awake()
    {
        movementController = GetComponent<PlayerMovement>();
        combatController = GetComponent<PlayerCombatController>();

        if(playerControlled)
        {
            Sprite newSprite = Resources.Load<Sprite>("Sprites/Player_" + TeamManager.getTeamByID(photonView.ownerId).color);
            if (newSprite != null)
                GetComponentInChildren<SpriteRenderer>().sprite = newSprite;

            TeamManager.getTeamByID(photonView.ownerId).AddUnit(this);

            if(photonView.isMine && TeamManager.getLocalTeam().getUnits.Count >= GameSettings.teamSize)
            {
                // Set status to ready if this was the last unit to spawn
                Hashtable props = new Hashtable();
                props.Add(RoomStatus.SpawnComplete.ToString(), true);
                PhotonNetwork.player.SetCustomProperties(props);
            }
        }
    }

    void Update()
    {
        if (!active)
            return;

        if (GameSettings.timedMode && TurnTimer.getTime <= 0)
            EndTurn();
    }

    public void Activate ()
    {
        active = true;
        movementController.Activate();
        combatController.Activate();

        if(GameSettings.usePointingArrow)
        {
            PointingArrow.SetColor(TeamManager.getTeamByID(photonView.ownerId).color);
            PointingArrow.SetTarget(transform);
        }
        CameraLerpTransform.SetCameraTarget(photonView.viewID);
        TurnTimer.StartNewTimer(GameSettings.turnDuration);
    }

    public void Deactivate ()
    {
        if (GameSettings.usePointingArrow)
            PointingArrow.Hide();
        active = false;
        movementController.EndTurn();
        combatController.EndTurn();
        if (GadgetUI.isVisible)
            GadgetUI.TogglePanel();
    }

    public void EndTurn ()
    {
        if (!isActiveTurn)
            return;

        Deactivate();

        GameController.EndTurn();
    }
}
