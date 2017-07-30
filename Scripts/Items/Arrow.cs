using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Photon.MonoBehaviour , ITurnEvent {

    public AudioClip impactClip;

    int maxDamage = 40, minDamage = 10;
    float maxDamageLength = 8.5f; // How many units the arrow must travel to reach max damage
    float arrowForce = 13f;
    double despawnTime = 6;
    double spawnTime;

    float forceMultiplier;
    bool hit = false;
    float timeHit;
    Rigidbody2D r;

    public bool isFinished { get { return hit && timeHit +1f < Time.time; } } // Event is finished when arrow hit X second ago

    float lengthMoved = 0;

    void Start()
    {
        spawnTime = PhotonNetwork.time;

        GameController.turnState = TurnState.WaitingForTurnEvents;
        TurnEvent turnEvent = new TurnEvent(this);
        GameController.eventQueue.Enqueue(turnEvent);

        r = GetComponent<Rigidbody2D>();

        CameraLerpTransform.SetLocalCameraTarget(transform);

        if (photonView.isMine)
        {
            r.simulated = true;
            forceMultiplier = (float)photonView.instantiationData[0];
            r.AddForce(transform.right * arrowForce * forceMultiplier, ForceMode2D.Impulse);
        }
    }

    void Update()
    {
        if(photonView.isMine)
        {
            if(!hit)
            {
                if(r.velocity.magnitude > 0.2f)
                {
                    Vector3 lookDir = r.velocity.normalized;
                    float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
                lengthMoved += r.velocity.magnitude * Time.deltaTime;
            }
            if(spawnTime + despawnTime <= PhotonNetwork.time)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!photonView.isMine || hit)
            return;
        GetComponent<AudioSource>().PlayOneShot(impactClip);
        r.simulated = false;
        r.velocity = Vector2.zero;
        hit = true;
        timeHit = Time.time;
        PlayerHealth ph = other.transform.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            photonView.RPC("HitRPC", PhotonTargets.Others, ph.photonView.viewID);
            transform.SetParent(other.transform);
            int damage = (int)Mathf.Clamp(lengthMoved * (maxDamage / maxDamageLength), minDamage, maxDamage);
            ph.TakeDamage(photonView.owner.ID,damage);
            print("Arrow flew " + lengthMoved + " units (damage: " + damage + ")");
        }
        else
        {
            photonView.RPC("HitRPC", PhotonTargets.All, -1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(photonView.isMine && other.tag == "Water")
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    void HitRPC (int hitViewID)
    {
        GetComponent<AudioSource>().PlayOneShot(impactClip);
        hit = true;
        timeHit = Time.time;
        if(hitViewID != -1)
        {
            Transform hitObject = PhotonView.Find(hitViewID).transform;
            transform.SetParent(hitObject.transform);
        }
    }
}
