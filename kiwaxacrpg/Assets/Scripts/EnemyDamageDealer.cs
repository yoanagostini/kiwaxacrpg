using UnityEngine;
using System.Collections;

// EnemyDamageDealer.cs
// This script handles dealing damage to the player
// Each enemy deals damage independently on its own cooldown cycle

public class EnemyDamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage to deal to player on contact")]
    public float damageAmount = 10f;
    
    [Tooltip("Cooldown between damage hits in seconds")]
    public float damageCooldown = 1f;
    
    [Header("Contact Settings")]
    [Tooltip("If true, requires specific hitbox tag. If false, damages player on any collision")]
    public bool requireHitboxTag = false;
    
    [Tooltip("Tag to check for if requireHitboxTag is true")]
    public string hitboxTag = "PlayerHitbox";
    
    [Header("Debug")]
    [Tooltip("Enable verbose debug messages")]
    public bool debugMode = true;
    
    // For tracking this enemy's cooldown
    private bool canDealDamage = true;
    
    // Track if we're currently in contact with player
    private bool inContactWithPlayer = false;
    private PlayerHealth playerHealth = null;
    
    // Start is called before the first frame update
    void Start()
    {
        // Make sure this enemy has a unique instance ID
        name = name + "_" + GetInstanceID();
        
        if (debugMode)
        {
            Debug.Log($"Enemy {name} initialized with damage: {damageAmount}, cooldown: {damageCooldown}");
        }
    }
    
    void Update()
    {
        // If we're in contact with player and can deal damage, deal damage
        if (inContactWithPlayer && canDealDamage && playerHealth != null)
        {
            DealDamage();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }
    
    void OnCollisionStay(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        CheckCollision(other.gameObject);
    }
    
    void OnTriggerStay(Collider other)
    {
        CheckCollision(other.gameObject);
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (IsPlayerObject(collision.gameObject))
        {
            inContactWithPlayer = false;
            if (debugMode) Debug.Log($"Enemy {name} no longer in contact with player");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (IsPlayerObject(other.gameObject))
        {
            inContactWithPlayer = false;
            if (debugMode) Debug.Log($"Enemy {name} no longer in contact with player");
        }
    }
    
    // Check if this is a player object we can damage
    private bool IsPlayerObject(GameObject obj)
    {
        if (requireHitboxTag)
        {
            return obj.CompareTag(hitboxTag);
        }
        else
        {
            return obj.CompareTag("Player");
        }
    }
    
    // Check collision and find player health if needed
    private void CheckCollision(GameObject obj)
    {
        if (IsPlayerObject(obj))
        {
            inContactWithPlayer = true;
            
            // Find player health component if we don't have it yet
            if (playerHealth == null)
            {
                if (obj.CompareTag(hitboxTag))
                {
                    playerHealth = obj.GetComponentInParent<PlayerHealth>();
                }
                else
                {
                    playerHealth = obj.GetComponent<PlayerHealth>();
                }
                
                if (playerHealth == null)
                {
                    Debug.LogWarning($"Enemy {name} could not find PlayerHealth component!");
                }
                else if (debugMode)
                {
                    Debug.Log($"Enemy {name} found PlayerHealth component");
                }
            }
        }
    }
    
    // Deal damage and start cooldown
    private void DealDamage()
    {
        // Make sure we don't deal damage again until cooldown is over
        canDealDamage = false;
        
        // Apply damage to player
        playerHealth.TakeDamage(damageAmount);
        
        if (debugMode)
        {
            Debug.Log($"Enemy {name} dealt {damageAmount} damage to player");
        }
        
        // Start cooldown coroutine
        StartCoroutine(DamageCooldown());
    }
    
    // Coroutine for damage cooldown
    private IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        canDealDamage = true;
        
        if (debugMode && inContactWithPlayer)
        {
            Debug.Log($"Enemy {name} cooldown finished, can deal damage again");
        }
    }
}