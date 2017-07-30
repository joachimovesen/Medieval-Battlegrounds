using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnEvent {

    public bool isComplete
    {
        get
        {
            return eventObject.Equals(null) ? true : eventObject.isFinished;
        }
    }

    ITurnEvent eventObject;

    public TurnEvent(ITurnEvent eventObject)
    {
        this.eventObject = eventObject;
    }

}
