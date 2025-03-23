using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HealthUI.cs - Modified to work with Slider
public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The slider used for the health bar")]
    public Slider healthBarSlider;
    
    [Tooltip("Text to display current/max health values")]
    public Text healthText;
    
    [Header("Settings")]
    [Tooltip("Reference to the player's health component")]
    public PlayerHealth playerHealth;
    
    [Tooltip("Color of the health bar")]
    public Color barColor = Color.red;
    
    // Main player controller reference
    private PlayerController playerController;
    
    // Start is called before the first frame update
    void Start()
    {
        // If no player health is assigned, try to find it
        if (playerHealth == null)
        {
            Debug.Log("No PlayerHealth assigned, searching for one...");
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
            
            if (playerHealth == null)
            {
                Debug.LogError("ERROR: No PlayerHealth component found in the scene!");
            }
            else
            {
                Debug.Log("Found PlayerHealth component on: " + playerHealth.gameObject.name);
            }
        }
        else
        {
            Debug.Log("PlayerHealth already assigned to: " + playerHealth.gameObject.name);
        }
        
        // Try to get player controller if available
        if (playerHealth != null)
        {
            playerController = playerHealth.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Debug.Log("Found PlayerController component");
            }
            else
            {
                Debug.Log("No PlayerController found on the same object as PlayerHealth");
            }
        }
        
        // Check UI references
        if (healthBarSlider == null)
        {
            Debug.LogError("ERROR: No health bar slider assigned!");
        }
        else
        {
            // Set the health bar color to red
            Image fillImage = healthBarSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = barColor;
                Debug.Log("Set health bar to red color at initialization");
            }
        }
        
        // Initialize the UI
        UpdateHealthUI();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Update the UI every frame to reflect current health
        UpdateHealthUI();
    }
    
    // Update the health bar and text
    public void UpdateHealthUI()
    {
        // If we have valid references
        if (playerHealth != null)
        {
            float currentHealth;
            float maxHealth;
            
            // Get health values either directly or from player controller stats
            if (playerController != null)
            {
                // Try to get values from PlayerController
                try {
                    currentHealth = playerController.GetStat("Health");
                    maxHealth = playerController.GetStat("MaxHealth");
                    Debug.Log($"From PlayerController: Health = {currentHealth}/{maxHealth}");
                }
                catch (System.Exception e) {
                    // Fall back to PlayerHealth if PlayerController fails
                    Debug.LogWarning($"Error getting health from PlayerController: {e.Message}");
                    currentHealth = playerHealth.GetCurrentHealth();
                    maxHealth = playerHealth.GetMaxHealth();
                    Debug.Log($"Fallback from PlayerHealth: Health = {currentHealth}/{maxHealth}");
                }
            }
            else
            {
                // Get values directly from PlayerHealth
                currentHealth = playerHealth.GetCurrentHealth();
                maxHealth = playerHealth.GetMaxHealth();
                Debug.Log($"Directly from PlayerHealth: Health = {currentHealth}/{maxHealth}");
            }
            
            // Prevent division by zero
            if (maxHealth <= 0)
            {
                Debug.LogError("ERROR: MaxHealth is zero or negative!");
                maxHealth = 100f; // Use default to avoid errors
            }
            
            // Calculate health percentage
            float healthPercentage = currentHealth / maxHealth;
            
            // Update the health bar slider if available
            if (healthBarSlider != null)
            {
                float oldValue = healthBarSlider.value;
                healthBarSlider.value = healthPercentage;
                Debug.Log($"Setting health bar slider to: {healthPercentage} (was: {oldValue})");
                
                // Keep the fill image red
                Image fillImage = healthBarSlider.fillRect.GetComponent<Image>();
                if (fillImage != null && fillImage.color != barColor)
                {
                    fillImage.color = barColor;
                }
            }
            else
            {
                Debug.LogError("ERROR: healthBarSlider is null when trying to update!");
            }
            
            // Update health text if available
            if (healthText != null)
            {
                healthText.text = $"{Mathf.Ceil(currentHealth)} / {maxHealth}";
            }
        }
        else
        {
            Debug.LogError("ERROR: playerHealth is null when trying to update UI!");
        }
    }
}