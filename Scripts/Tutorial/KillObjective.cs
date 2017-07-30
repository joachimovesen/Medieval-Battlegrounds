using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillObjective : Objective {

    public List<GameObject> targets;
    int targetCount;
    int targetsKilled = 0;

	public override void Activate ()
    {
        started = true;
        foreach(GameObject target in targets)
        {
            target.SetActive(true);
        }
    }

    private void Start()
    {
        targetCount = targets.Count;
    }

    private void Update()
    {
        if (!started || complete)
            return;

        if(targets.Count > 0 && targets[0] == null)
        {
            targets.RemoveAt(0);
            targetsKilled++;
        }

        if(targetsKilled == targetCount)
        {
            OnComplete();
        }
    }

}
