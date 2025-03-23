using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// PlayerHealth.cs
// This script handles player health, damage, and death
// Can be attached to the player object to manage health state

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the player - pulled from WarriorClass")]
    private float maxHealth = 100f;
    
    [Tooltip("Current health of the player")]
    private float currentHealth;
    
    [Header("Damage Settings")]
    [Tooltip("Invincibility time after taking damage (in seconds)")]
    public float invincibilityTime = 1f;
    
    [Tooltip("Visual effect when player is invincible")]
    public GameObject invincibilityEffect;
    
    [Tooltip("Sound to play when player takes damage")]
    public AudioClip damageSound;
    
    // Flag to track if player is currently invincible
    private bool isInvincible = false;
    
    // Reference to player's renderer for visual effects
    private Renderer playerRenderer;
    
    // Reference to audio source
    private AudioSource audioSource;
    
    // Reference to the player controller or warrior class
    private PlayerController playerController;
    
    // Flag to track if initialization is complete
    private bool isInitialized = false;
    
    // Awake is called before Start
    private void Awake()
    {
        // Initialize references first
        playerRenderer = GetComponentInChildren<Renderer>();
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
        
        // If no audio source is found, add one
        if (audioSource == null && damageSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Set a default health value in case we access health before Start completes
        currentHealth = maxHealth;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // Check if we have a player controller
        if (playerController == null)
        {
            // Fallback default if no controller found
            maxHealth = 100f;
            currentHealth = maxHealth;
        }
        else
        {
            // Get the health values from the player controller's stats
            try
            {
                // Try to get MaxHealth stat from controller first
                maxHealth = playerController.GetStat("MaxHealth");
                
                // If MaxHealth exists, use it
                if (maxHealth > 0)
                {
                    // Get current health from controller too
                    try {
                        currentHealth = playerController.GetStat("Health");
                    }
                    catch (System.Exception) {
                        // If we can't get current health, use max health
                        currentHealth = maxHealth;
                    }
                }
                else
                {
                    // If MaxHealth is invalid, try Health stat instead
                    float health = playerController.GetStat("Health");
                    
                    if (health > 0)
                    {
                        maxHealth = health;
                        currentHealth = maxHealth;
                    }
                    else
                    {
                        // If both fail, use default
                        maxHealth = 100f;
                        currentHealth = maxHealth;
                    }
                }
            }
            catch (System.Exception)
            {
                // Fallback to default values if stat retrieval fails
                maxHealth = 100f;
                currentHealth = maxHealth;
            }
        }
        
        // Initialization complete
        isInitialized = true;
    }
    
    // Update health bar UI - Empty placeholder method
    void UpdateHealthUI()
    {
        // No longer needed - HealthUI handles this now
        // This method is kept empty for compatibility with any existing calls
    }
    
    // Public method to take damage - can be called by enemies or other damage sources
    public void TakeDamage(float damageAmount)
    {
        // If player is invincible, ignore damage
        if (isInvincible)
        {
            return;
        }
            
        // Apply damage
        currentHealth -= damageAmount;
        
        // Clamp health to prevent negative values
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Sync health with player controller stats if available
        if (playerController != null && isInitialized)
        {
            try
            {
                playerController.SetStat("Health", currentHealth);
            }
            catch (System.Exception)
            {
                // Silently fail if setting stat fails
            }
        }
        
        // Play damage sound if available
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Check if player is dead
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility period
            StartCoroutine(InvincibilityCoroutine());
        }
    }
    
    // Coroutine for temporary invincibility after taking damage
    private IEnumerator InvincibilityCoroutine()
    {
        // Set invincible flag
        isInvincible = true;
        
        // Show invincibility effect if available
        if (invincibilityEffect != null)
        {
            invincibilityEffect.SetActive(true);
        }
        
        // Flash the player renderer as visual feedback
        if (playerRenderer != null)
        {
            // Flash 5 times during invincibility period
            float flashInterval = invincibilityTime / 10;
            
            for (int i = 0; i < 5; i++)
            {
                playerRenderer.enabled = false;
                yield return new WaitForSeconds(flashInterval);
                playerRenderer.enabled = true;
                yield return new WaitForSeconds(flashInterval);
            }
        }
        else
        {
            // If no renderer, just wait for invincibility time
            yield return new WaitForSeconds(invincibilityTime);
        }
        
        // Hide invincibility effect
        if (invincibilityEffect != null)
        {
            invincibilityEffect.SetActive(false);
        }
        
        // Reset invincible flag
        isInvincible = false;
    }
    
    // Public method to heal the player
    public void Heal(float healAmount)
    {
        // Add health
        currentHealth += healAmount;
        
        // Clamp to max health
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        // Sync health with player controller stats if available
        if (playerController != null && isInitialized)
        {
            try
            {
                playerController.SetStat("Health", currentHealth);
            }
            catch (System.Exception)
            {
                // Silently fail if setting stat fails
            }
        }
    }
    
    // Handle player death
    private void Die()
    {
        // Here you would handle what happens when player dies
        // For example: show game over screen, restart level, play death animation, etc.
        
        // For now, just disable player movement and collisions
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
    
    // Getter methods for current and max health
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
}