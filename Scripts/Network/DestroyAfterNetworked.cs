using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterNetworked : Photon.MonoBehaviour {

    public double seconds = 3;
    double spawnTime;

    private void Start()
    {
        spawnTime = PhotonNetwork.time;
    }

    private void Update()
    {
        if (!photonView.isMine)
            return;

        if (PhotonNetwork.time >= spawnTime + seconds)
            PhotonNetwork.Destroy(gameObject);
    }
}
