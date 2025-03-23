using UnityEngine;
using System.Collections.Generic;

// Database to manage all items in the game
// This uses the Singleton pattern for easy access
public class ItemDatabase : MonoBehaviour
{
    // Static instance that can be accessed from anywhere
    public static ItemDatabase Instance { get; private set; }
    
    // List of all items in the game
    [SerializeField]
    private List<Item> allItems = new List<Item>();
    
    // Dictionary for quick lookup by ID (optional, for more complex systems)
    private Dictionary<string, Item> itemDictionary = new Dictionary<string, Item>();
    
    // Weapon template references for generating random weapons
    [Header("Weapon Templates")]
    [SerializeField]
    private WeaponItem axeTemplate;
    
    // These fields are kept for future use but won't be accessed currently
    [SerializeField, HideInInspector]
    private WeaponItem swordTemplate;
    [SerializeField, HideInInspector]
    private WeaponItem maceTemplate;
    [SerializeField, HideInInspector]
    private WeaponItem daggerTemplate;
    [SerializeField, HideInInspector]
    private WeaponItem bowTemplate;
    [SerializeField, HideInInspector]
    private WeaponItem staffTemplate;
    [SerializeField, HideInInspector]
    private WeaponItem wandTemplate;
    
    // Arrays of prefixes and suffixes for generating random item names
    private string[] commonPrefixes = { "Basic", "Simple", "Crude", "Ordinary", "Plain" };
    private string[] rarePrefixes = { "Fine", "Quality", "Superior", "Crafted", "Sturdy" };
    private string[] legendaryPrefixes = { "Heroic", "Mythical", "Ancient", "Mighty", "Glorious" };
    private string[] uniquePrefixes = { "Godly", "Divine", "Celestial", "Infernal", "Eternal" };
    
    private string[] weaponSuffixes = { 
        "of Power", "of Strength", "of Agility", 
        "of the Bear", "of the Wolf", "of the Eagle",
        "of Havoc", "of Destruction", "of Ruin",
        "of the Warrior", "of the Hunter", "of the Mage"
    };
    
    // Initialize the database
    private void Awake()
    {
        // Implement singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize the dictionary for quick lookups
        foreach (Item item in allItems)
        {
            if (item != null)
            {
                itemDictionary[item.itemName] = item;
            }
        }
    }
    
    // Get an item by name
    public Item GetItem(string itemName)
    {
        if (itemDictionary.ContainsKey(itemName))
        {
            return itemDictionary[itemName].Clone();
        }
        
        Debug.LogWarning($"Item '{itemName}' not found in database!");
        return null;
    }
    
    // Generate a random weapon with specified type and rarity
    public WeaponItem GenerateRandomWeapon(WeaponType type, ItemRarity rarity)
    {
        // For now, we only support axes
        if (type != WeaponType.Axe)
        {
            Debug.LogWarning($"Currently only axe templates are implemented. Defaulting to axe.");
            type = WeaponType.Axe;
        }
        
        // Get the appropriate template based on weapon type
        WeaponItem template = GetWeaponTemplate(type);
        
        if (template == null)
        {
            Debug.LogError($"No template found for weapon type: {type}");
            return null;
        }
        
        // Clone the template
        WeaponItem newWeapon = (WeaponItem)template.Clone();
        
        // Generate the weapon stats based on type and rarity
        newWeapon.GenerateWeaponStats(type, rarity);
        
        // Generate a name for the weapon
        newWeapon.itemName = GenerateWeaponName(type, rarity);
        
        // Generate a simple description
        newWeapon.description = $"A {rarity.ToString().ToLower()} {type.ToString().ToLower()} with {newWeapon.baseDamage:F1} damage.";
        
        return newWeapon;
    }
    
    // Generate a completely random weapon with random type and rarity
    public WeaponItem GenerateRandomWeapon()
    {
        // For now, only generate axes
        WeaponType randomType = WeaponType.Axe;
        
        // Get a random rarity with weighted probability
        ItemRarity randomRarity = GetRandomRarityWithWeights();
        
        // Generate the weapon
        return GenerateRandomWeapon(randomType, randomRarity);
    }
    
    // Get random rarity based on weighted probabilities
    private ItemRarity GetRandomRarityWithWeights()
    {
        float random = Random.value;
        
        // 70% Common, 20% Rare, 8% Legendary, 2% Unique
        if (random < 0.7f)
            return ItemRarity.Common;
        else if (random < 0.9f)
            return ItemRarity.Rare;
        else if (random < 0.98f)
            return ItemRarity.Legendary;
        else
            return ItemRarity.Unique;
    }
    
    // Get the correct template for a weapon type
    private WeaponItem GetWeaponTemplate(WeaponType type)
    {
        // For now, we only have the axe template
        switch (type)
        {
            case WeaponType.Axe:
                return axeTemplate;
            
            // All other cases return the axe template for now
            default:
                Debug.LogWarning($"Template for {type} not implemented yet. Using Axe template instead.");
                return axeTemplate;
        }
    }
    
    // Generate a name for a weapon based on type and rarity
    private string GenerateWeaponName(WeaponType type, ItemRarity rarity)
    {
        string prefix = "";
        
        // Get a random prefix based on rarity
        switch (rarity)
        {
            case ItemRarity.Common:
                prefix = commonPrefixes[Random.Range(0, commonPrefixes.Length)];
                break;
            case ItemRarity.Rare:
                prefix = rarePrefixes[Random.Range(0, rarePrefixes.Length)];
                break;
            case ItemRarity.Legendary:
                prefix = legendaryPrefixes[Random.Range(0, legendaryPrefixes.Length)];
                break;
            case ItemRarity.Unique:
                prefix = uniquePrefixes[Random.Range(0, uniquePrefixes.Length)];
                break;
        }
        
        // For rare+ items, add a suffix
        string suffix = "";
        if (rarity >= ItemRarity.Rare)
        {
            suffix = " " + weaponSuffixes[Random.Range(0, weaponSuffixes.Length)];
        }
        
        // Combine everything
        return $"{prefix} {type.ToString()}{suffix}";
    }
    
    // Test function to generate and log info about a random weapon
    public void TestRandomWeaponGeneration()
    {
        WeaponItem weapon = GenerateRandomWeapon();
        Debug.Log($"Generated weapon: {weapon.GetDisplayName()}");
        Debug.Log(weapon.GetTooltip());
    }
}