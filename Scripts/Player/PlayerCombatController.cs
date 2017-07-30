using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : Photon.MonoBehaviour {

    [Header("References")]
    public Transform center;
    public Transform weapon;
    public Transform aim;
    public Transform rHand;
    public GameObject sword;

    float sensitivity = 2f;
    float forceMultiplier;
    float timePressed;
    float secondsToCharge = 1.75f;
    bool isCharging = false;

    int swordDamage = 17;

    Player p;

    [Header("Charging Colors")]
    public Color backgroundColor;
    public Color chargeColor;

    Texture2D backgroundTexture, chargeTexture;

    void Awake()
    {
        p = GetComponent<Player>();
    }

	void Start ()
    {
        if(photonView.isMine)
        {
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, backgroundColor);
            backgroundTexture.Apply();

            chargeTexture = new Texture2D(1, 1);
            chargeTexture.SetPixel(0, 0, chargeColor);
            chargeTexture.Apply();

            GadgetUI.onWeaponChangeEvent += OnWeaponChanged;
        }
    }
	
	void Update ()
    {
        if (!photonView.isMine)
            return;

        if(p.isActiveTurn)
        {
            RotateWeapon(Input.GetAxis("Vertical"));

            if (Input.GetKeyDown(KeyCode.Space) && !GadgetUI.isVisible)
            {
                ButtonDownHandler(GadgetUI.activeGadget);
            }

            if(Input.GetKeyUp(KeyCode.Space) && isCharging)
            {
                ButtonUpHandler(GadgetUI.activeGadget);
            }

            if(!isCharging && p.attatchedHook == null && Input.GetKeyDown(KeyCode.Tab))
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
                GadgetUI.TogglePanel();
            }
        }
	}

    void ButtonDownHandler (GadgetType type)
    {
        if (!Gadgets.CanUse(type))
            return;

        switch(type)
        {
            case GadgetType.Arrow:
                forceMultiplier = 0f;
                isCharging = true;
                timePressed = Time.time;
                break;
            case GadgetType.Grapple:
                SpawnHook();
                break;
            case GadgetType.Sword:
                SwingSword();
                break;
        }
    }

    void ButtonUpHandler (GadgetType type)
    {
        if (!Gadgets.CanUse(type))
            return;

        switch (type)
        {
            case GadgetType.Arrow:
                isCharging = false;
                forceMultiplier = Mathf.Clamp((Time.time - timePressed) / secondsToCharge, 0, 1);
                SpawnArrow(forceMultiplier);
                break;
        }
    }

    void SpawnArrow (float force)
    {
        Vector3 lookDir = aim.transform.position - weapon.transform.position;
        lookDir.Normalize();
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        Quaternion arrowSpawnRot = Quaternion.AngleAxis(angle, Vector3.forward);

        PhotonNetwork.Instantiate("Arrow", aim.position, arrowSpawnRot, 0, new object[] { force });
        p.EndTurn();
    }

    void SpawnHook ()
    {
        if (Gadgets.grappleUsed)
            return;

        if (p.attatchedHook != null)
            p.attatchedHook.Detach();

        Vector3 lookDir = aim.transform.position - weapon.transform.position;
        lookDir.Normalize();
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        Quaternion hookSpawnRot = Quaternion.AngleAxis(angle-90, Vector3.forward);

        PhotonNetwork.Instantiate("GrapplingHook", aim.position, hookSpawnRot, 0, new object[] { photonView.viewID });
        Gadgets.grappleUsed = true;
    }

    void SwingSword ()
    {
        PhotonNetwork.Instantiate(sword.name, rHand.position, Quaternion.identity,0,new object[] { photonView.viewID } );

        int mask = 1 << LayerMask.NameToLayer("Player");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(rHand.GetChild(0).position, 0.4f, mask);
        foreach(Collider2D col in colliders)
        {
            if (col.gameObject == gameObject)
                continue;

            PlayerHealth ph = col.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(PhotonNetwork.player.ID, swordDamage, new Vector2?(new Vector2(4 * transform.localScale.x, 4)));
            }
        }
        p.EndTurn();
    }

    void RotateWeapon(float axis)
    {
        float rot = weapon.rotation.eulerAngles.z;

        var rotationTarget = Mathf.Clamp(rot - (axis * -transform.localScale.x), 90, 270);
        rot = Mathf.MoveTowardsAngle(rot, rotationTarget, Time.deltaTime * 30 * sensitivity);

        weapon.rotation = Quaternion.Euler(0f, 0f, rot);
    }

    public void Activate()
    {
        SelectAimtype(GadgetUI.activeGadget);
        Gadgets.grappleUsed = false;
    }

    public void EndTurn ()
    {
        AimUI.SetTarget(null);
        isCharging = false;
        if (p.attatchedHook != null)
            p.attatchedHook.Detach();
    }

    void OnWeaponChanged (GadgetType newWeapon)
    {
        if (!p.isActiveTurn)
            return;

        SelectAimtype(newWeapon);
    }

    void SelectAimtype (GadgetType type)
    {
        if(!Gadgets.CanUse(type))
        {
            AimUI.SetTarget(null);
            rHand.GetChild(0).gameObject.SetActive(false);
            return;
        }
        switch (type)
        {
            case GadgetType.Arrow:
                AimUI.SetTarget(aim);
                rHand.GetChild(0).gameObject.SetActive(false);
                break;
            case GadgetType.Grapple:
                AimUI.SetTarget(aim);
                rHand.GetChild(0).gameObject.SetActive(false);
                break;
            case GadgetType.Sword:
                AimUI.SetTarget(null);
                rHand.GetChild(0).gameObject.SetActive(true);
                break;
        }
    }

    void OnGUI()
    {
        if (!photonView.isMine || !isCharging)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Rect backRect = new Rect(screenPos.x-50,Screen.height-screenPos.y+15,100,15);
        GUI.DrawTexture(backRect, backgroundTexture);

        forceMultiplier = Mathf.Clamp((Time.time - timePressed) / secondsToCharge, 0, 1);
        Rect chargeRect = new Rect(screenPos.x-50, Screen.height-screenPos.y+15, 100 * forceMultiplier, 15);
        GUI.DrawTexture(chargeRect, chargeTexture);
    } 
}

public enum GadgetType
{
    Arrow,
    Sword,
    Grapple
}
