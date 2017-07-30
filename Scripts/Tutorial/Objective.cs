using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Objective : MonoBehaviour
{
    public bool isFinished { get { return complete; } }

    protected bool started = false;
    protected bool complete = false;
    public UnityAction onCompleteEvents;

    public virtual void Activate() { }

    protected void OnComplete ()
    {
        if (complete)
            return;
        complete = true;

        if(onCompleteEvents != null)
        {
            onCompleteEvents();
        }
    }
}
