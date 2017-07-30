using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSync : Photon.MonoBehaviour {

    PlayerCombatController combatController;

    void Awake()
    {
        combatController = GetComponent<PlayerCombatController>();
    }

	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.isWriting)
        {
            stream.SendNext(transform.localScale); // Send localScale for synced facing direction
            stream.SendNext(combatController.weapon.transform.rotation); // Sync weapon rotation
        }
        else if(stream.isReading)
        {
            Vector3 localScale = (Vector3)stream.ReceiveNext();
            transform.localScale = localScale;

            Quaternion weaponRotation = (Quaternion)stream.ReceiveNext();
            combatController.weapon.rotation = weaponRotation;
        }
    }
}
