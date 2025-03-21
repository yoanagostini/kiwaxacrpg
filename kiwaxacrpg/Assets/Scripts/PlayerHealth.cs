using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// PlayerHealth.cs
// This script handles player health, damage, and death
// Can be attached to the player object to manage health state

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the player")]
    public float maxHealth = 100f;
    
    [Tooltip("Current health of the player")]
    private float currentHealth;
    
    [Header("Damage Settings")]
    [Tooltip("Invincibility time after taking damage (in seconds)")]
    public float invincibilityTime = 1f;
    
    [Tooltip("Visual effect when player is invincible")]
    public GameObject invincibilityEffect;
    
    [Tooltip("Sound to play when player takes damage")]
    public AudioClip damageSound;
    
    [Header("UI References")]
    [Tooltip("Reference to health bar UI element")]
    public Image healthBarImage;
    
    [Tooltip("Text to display health value")]
    public Text healthText;
    
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
        }
        else
        {
            // Get the health value from the player controller's stats if available
            try
            {
                // Try to get Health stat from controller
                float controllerHealth = playerController.GetStat("Health");
                if (controllerHealth > 0)
                {
                    // Use the health value from the controller
                    maxHealth = controllerHealth;
                    currentHealth = maxHealth;
                    Debug.Log($"Initialized player health from PlayerController stats: {maxHealth}");
                }
                
                // Try to set MaxHealth stat if it doesn't exist
                try {
                    playerController.SetStat("MaxHealth", maxHealth);
                } catch (System.Exception) {
                    // It's okay if this fails
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not get Health stat from PlayerController: {e.Message}. Using default value: {maxHealth}");
                
                // Fall back to default values
                currentHealth = maxHealth;
            }
        }
        
        // Update UI if available
        UpdateHealthUI();
        
        // Initialization complete
        isInitialized = true;
    }
    
    // Update health bar UI
    void UpdateHealthUI()
    {
        // Update health bar fill amount if reference exists
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }
        
        // Update health text if reference exists
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)}/{maxHealth}";
        }
    }
    
    // Public method to take damage - can be called by enemies or other damage sources
    public void TakeDamage(float damageAmount)
    {
        // If player is invincible, ignore damage
        if (isInvincible)
            return;
            
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
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set Health stat on PlayerController: {e.Message}");
            }
        }
        
        // Update UI
        UpdateHealthUI();
        
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
        
        Debug.Log($"Player took {damageAmount} damage. Current health: {currentHealth}");
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
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set Health stat on PlayerController: {e.Message}");
            }
        }
        
        // Update UI
        UpdateHealthUI();
        
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