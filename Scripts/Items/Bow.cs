using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour {

    Animator anim;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        Invoke("TriggerCharge", 2f);
        Invoke("TriggerFire", 5f);
    }

    void TriggerCharge ()
    {
        anim.SetTrigger("charge");
    }

    void TriggerFire ()
    {
        anim.SetTrigger("fire");
    }

}
