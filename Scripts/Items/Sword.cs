using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Photon.MonoBehaviour {

    float rotateSpeed = 3.5f;
    float rotateAmount = 110;
    float amountRotated = 0;

    private void Start()
    {
        int parentViewID = (int)photonView.instantiationData[0];
        if(PhotonView.Find(parentViewID) != null)
            transform.SetParent(PhotonView.Find(parentViewID).GetComponent<PlayerCombatController>().rHand);
        else if(photonView.isMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void Update()
    {
        if(photonView.isMine && amountRotated >= rotateAmount)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        float rotateBy = rotateAmount * rotateSpeed * Time.deltaTime;
        transform.Rotate(0, 0, -rotateBy);
        amountRotated += rotateBy;
    }

}
