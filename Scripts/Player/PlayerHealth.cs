using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Photon.MonoBehaviour {

    public Transform popupTextPrefab;
    public Transform healthbarPrefab;

    int health;
    public int startHealth = 100;
    public int getHealth { get{ return health; } }

    Player p;

    HealthbarUI healthbar;

    void Awake()
    {
        p = GetComponent<Player>();
        health = startHealth;

        healthbar = Instantiate(healthbarPrefab, GameObject.Find("HPBars").transform).GetComponent<HealthbarUI>();
        healthbar.SetTarget(this);
    }

    public void TakeDamage (int dmgDealer, int amount, Vector2? push = null)
    {
        if (!photonView.isMine)
            photonView.RPC("TakeDamageRPC", photonView.owner, dmgDealer, amount);
        else
            TakeDamageRPC(dmgDealer,amount);
            
        if(push != null)
        {
            GetComponent<PlayerMovement>().Push(push.Value);
        }
    }

    [PunRPC]
    void TakeDamageRPC(int dmgDealer, int amount)
    {
        health -= amount;
        if(health <= 0)
        {
            health = 0;
            photonView.RPC("Die", PhotonTargets.All, dmgDealer);
            return;
        }
        photonView.RPC("TakeDamageEventRPC", PhotonTargets.All, amount,health);
    }

    [PunRPC]
    void TakeDamageEventRPC (int damage, int newHealthValue)
    {
        // Display damage taken graphic here
        PopupTextUI popup = Instantiate(popupTextPrefab, GameObject.Find("Popup").transform).GetComponent<PopupTextUI>();
        string damageText = damage > 0 ? "-" + damage : "<color=green>+"+Mathf.Abs(damage).ToString() + "</color>";
        popup.Initialize(this,damageText);
        health = newHealthValue;
    }

    [PunRPC]
    void Die (int dmgDealer)
    {
        // Do death events here
        health = 0;
        
        if (photonView.isMine)
        {
            if(p != null)
            {
                if (p.isActiveTurn)
                    p.EndTurn();
            }
            string deathEffect = "Explosion" + TeamManager.getLocalTeam().color;
            if (!p.playerControlled)
                deathEffect = "Explosion";
            if (Resources.Load(deathEffect) != null)
                PhotonNetwork.Instantiate(deathEffect, transform.position, Quaternion.identity, 0);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (p.playerControlled)
        {
            if(TeamManager.getTeamByID(p.photonView.ownerId) != null)
                TeamManager.getTeamByID(p.photonView.ownerId).RemoveUnit(p);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Water")
        {
            if (photonView.isMine)
            {
                photonView.RPC("Die", PhotonTargets.All, -1);
            }
        }
    }
}
