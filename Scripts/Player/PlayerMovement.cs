using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerMovement : Photon.MonoBehaviour {

    public AudioSource audioSource;
    public AudioClip jumpLandClip;
    public Collider2D groundCheck;

    float movementSpeed = 1f;
    float jumpForce = 6f;
    float fallHeight = 0;
    float fallLimit = 4.5f;

    float groundCheckWidth;
    int groundLayers;
    bool previouslyGrounded = true;
    bool pushed = false;

    Rigidbody2D r;
    Player p;

    void Awake ()
    {
        fallHeight = transform.position.y;
        r = GetComponent<Rigidbody2D>();
        p = GetComponent<Player>();
        groundLayers = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Player");

        if (!photonView.isMine && p.playerControlled)
        {
            r.isKinematic = true;
        }
        else
        {
            r.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
    }

    void Update ()
    {
        bool grounded = false;

        if (groundCheck != null && groundCheck.IsTouchingLayers(groundLayers))
        {
            grounded = true;
            if (photonView.isMine && Gadgets.infiniteGrapple && p.attatchedHook == null)
                Gadgets.grappleUsed = false;
            if (!previouslyGrounded)
            {
                if(photonView.isMine && pushed)
                {
                    r.constraints = p.isActiveTurn ? r.constraints = RigidbodyConstraints2D.FreezeRotation : r.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
                    pushed = false;
                }
                audioSource.PlayOneShot(jumpLandClip);
            }
        }

        if (!photonView.isMine)
        {
            previouslyGrounded = grounded;
            return;
        }

        if (p.isActiveTurn)
        {
            if (!GadgetUI.isVisible && p.attatchedHook == null)
            {
                float xInput = Input.GetAxis("Horizontal");
                r.velocity = new Vector2(xInput * movementSpeed, r.velocity.y);
                if (xInput != 0)
                    transform.localScale = xInput > 0 ? Vector3.one : new Vector3(-1, 1);

                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    // Jump!
                    if (grounded)
                        Jump();
                }
            }
            else if(p.attatchedHook != null)
            {
                r.velocity = new Vector2(0,r.velocity.y);

                if(Input.GetKeyDown(KeyCode.LeftShift))
                {
                    p.attatchedHook.Detach();
                    fallHeight = transform.position.y;
                    Jump();
                }
            }
        }

        if (!grounded)
        {
            if (transform.position.y > fallHeight)
                fallHeight = transform.position.y;
        }
        else
        {
            if (!previouslyGrounded)
            {
                if(transform.position.y < fallHeight)
                    FallHandler((fallHeight - transform.position.y));
            }
            fallHeight = transform.position.y;
        }

        previouslyGrounded = grounded;
	}

    void FallHandler (float distanceFallen)
    {
        if (p.attatchedHook != null && p.attatchedHook.hasHit)
            return;

        if (distanceFallen >= fallLimit)
        {
            // Takes damage for every 0.5 units, with the minimum damage taken being 3.
            int halfUnits = (int)Mathf.Floor((distanceFallen - fallLimit) / 0.5f); 
            int fallDamage = Mathf.Max(3,halfUnits * 2);
            GetComponent<PlayerHealth>().TakeDamage(PhotonNetwork.player.ID, fallDamage);
            p.EndTurn();
        }
    }

    void Jump ()
    {
        r.AddForce(transform.up * jumpForce,ForceMode2D.Impulse);
    }

    public void Activate ()
    {
        r.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void EndTurn ()
    {
        r.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        r.velocity = new Vector2(0, r.velocity.y);
    }

    public void Push (Vector2 force)
    {
        if (photonView.isMine)
            PushRPC(force);
        else
            photonView.RPC("PushRPC", photonView.owner, force);
    }

    [PunRPC]
    void PushRPC (Vector2 force)
    {
        pushed = true;
        r.constraints = RigidbodyConstraints2D.FreezeRotation;
        r.AddForce(force,ForceMode2D.Impulse);
    }
}
