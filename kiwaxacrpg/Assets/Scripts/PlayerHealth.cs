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
            Debug.LogWarning("PlayerHealth could not find a PlayerController component!");
            maxHealth = 100f; // Fallback default if no controller found
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
                    Debug.Log($"Retrieved MaxHealth from WarriorClass: {maxHealth}");
                    
                    // Get current health from controller too
                    try {
                        currentHealth = playerController.GetStat("Health");
                        Debug.Log($"Retrieved current Health from WarriorClass: {currentHealth}");
                    }
                    catch (System.Exception) {
                        // If we can't get current health, use max health
                        currentHealth = maxHealth;
                        Debug.Log($"Using MaxHealth as current health: {currentHealth}");
                    }
                }
                else
                {
                    // If MaxHealth is invalid, try Health stat instead
                    Debug.LogWarning("MaxHealth stat was invalid, trying Health stat instead");
                    float health = playerController.GetStat("Health");
                    
                    if (health > 0)
                    {
                        maxHealth = health;
                        currentHealth = maxHealth;
                        Debug.Log($"Using Health stat as MaxHealth: {maxHealth}");
                    }
                    else
                    {
                        // If both fail, use default
                        maxHealth = 100f;
                        currentHealth = maxHealth;
                        Debug.LogWarning($"Could not get valid health values from controller, using default: {maxHealth}");
                    }
                }
            }
            catch (System.Exception e)
            {
                // Fallback to default values if stat retrieval fails
                maxHealth = 100f;
                currentHealth = maxHealth;
                Debug.LogWarning($"Could not get health stats from PlayerController: {e.Message}. Using default value: {maxHealth}");
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
            Debug.Log($"Player is invincible, ignored {damageAmount} damage");
            return;
        }
            
        // Apply damage - log before and after health for debugging
        float healthBefore = currentHealth;
        currentHealth -= damageAmount;
        float healthAfter = currentHealth;
        
        // Clamp health to prevent negative values
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Sync health with player controller stats if available
        if (playerController != null && isInitialized)
        {
            try
            {
                playerController.SetStat("Health", currentHealth);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set Health stat on PlayerController: {e.Message}");
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
        
        // Detailed logging to diagnose multiple enemy damage issue
        Debug.Log($"Player took {damageAmount} damage from source {System.Environment.StackTrace.Substring(0, 100)}... Health: {healthBefore} -> {healthAfter}");
    }
    
    // Coroutine for temporary invincibility after taking damage
    private IEnumerator InvincibilityCoroutine()
    {
        // Set invincible flag
        isInvincible = true;
        Debug.Log("Player invincibility started");
        
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
        Debug.Log("Player invincibility ended");
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
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set Health stat on PlayerController: {e.Message}");
            }
        }
        
        Debug.Log($"Player healed for {healAmount}. Current health: {currentHealth}");
    }
    
    // Handle player death
    private void Die()
    {
        Debug.Log("Player died!");
        
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
        
        // This could be expanded based on your game's requirements
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