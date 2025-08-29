using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script defines 'Enemy's' health and behavior. 
/// </summary>
public class Enemy : MonoBehaviour
{

    #region FIELDS
    [Tooltip("Health points in integer")]
    public int health;

    [Tooltip("Enemy's projectile prefab")]
    public GameObject Projectile;

    [Tooltip("VFX prefab generating after destruction")]
    public GameObject destructionVFX;
    public GameObject hitEffect;

    [HideInInspector] public int shotChance; //probability of 'Enemy's' shooting during tha path
    [HideInInspector] public float shotTimeMin, shotTimeMax; //max and min time for shooting from the beginning of the path

    [Header("Shield Parameters")]
    [Tooltip("VFX for the shield (e.g., a Particle System)")]
    public GameObject shieldVFX;
    [Tooltip("Duration the shield is active (in seconds)")]
    public float shieldActiveDuration = 2f;
    [Tooltip("Interval between shield activations (in seconds)")]
    public float shieldActivationInterval = 10f;
    private bool isShielded = false;
    #endregion

    private void Start()
    {
        // Start the shooting coroutine
        Invoke("ActivateShooting", Random.Range(shotTimeMin, shotTimeMax));

        // Start the repeating shield activation cycle
        InvokeRepeating("ActivateShield", 0, shieldActivationInterval);
    }

    // Activates the shield
    void ActivateShield()
    {
        if (shieldVFX != null)
        {
            isShielded = true;
            shieldVFX.SetActive(true);
            // Deactivate the shield after a set duration
            Invoke("DeactivateShield", shieldActiveDuration);
        }
    }

    // Deactivates the shield
    void DeactivateShield()
    {
        isShielded = false;
        if (shieldVFX != null)
        {
            shieldVFX.SetActive(false);
        }
    }

    //coroutine making a shot
    void ActivateShooting()
    {
        if (Random.value < (float)shotChance / 100)
        {
            Instantiate(Projectile, gameObject.transform.position, Quaternion.identity);
        }
    }

    //method of getting damage for the 'Enemy'
    public void GetDamage(int damage)
    {
        // First, check if the shield is active
        if (isShielded)
        {
            // The shield absorbs the damage completely
            Instantiate(hitEffect, transform.position, Quaternion.identity, transform);
        }
        else // If the shield is not active, apply damage to the enemy's health
        {
            health -= damage;
            if (health <= 0)
                Destruction();
            else
                Instantiate(hitEffect, transform.position, Quaternion.identity, transform);
        }
    }

    //if 'Enemy' collides 'Player', 'Player' gets the damage equal to projectile's damage value
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (Projectile.GetComponent<Projectile>() != null)
                Player.instance.GetDamage(Projectile.GetComponent<Projectile>().damage);
            else
                Player.instance.GetDamage(1);
        }
    }

    //method of destroying the 'Enemy'
    void Destruction()
    {
        Instantiate(destructionVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}