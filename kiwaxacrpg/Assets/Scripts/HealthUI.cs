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
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        }
        
        // Try to get player controller if available
        if (playerHealth != null)
        {
            playerController = playerHealth.GetComponent<PlayerController>();
        }
        
        // Check UI references
        if (healthBarSlider != null)
        {
            // Set the health bar color to red
            Image fillImage = healthBarSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = barColor;
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
                }
                catch (System.Exception) {
                    // Fall back to PlayerHealth if PlayerController fails
                    currentHealth = playerHealth.GetCurrentHealth();
                    maxHealth = playerHealth.GetMaxHealth();
                }
            }
            else
            {
                // Get values directly from PlayerHealth
                currentHealth = playerHealth.GetCurrentHealth();
                maxHealth = playerHealth.GetMaxHealth();
            }
            
            // Prevent division by zero
            if (maxHealth <= 0)
            {
                maxHealth = 100f; // Use default to avoid errors
            }
            
            // Calculate health percentage
            float healthPercentage = currentHealth / maxHealth;
            
            // Update the health bar slider if available
            if (healthBarSlider != null)
            {
                healthBarSlider.value = healthPercentage;
                
                // Keep the fill image red
                Image fillImage = healthBarSlider.fillRect.GetComponent<Image>();
                if (fillImage != null && fillImage.color != barColor)
                {
                    fillImage.color = barColor;
                }
            }
            
            // Update health text if available
            if (healthText != null)
            {
                healthText.text = $"{Mathf.Ceil(currentHealth)} / {maxHealth}";
            }
        }
    }
}