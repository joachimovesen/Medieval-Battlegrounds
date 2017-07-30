using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CrateSpawner : Photon.MonoBehaviour {

    public Transform crateSpawns;

    public void SpawnCrate ()
    {
        int ran = Random.Range(0, crateSpawns.childCount);
        if(crateSpawns.childCount > 0)
        {
            PhotonNetwork.InstantiateSceneObject("HealthCrate", crateSpawns.GetChild(ran).position + Vector3.up, Quaternion.identity,0,null);
            Debug.Log("Spawned crate");
        }
    }
}
