using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTimer : Photon.MonoBehaviour {

    public static float getTime { get { return instance != null ? instance.timeStarted + instance.duration - (float)PhotonNetwork.time: 0; } }

    static TurnTimer instance;

    float timeStarted;
    bool timerStarted = false;
    float duration;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if(GameSettings.timedMode && timerStarted)
        {
            int timeLeft = (int)Mathf.Clamp(getTime,0,duration);
            GeneralUI.SetTimeText(timeLeft.ToString());
            if(timeLeft <= 0)
            {
                stopTimer();
            }
        }
    } 

    void stopTimer ()
    {
        timerStarted = false;
        duration = 0;
    }

    [PunRPC]
	void StartTimer (float timeStarted, float duration)
    {
        this.timeStarted = timeStarted;
        this.duration = duration;
        timerStarted = true;
    }

    public static void StartNewTimer (float duration)
    {
        if (!GameSettings.timedMode)
            return;
        instance.photonView.RPC("StartTimer", PhotonTargets.All, (float)PhotonNetwork.time, duration);
    }

    public static void StartNewLocalTimer (float duration)
    {
        if (!GameSettings.timedMode)
            return;
        instance.StartTimer((float)PhotonNetwork.time, duration);
    }

    public static void StopTimer ()
    {
        instance.stopTimer();
    }

}
