using UnityEngine;
using System.Collections.Generic;

// Enhanced WarriorClass.cs
// This script extends the enhanced PlayerController with Warrior-specific functionality
// Compatible with the fixed camera and WASD movement system

public class WarriorClass : PlayerController
{
    [Header("Warrior-Specific Settings")]
    [Tooltip("Warrior's melee attack damage")]
    public float attackDamage = 25f;
    
    [Tooltip("Warrior's attack range")]
    public float attackRange = 2f;
    
    [Tooltip("Cooldown between attacks in seconds")]
    public float attackCooldown = 1.5f;
    
    [Tooltip("Layer mask for enemies that can be damaged")]
    public LayerMask enemyLayerMask = -1; // Default to "Everything"
    
    [Header("Visual Feedback")]
    [Tooltip("Optional attack effect prefab")]
    public GameObject attackEffectPrefab;
    
    [Tooltip("Duration of attack effect in seconds")]
    public float attackEffectDuration = 0.5f;
    
    // Track cooldown time
    private float attackCooldownTimer = 0f;
    
    // List of abilities (could be references to ScriptableObjects for more flexibility)
    private List<AbilityBase> warriorAbilities = new List<AbilityBase>();
    
    // UI elements to show ability cooldowns
    [Header("UI References")]
    [Tooltip("Optional UI element to display attack cooldown")]
    public UnityEngine.UI.Image attackCooldownUI;
    
    // Camera offset from player (fixed)
    [Header("Fixed Camera Settings")]
    [Tooltip("Camera offset from player position")]
    public Vector3 fixedCameraOffset = new Vector3(0, 10, -10);
    
    [Tooltip("Fixed camera rotation (in Euler angles)")]
    public Vector3 cameraRotation = new Vector3(45, 0, 0);
    
    [Tooltip("Enable camera follow without rotation")]
    public bool useFixedCameraFollow = true;
    
    // Override the initialization
    protected override void Start()
    {
        // Find camera if not set
        if (cameraTransform == null)
        {
            // Try to find camera in children
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
            {
                cameraTransform = childCamera.transform;
            }
            else
            {
                // Try to find main camera
                cameraTransform = Camera.main.transform;
            }
        }
        
        // Detach camera from player if it's a child - do this BEFORE calling base.Start()
        if (cameraTransform != null && cameraTransform.parent == transform)
        {
            // Detach from player
            cameraTransform.parent = null;
        }
        
        // Set initial camera position and rotation
        if (useFixedCameraFollow && cameraTransform != null)
        {
            // Set camera position relative to player
            cameraTransform.position = transform.position + fixedCameraOffset;
            
            // Set fixed camera rotation
            cameraTransform.rotation = Quaternion.Euler(cameraRotation);
        }
        
        // Now call base class Start for other initialization
        base.Start();
        
        // Add warrior-specific initialization
        InitializeWarriorStats();
        
        // Example of how you could load abilities
        LoadWarriorAbilities();
    }
    
    // Initialize warrior-specific stats
    private void InitializeWarriorStats()
    {
        // Override some base stats for warrior
        SetStat("Health", 500f); // Warriors have more health
        SetStat("MaxHealth", 500f);
        SetStat("Mana", 30f);    // Warriors have less mana
        SetStat("Strength", 15f); // Warriors have more strength
        
        // Add warrior-specific stats
        SetStat("ArmorClass", 20f);
        SetStat("BlockChance", 15f);
        
        // Add fury resource for warrior-specific abilities
        SetStat("MaxFury", 100f);
        SetStat("CurrentFury", 0f);
    }
    
    // Load warrior abilities - could be loaded from ScriptableObjects or other data source
    private void LoadWarriorAbilities()
    {
        // Example of ability initialization - in a real game these might be loaded dynamically
        // or created from ScriptableObjects for better flexibility
        
        // This is just a placeholder - in a real implementation you'd add actual ability objects
        // warriorAbilities.Add(new CleavingStrike(this));
        // warriorAbilities.Add(new WarCry(this));
    }
    
    // Override Update to add warrior-specific behavior
    protected override void Update()
    {
        // Call base Update first to handle movement
        base.Update();
        
        // Handle fixed camera follow (overrides the camera behavior in base class)
        if (useFixedCameraFollow && cameraTransform != null)
        {
            // Update camera position to follow player while maintaining fixed offset
            cameraTransform.position = transform.position + fixedCameraOffset;
            
            // Keep fixed rotation
            cameraTransform.rotation = Quaternion.Euler(cameraRotation);
        }
        
        // Update ability cooldowns
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
            
            // Update UI cooldown if available
            if (attackCooldownUI != null)
            {
                attackCooldownUI.fillAmount = attackCooldownTimer / attackCooldown;
            }
        }
        
