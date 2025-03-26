using UnityEngine;

// BasicEnemy.cs
// Simple enemy that follows the player and drops items on death
public class BasicEnemy : EnemyBase
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed of the enemy")]
    public float moveSpeed = 3f;
    
    [Tooltip("How fast the enemy rotates to face the player")]
    public float rotationSpeed = 5f;
    
    [Tooltip("Distance at which the enemy stops approaching the player")]
    public float stoppingDistance = 2.5f;
    
    [Header("Stats")]
    [Tooltip("Enemy's maximum health")]
    public float maxHealth = 50f;
    
    [Tooltip("Current health of the enemy")]
    private float currentHealth;
    
    // Reference to the player transform
    private Transform playerTransform;
    
    // Reference to the enemy's Rigidbody component
    private Rigidbody rb;
    
    // Start is called before the first frame update
    void Start()
    {
        // Find the player by tag
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        
        // Initialize health
        currentHealth = maxHealth;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Only move if we have a player reference
        if (playerTransform != null)
        {
            MoveTowardsPlayer();
        }
    }
    
    // Move the enemy towards the player
    void MoveTowardsPlayer()
    {
        // Calculate direction to player
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0; // Keep movement on the horizontal plane
        float distance = direction.magnitude;
        
        // Only move if we're outside the stopping distance
        if (distance > stoppingDistance)
        {
            // Normalize the direction
            direction.Normalize();
            
            // Move towards player
            rb.linearVelocity = direction * moveSpeed;
            
            // Rotate towards the player
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Stop moving when close enough
            rb.linearVelocity = Vector3.zero;
            
            // Keep facing the player
            Vector3 lookDirection = playerTransform.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    // Implementation of the abstract TakeDamage method from EnemyBase
    public override void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // Check if the enemy is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Handle enemy death
    private void Die()
    {      
        // Here you could spawn particles, play sound effects, etc.
        
        // Destroy the enemy GameObject
        Destroy(gameObject);
    }
}