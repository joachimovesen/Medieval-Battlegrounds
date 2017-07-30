using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {

    List<ITurnCallbacks> listeners = new List<ITurnCallbacks>();

    public static int turnIndex; // the index for the current turn
    int turnsPassed; // Tracking total amount of turns for the current session

    public const byte turnManagerEventOffset = 0;
    public const byte turnBeginEvent = 1 + turnManagerEventOffset;
    public const byte turnEndEvent = 2 + turnManagerEventOffset;
    public const byte timerFinishedEvent = 3 + turnManagerEventOffset;

    private void Awake()
    {
        PhotonNetwork.OnEventCall += OnEvent;
    }

    public void OnEvent(byte eventCode, object content, int senderId)
    {
        switch (eventCode)
        {
            case turnBeginEvent:
                turnIndex = (int)content;
                listeners.ForEach(x => x.OnTurnBegin(turnIndex));
                break;
            case turnEndEvent:
                turnsPassed++;
                listeners.ForEach(x => x.OnTurnEnd(turnIndex));
                break;
            case timerFinishedEvent:
                listeners.ForEach(x => x.OnTurnTimerFinished(turnIndex));
                break;
        }
    }

    public void AddListener (ITurnCallbacks callbacks)
    {
        if (listeners == null)
            listeners = new List<ITurnCallbacks>();

        if (!listeners.Contains(callbacks))
            listeners.Add(callbacks);
    }

    private void OnDestroy()
    {
        listeners = new List<ITurnCallbacks>();
    }
}

public interface ITurnCallbacks
{
    void OnTurnBegin(int index);
    void OnTurnEnd(int index);
    void OnTurnTimerFinished(int index);
}
