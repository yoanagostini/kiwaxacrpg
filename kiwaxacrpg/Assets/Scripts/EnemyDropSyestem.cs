using UnityEngine;

// This script extends the BasicEnemy class to add item drop functionality
public class EnemyDropSystem : MonoBehaviour
{
    [Header("Drop Settings")]
    [Tooltip("Chance that this enemy will drop an item (0-1)")]
    [Range(0f, 1f)]
    public float dropChance = 0.3f;
    
    [Tooltip("Minimum number of items that can drop")]
    public int minDrops = 0;
    
    [Tooltip("Maximum number of items that can drop")]
    public int maxDrops = 1;
    
    [Header("Drop Positioning")]
    [Tooltip("Spread items out in a radius when multiple drop")]
    public float dropRadius = 1f;
    
    [Tooltip("Height offset for dropping items")]
    public float dropHeight = 0.5f;
    
    [Header("Rarity Weights")]
    [Tooltip("Weight for Common items")]
    public float commonWeight = 70f;
    
    [Tooltip("Weight for Rare items")]
    public float rareWeight = 20f;
    
    [Tooltip("Weight for Legendary items")]
    public float legendaryWeight = 8f;
    
    [Tooltip("Weight for Unique items")]
    public float uniqueWeight = 2f;
    
    // Called when the enemy dies
    public void DropLoot(Vector3 dropPosition)
    {
        // Check if we should drop anything
        if (Random.value > dropChance)
        {
            // No drop this time
            return;
        }
        
        // Determine how many items to drop
        int dropCount = Random.Range(minDrops, maxDrops + 1);
        
        // No need to continue if dropCount is 0
        if (dropCount <= 0) return;
        
        // Make sure we have required references
        if (ItemDatabase.Instance == null || WorldItemFactory.Instance == null)
        {
            return;
        }
        
        // Drop the items
        for (int i = 0; i < dropCount; i++)
        {
            // Generate a random item
            ItemRarity rarity = GetRandomRarity();
            WeaponItem weapon = ItemDatabase.Instance.GenerateRandomWeapon(WeaponType.Axe, rarity);
            
            if (weapon != null)
            {
                // Calculate drop position
                Vector3 pos = CalculateDropPosition(dropPosition, i, dropCount);
                
                // Create the world item
                WorldItemFactory.Instance.CreateWorldItem(weapon, pos);
            }
        }
    }
    
    // Calculate a position to drop the item
    private Vector3 CalculateDropPosition(Vector3 basePosition, int index, int totalDrops)
    {
        // If only one item, drop it at the base position with height offset
        if (totalDrops == 1)
        {
            return new Vector3(basePosition.x, basePosition.y + dropHeight, basePosition.z);
        }
        
        // For multiple items, calculate positions in a circle
        float angle = (360f / totalDrops) * index;
        float radians = angle * Mathf.Deg2Rad;
        
        float x = basePosition.x + Mathf.Cos(radians) * dropRadius;
        float z = basePosition.z + Mathf.Sin(radians) * dropRadius;
        
        return new Vector3(x, basePosition.y + dropHeight, z);
    }
    
    // Get a random rarity based on weights
    private ItemRarity GetRandomRarity()
    {
        // Calculate total weight
        float totalWeight = commonWeight + rareWeight + legendaryWeight + uniqueWeight;
        
        // Get a random value
        float randomValue = Random.Range(0f, totalWeight);
        
        // Determine which rarity was selected
        if (randomValue < commonWeight)
        {
            return ItemRarity.Common;
        }
        else if (randomValue < commonWeight + rareWeight)
        {
            return ItemRarity.Rare;
        }
        else if (randomValue < commonWeight + rareWeight + legendaryWeight)
        {
            return ItemRarity.Legendary;
        }
        else
        {
            return ItemRarity.Unique;
        }
    }
}