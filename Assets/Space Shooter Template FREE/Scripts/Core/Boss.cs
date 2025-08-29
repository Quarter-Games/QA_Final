using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script defines 'Boss's' health and behavior. 
/// </summary>
public class Boss : MonoBehaviour
{

    #region FIELDS
    [Tooltip("Health points in integer")]
    public int health;

    [Tooltip("Enemy's projectile prefab")]
    public GameObject Projectile;

    [Tooltip("VFX prefab generating after destruction")]
    public GameObject destructionVFX;
    public GameObject hitEffect;

    public int shotChance; //probability of 'Enemy's' shooting during tha path
    [HideInInspector] public float shotTimeMin, shotTimeMax; //max and min time for shooting from the beginning of the path

    // New fields for movement
    [Header("Movement")]
    [Tooltip("Speed of the boss's movement")]
    public float moveSpeed = 3f;
    [Tooltip("How often the boss changes direction")]
    public float changeDirectionTime = 3f;
    [Tooltip("Maximum distance the boss can move from its starting position")]
    public float maxMovementRadius = 5f; // New field for movement radius

    private Transform playerTransform;
    private Vector2 targetPosition;
    private float timeToChangeDirection;
    private Vector2 startPosition; // Stores the boss's initial position

    #endregion

    void Start()
    {
        // Find the player object and get its Transform
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Start the shooting coroutine
        InvokeRepeating("ActivateShooting", Random.Range(shotTimeMin, shotTimeMax), Random.Range(shotTimeMin, shotTimeMax));

        // Set the initial time to change direction
        timeToChangeDirection = Time.time + changeDirectionTime;
        startPosition = transform.position; // Store the initial position
        SetNewTargetPosition();
    }

    void Update()
    {
        // Handle boss movement
        if (Time.time > timeToChangeDirection)
        {
            SetNewTargetPosition();
            timeToChangeDirection = Time.time + changeDirectionTime;
        }

        // Move towards the target position
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Optional: Make the boss look at the player
        if (playerTransform != null)
        {
            Vector2 direction = playerTransform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }

    void SetNewTargetPosition()
    {
        // Calculate a new random position within the defined radius
        Vector2 randomOffset = Random.insideUnitCircle * maxMovementRadius;
        targetPosition = startPosition + randomOffset;
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
        health -= damage;
        if (health <= 0)
            Destruction();
        else
            Instantiate(hitEffect, transform.position, Quaternion.identity, transform);
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