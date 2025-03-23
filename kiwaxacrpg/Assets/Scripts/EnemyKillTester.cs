using UnityEngine;
using System.Collections.Generic;

// Utility script to test killing enemies and item drops
// Attach this to your player character
public class EnemyKillTester : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("Key to press to kill the nearest enemy")]
    public KeyCode killNearestEnemyKey = KeyCode.K;
    
    [Tooltip("Maximum range to find enemies")]
    public float maxRange = 20f;
    
    [Tooltip("Force instant kill regardless of enemy health")]
    public bool forceInstantKill = true;
    
    [Tooltip("Damage to deal if not using instant kill")]
    public float damageAmount = 100f;

    // Update is called once per frame
    void Update()
    {
        // Check for key press to kill nearest enemy
        if (Input.GetKeyDown(killNearestEnemyKey))
        {
            KillNearestEnemy();
        }
    }
    
    // Find and kill the nearest enemy
    private void KillNearestEnemy()
    {
        // Find all enemies
        BasicEnemy[] enemies = FindObjectsByType<BasicEnemy>(FindObjectsSortMode.None);
        
        if (enemies.Length == 0)
        {
            Debug.Log("No enemies found in the scene.");
            return;
        }
        
        // Find the closest enemy
        BasicEnemy closestEnemy = null;
        float closestDistance = float.MaxValue;
        
        foreach (BasicEnemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance && distance <= maxRange)
            {
                closestEnemy = enemy;
                closestDistance = distance;
            }
        }
        
        // Kill the closest enemy if found
        if (closestEnemy != null)
        {
            Debug.Log($"Killing enemy at distance {closestDistance}m");
            
            if (forceInstantKill)
            {
                // Damage the enemy with a very high value to ensure it dies
                closestEnemy.TakeDamage(10000f);
            }
            else
            {
                // Apply the configured damage amount
                closestEnemy.TakeDamage(damageAmount);
            }
        }
        else
        {
            Debug.Log("No enemies within range.");
        }
    }
    
    // Test method to generate a random item directly at player position
    public void SpawnRandomItemAtPlayer()
    {
        if (WorldItemFactory.Instance != null)
        {
            // Spawn slightly in front of player
            Vector3 spawnPos = transform.position + transform.forward * 2f;
            WorldItemFactory.Instance.CreateRandomItem(spawnPos);
            Debug.Log("Created random item in front of player");
        }
        else
        {
            Debug.LogWarning("WorldItemFactory not found in scene");
        }
    }
    
    // Test method to create items of different rarities for testing
    public void SpawnItemSet()
    {
        if (WorldItemFactory.Instance == null || ItemDatabase.Instance == null)
        {
            Debug.LogWarning("Required components not found in scene");
            return;
        }
        
        // Spawn a set of items in front of the player
        Vector3 basePos = transform.position + transform.forward * 3f;
        float spacing = 1.5f;
        
        // Common
        WeaponItem common = ItemDatabase.Instance.GenerateRandomWeapon(WeaponType.Axe, ItemRarity.Common);
        WorldItemFactory.Instance.CreateWorldItem(common, basePos + Vector3.right * -spacing * 1.5f);
        
        // Rare
        WeaponItem rare = ItemDatabase.Instance.GenerateRandomWeapon(WeaponType.Axe, ItemRarity.Rare);
        WorldItemFactory.Instance.CreateWorldItem(rare, basePos + Vector3.right * -spacing * 0.5f);
        
        // Legendary
        WeaponItem legendary = ItemDatabase.Instance.GenerateRandomWeapon(WeaponType.Axe, ItemRarity.Legendary);
        WorldItemFactory.Instance.CreateWorldItem(legendary, basePos + Vector3.right * spacing * 0.5f);
        
        // Unique
        WeaponItem unique = ItemDatabase.Instance.GenerateRandomWeapon(WeaponType.Axe, ItemRarity.Unique);
        WorldItemFactory.Instance.CreateWorldItem(unique, basePos + Vector3.right * spacing * 1.5f);
        
        Debug.Log("Created a set of items with different rarities");
    }
}