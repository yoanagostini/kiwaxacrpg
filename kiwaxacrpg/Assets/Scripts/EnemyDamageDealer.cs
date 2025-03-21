using UnityEngine;

// EnemyDamageDealer.cs
// This script handles dealing damage to the player
// Should be attached to an enemy or an enemy's attack hitbox

public class EnemyDamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage to deal to player on contact")]
    public float damageAmount = 10f;
    
    [Tooltip("Cooldown between damage hits in seconds")]
    public float damageCooldown = 1f;
    
    [Header("Contact Settings")]
    [Tooltip("If true, requires specific hitbox tag. If false, damages player on any collision")]
    public bool requireHitboxTag = true;
    
    [Tooltip("Tag to check for if requireHitboxTag is true")]
    public string hitboxTag = "PlayerHitbox";
    
    // Track cooldown time
    private float lastDamageTime = 0f;
    
    // References to track ongoing collisions
    private GameObject playerObject = null;
    
    // Update is called once per frame
    private void Update()
    {
        // If we're in contact with the player and our cooldown is up, deal damage
        if (playerObject != null && Time.time - lastDamageTime >= damageCooldown)
        {
            DealDamageToPlayer(playerObject);
        }
    }
    
    // Check for collisions
    private void OnCollisionEnter(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }
    
    // Check for trigger collisions
    private void OnTriggerEnter(Collider other)
    {
        CheckCollision(other.gameObject);
    }
    
    // Stay in trigger means continuous contact
    private void OnTriggerStay(Collider other)
    {
        CheckCollision(other.gameObject);
    }
    
    // Stay in collision means continuous contact
    private void OnCollisionStay(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }
    
    // When leaving contact, clear the player reference
    private void OnCollisionExit(Collision collision)
    {
        if (playerObject == collision.gameObject)
        {
            playerObject = null;
        }
    }
    
    // When leaving trigger, clear the player reference
    private void OnTriggerExit(Collider other)
    {
        if (playerObject == other.gameObject)
        {
            playerObject = null;
        }
    }
    
    // Helper method to check if an object should receive damage
    private void CheckCollision(GameObject obj)
    {
        // If we require a specific hitbox tag
        if (requireHitboxTag)
        {
            // If the object has the hitbox tag, store reference
            if (obj.CompareTag(hitboxTag))
            {
                playerObject = obj;
            }
        }
        else
        {
            // If the object is the player, store reference
            if (obj.CompareTag("Player"))
            {
                playerObject = obj;
            }
        }
    }
    
    // Helper method to deal damage to the player
    private void DealDamageToPlayer(GameObject targetObject)
    {
        // Update last damage time
        lastDamageTime = Time.time;
        
        // If we hit a hitbox, find the parent player object
        PlayerHealth playerHealth = null;
        
        if (targetObject.CompareTag(hitboxTag))
        {
            // If this is a hitbox, the PlayerHealth is likely on the parent
            playerHealth = targetObject.GetComponentInParent<PlayerHealth>();
        }
        else if (targetObject.CompareTag("Player"))
        {
            // If this is the player directly, get component from this object
            playerHealth = targetObject.GetComponent<PlayerHealth>();
        }
        
        // If we found a PlayerHealth component, apply damage
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            
            // Visual feedback or sound effects could be added here
            Debug.Log($"Enemy dealt {damageAmount} damage to player");
        }
        else
        {
            Debug.LogWarning("PlayerHealth component not found on collision target");
        }
    }
}