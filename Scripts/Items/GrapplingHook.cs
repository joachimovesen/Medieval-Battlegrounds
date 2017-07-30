using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : Photon.MonoBehaviour {

    public AudioClip hitClip;
    public AudioSource mainAudioSource, backgroundAudioSource;

    public Transform ropeStart;
    public Transform rope;

    Vector2 direction;

    float speed = 5f;
    float maxRange = 7f;
    float distanceTraveled = 0f;
    public bool hasHit = false;
    bool finished = false;

    GameObject character;

    private void Start()
    {
        int characterID = (int)photonView.instantiationData[0];
        character = PhotonView.Find(characterID).gameObject;
        character.GetComponent<Player>().attatchedHook = this;

        direction = transform.up;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (finished)
            return;
        else if(hasHit)
        {
            if(other.gameObject == character)
            {
                finished = true;
            }
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hasHit = true;
            mainAudioSource.PlayOneShot(hitClip);
            backgroundAudioSource.Play();
            if (photonView.isMine)
            {
                character.GetComponent<Rigidbody2D>().gravityScale = 0;
                character.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }
    }

    private void Update()
    {
        if (finished)
        {
            if (backgroundAudioSource.isPlaying)
                backgroundAudioSource.Stop();
            return;
        }

        float distance = Vector2.Distance(ropeStart.position, character.transform.GetChild(1).position);
        rope.transform.localPosition = new Vector3(rope.transform.localPosition.x,-distance/2,rope.transform.localPosition.z);
        rope.transform.localScale = new Vector3(rope.transform.localScale.x,distance,1);

        if (!photonView.isMine)
            return;

        if (hasHit)
        {
            // Pull character towards grapple
            Vector3 direction = (transform.position - character.transform.position).normalized;
            character.transform.Translate(direction * speed  * Time.deltaTime);
        }
        else
        {
            // Rotatehook to face away from player
            Vector3 lookDir = transform.position - character.transform.GetChild(1).position;
            lookDir.Normalize();
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

            // Move the grappling hook forward until collision
            transform.Translate(direction * speed * Time.deltaTime,Space.World);
            distanceTraveled += speed * Time.deltaTime;
            if (distanceTraveled >= maxRange)
                Detach();
        }   
    }

    public void Detach()
    {
        character.GetComponent<Rigidbody2D>().gravityScale = 1;
        PhotonNetwork.Destroy(gameObject);
    }

}