        // Handle warrior attack input
        if (Input.GetMouseButtonDown(0) && attackCooldownTimer <= 0)
        {
            PerformMeleeAttack();
        }
        
        // Example: Generate fury over time or through movement
        if (movementDirection.magnitude > 0.1f)
        {
            // Generate fury while moving
            float currentFury = GetStat("CurrentFury");
            float maxFury = GetStat("MaxFury");
            
            // Increase fury by a small amount
            currentFury = Mathf.Min(currentFury + Time.deltaTime * 5f, maxFury);
            SetStat("CurrentFury", currentFury);
        }
        
        // Example: Ability 1 on key press
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseAbility(0); // Use first ability
        }
        
        // Example: Ability 2 on key press
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseAbility(1); // Use second ability
        }
        
        // Example: Ability 3 on key press
        if (Input.GetKeyDown(KeyCode.R))
        {
            UseAbility(2); // Use third ability
        }
    }
    
    // Perform basic melee attack
    private void PerformMeleeAttack()
    {
        // Start attack cooldown
        attackCooldownTimer = attackCooldown;
        
        // Calculate attack direction - use forward direction of player
        Vector3 attackDirection = transform.forward;
        
        // Example implementation of a melee attack using a spherecast for better detection
        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position + Vector3.up, // Start slightly raised to handle ground detection better
            attackRange / 2, // Sphere radius (half of the attack range for a good compromise)
            attackDirection, // Direction of attack
            attackRange, // Distance to check
            enemyLayerMask // Layer mask for enemies
        );
        
        // Calculate final damage based on stats
        float finalDamage = attackDamage * (1 + GetStat("Strength") / 100f);
        
        // Process all hits
        foreach (RaycastHit hit in hits)
        {
            // Skip self-collision
            if (hit.collider.transform == transform)
                continue;
                
            // Check if we hit an enemy
            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // Apply damage to enemy
                enemy.TakeDamage(finalDamage);
                
                // Generate fury on successful hit
                float currentFury = GetStat("CurrentFury");
                float maxFury = GetStat("MaxFury");
                currentFury = Mathf.Min(currentFury + 10f, maxFury);
                SetStat("CurrentFury", currentFury);
            }
        }
        
        // Spawn attack effect if available
        if (attackEffectPrefab != null)
        {
            // Instantiate effect at position slightly in front of player
            GameObject effect = Instantiate(
                attackEffectPrefab, 
                transform.position + transform.forward * attackRange / 2 + Vector3.up, 
                Quaternion.LookRotation(transform.forward)
            );
            
            // Destroy after duration
            Destroy(effect, attackEffectDuration);
        }
    }
    
    // Use a specific warrior ability
    private void UseAbility(int abilityIndex)
    {
        // Check if ability exists and can be used
        if (abilityIndex < warriorAbilities.Count)
        {
            AbilityBase ability = warriorAbilities[abilityIndex];
            
            // Check if we have enough resources
            float resourceCost = ability.GetResourceCost();
            string resourceType = ability.GetResourceType();
            
            if (GetStat(resourceType) >= resourceCost)
            {
                // Use the ability
                ability.Use();
                
                // Consume resource
                SetStat(resourceType, GetStat(resourceType) - resourceCost);
            }
        }
    }
}

// This is a base class for abilities - would be in a separate file in a real project
public abstract class AbilityBase
{
    protected PlayerController owner;
    protected float cooldown;
    protected float resourceCost;
    protected string resourceType; // "Mana", "Fury", "Energy", etc.
    protected string abilityName;
    
    public AbilityBase(PlayerController owner, float cooldown, float resourceCost, string resourceType, string name)
    {
        this.owner = owner;
        this.cooldown = cooldown;
        this.resourceCost = resourceCost;
        this.resourceType = resourceType;
        this.abilityName = name;
    }
    
    public abstract void Use();
    
    public float GetResourceCost() { return resourceCost; }
    public string GetResourceType() { return resourceType; }
    public string GetAbilityName() { return abilityName; }
    public float GetCooldown() { return cooldown; }
}

// This is a placeholder for EnemyBase - would be in a separate file in a real project
public abstract class EnemyBase : MonoBehaviour
{
    public abstract void TakeDamage(float damage);
}