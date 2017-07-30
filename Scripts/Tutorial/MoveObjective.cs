using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjective : Objective {

    public override void Activate()
    {
        started = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(started && other.tag == "Player")
        {
            OnComplete();
        }
    }

}
