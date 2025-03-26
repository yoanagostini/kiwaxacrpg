using UnityEngine;

// PlayerHitbox.cs
// This script identifies an object as a player hitbox
// It's a simple component to add to hitbox colliders

public class PlayerHitbox : MonoBehaviour
{
    [Tooltip("Reference to the player's health component")]
    public PlayerHealth playerHealth;
    
    // Start is called before the first frame update
    void Start()
    {
        // If no player health reference was provided, try to find it in the parent
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
            
            if (playerHealth == null)
            {
                Debug.LogError("PlayerHitbox needs a reference to PlayerHealth! Add one in the inspector or make sure this object is a child of the player.");
            }
        }
        
        // Make sure this object has the correct tag
        if (!gameObject.CompareTag("PlayerHitbox"))
        {
            //Debug.LogWarning("PlayerHitbox object should have the 'PlayerHitbox' tag! Adding it now.");
            gameObject.tag = "PlayerHitbox";
        }
    }
    
    // This method can be called by enemy damage dealers if needed
    public void TakeDamage(float damageAmount)
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
        }
    }
}