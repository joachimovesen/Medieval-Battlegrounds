using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour {

    public string tutorialName = "Tutorial";

    int stage = 0;
    public Stage[] stages;
    Stage currentStage { get { return stages[stage]; } }

    string waitingForItem = "";

    public void StartTutorial()
    {
        GadgetUI.onWeaponChangeEvent += OnGadgetChange;
        TutorialUI.onContinue = OnContinuePressed;
        ShowStage();
    }

    void OnContinuePressed()
    {
        stage++;
        ShowStage();
    }

    void ShowStage()
    {
        if (stage < stages.Length)
        {
            TutorialUI.SetTutorialText(currentStage.message);
            TutorialUI.ToggleContinueButton(!currentStage.hideContinueButton);
            TutorialUI.Show();
            currentStage.action.Invoke();
        }
    }

    public void AddContinueCallback (Objective objective)
    {
        objective.onCompleteEvents += OnContinuePressed;
    }

    public void WaitForItemSelect (string item)
    {
        Gadgets.canUseArrow = item == GadgetType.Arrow.ToString() ? true : false;
        Gadgets.canUseSword = item == GadgetType.Sword.ToString() ? true : false;
        Gadgets.canUseGrapple = item == GadgetType.Grapple.ToString() ? true : false;
        waitingForItem = item;
    }

    public void TeleportPlayer ()
    {
        TeamManager.getLocalTeam().getUnits[0].transform.position = transform.position;
        CameraLerpTransform.SetLocalCameraTarget(transform);
    }

    public void ResetGame ()
    {
        TeamManager.getLocalTeam().getUnits[0].Deactivate();
        GameController.gameStarted = false;
    }

    public void ExitTutorial ()
    {
        TutorialUI.Hide();
        PhotonNetwork.LeaveRoom();
        Utils.LoadScene(0);
    }

    public void EnableAllItems ()
    {
        Gadgets.canUseArrow = true;
        Gadgets.canUseSword = true;
        Gadgets.canUseGrapple = true;
    }

    void OnGadgetChange(GadgetType type)
    {
        if(waitingForItem == type.ToString())
        {
            waitingForItem = "";
            Gadgets.canUseArrow = type == GadgetType.Arrow ? true : false;
            Gadgets.canUseSword = type == GadgetType.Sword ? true : false;
            Gadgets.canUseGrapple = type == GadgetType.Grapple ? true : false;
            OnContinuePressed();
        }
    }
}

[System.Serializable]
public struct Stage
{
    [TextArea]
    public string message;
    public bool hideContinueButton;
    public UnityEvent action;
}

public enum StageType
{
    Info,
    Objective
}
