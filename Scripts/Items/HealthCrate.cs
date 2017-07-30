using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCrate : Photon.MonoBehaviour , ITurnEvent {

    public int health = 20;

    bool active = false;

    public bool isFinished { get { return active; } }

    private void Start()
    {
        GameController.turnState = TurnState.WaitingForTurnEvents;
        TurnEvent turnEvent = new TurnEvent(this);
        GameController.eventQueue.Enqueue(turnEvent);

        if(!PhotonNetwork.isMasterClient)
            GetComponent<Rigidbody2D>().isKinematic = true;

        CameraLerpTransform.SetLocalCameraTarget(transform);
        GeneralUI.SetTurnText("Crate appeared!");

        Invoke("Activate", 2f);
    } 

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        if(other.transform.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerHealth>().TakeDamage(-1, -health);
            active = false;
            PhotonNetwork.Instantiate("Pickup_Sound", transform.position, Quaternion.identity, 0);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void OnMasterClientSwitched (PhotonPlayer player)
    {
        if(PhotonNetwork.isMasterClient)
        {
            GetComponent<Rigidbody2D>().isKinematic = false;
        }
    }

    void Activate ()
    {
        active = true;
    }

}
