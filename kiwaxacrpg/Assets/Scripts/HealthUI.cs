using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HealthUI.cs
// This script manages the player's health UI display
// It should be attached to a UI canvas object

public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The fill image used for the health bar")]
    public Image healthBarFill;
    
    [Tooltip("Text to display current/max health values")]
    public Text healthText;
    
    [Header("Settings")]
    [Tooltip("Reference to the player's health component")]
    public PlayerHealth playerHealth;
    
    [Tooltip("Color of the health bar when health is high")]
    public Color healthyColor = new Color(0.2f, 0.8f, 0.2f);
    
    [Tooltip("Color of the health bar when health is low")]
    public Color criticalColor = new Color(0.8f, 0.2f, 0.2f);
    
    [Tooltip("Threshold at which health is considered low (percentage)")]
    [Range(0.0f, 1.0f)]
    public float criticalHealthThreshold = 0.3f;
    
    // Main player controller reference
    private PlayerController playerController;
    
    // Start is called before the first frame update
    void Start()
    {
        // If no player health is assigned, try to find it
        if (playerHealth == null)
        {
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
            
            if (playerHealth == null)
            {
                Debug.LogError("No PlayerHealth component found! Please assign it in the inspector.");
            }
        }
        
        // Try to get player controller if available
        if (playerHealth != null)
        {
            playerController = playerHealth.GetComponent<PlayerController>();
        }
        
        // Initialize the UI
        UpdateHealthUI();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Update the UI every frame to reflect current health
        // This could be optimized to only update when health changes
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
                currentHealth = playerController.GetStat("Health");
                maxHealth = playerController.GetStat("MaxHealth");
            }
            else
            {
                currentHealth = playerHealth.GetCurrentHealth();
                maxHealth = playerHealth.GetMaxHealth();
            }
            
            // Calculate health percentage
            float healthPercentage = currentHealth / maxHealth;
            
            // Update the health bar fill amount if available
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = healthPercentage;
                
                // Update color based on health percentage
                healthBarFill.color = Color.Lerp(criticalColor, healthyColor, 
                    Mathf.InverseLerp(0, criticalHealthThreshold * 2, healthPercentage));
            }
            
            // Update health text if available
            if (healthText != null)
            {
                healthText.text = $"{Mathf.Ceil(currentHealth)} / {maxHealth}";
            }
        }
    }
}