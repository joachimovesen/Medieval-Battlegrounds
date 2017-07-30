using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralUI : MonoBehaviour {

    static GeneralUI instance;

    public Text teamText;
    public Text turnText;
    public GameObject winPanel;
    public Text winText;
    public Text timeText;

    void Awake()
    {
        instance = this;
    } 

    public static void SetTeamText ()
    {
        string color = TeamManager.getLocalTeam().color;

        instance.teamText.text = "You are <color=" + color.ToLower() + ">" + color.ToUpper() + "</color>";
    }

    public static void SetTurnText(int playerID)
    {
        if (instance.turnText == null)
            return;

        bool myTurn = playerID == PhotonNetwork.player.ID;
        string color = TeamManager.getTeamByID(playerID).color;

        instance.turnText.text = myTurn ? "Your turn!" : "<color=" + color.ToLower() + ">" + color.ToUpper() + "</color> turn";
    }

    public static void SetTurnText(string text)
    {
        if (instance.turnText == null)
            return;

        instance.turnText.text = text;
    }

    public static void SetWinText(string text)
    {
        instance.winText.text = text;
    }

    public static void ShowWinText ()
    {
        instance.winPanel.gameObject.SetActive(true);
    }

    public static void SetTimeText (string text)
    {
        instance.timeText.text = text;
    }

}
