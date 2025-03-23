using UnityEngine;

// Simple script to test weapon generation
public class WeaponTester : MonoBehaviour
{
    // Button keycodes for testing different rarity weapons
    [Header("Test Keys")]
    [Tooltip("Press this key to generate a Common axe")]
    public KeyCode commonWeaponKey = KeyCode.Alpha1;
    
    [Tooltip("Press this key to generate a Rare axe")]
    public KeyCode rareWeaponKey = KeyCode.Alpha2;
    
    [Tooltip("Press this key to generate a Legendary axe")]
    public KeyCode legendaryWeaponKey = KeyCode.Alpha3;
    
    [Tooltip("Press this key to generate a Unique axe")]
    public KeyCode uniqueWeaponKey = KeyCode.Alpha4;
    
    [Tooltip("Press this key to generate a random rarity axe")]
    public KeyCode randomWeaponKey = KeyCode.Space;
    
    // Update is called once per frame
    void Update()
    {
        // Check for key presses
        if (Input.GetKeyDown(commonWeaponKey))
        {
            GenerateWeapon(ItemRarity.Common);
        }
        else if (Input.GetKeyDown(rareWeaponKey))
        {
            GenerateWeapon(ItemRarity.Rare);
        }
        else if (Input.GetKeyDown(legendaryWeaponKey))
        {
            GenerateWeapon(ItemRarity.Legendary);
        }
        else if (Input.GetKeyDown(uniqueWeaponKey))
        {
            GenerateWeapon(ItemRarity.Unique);
        }
        else if (Input.GetKeyDown(randomWeaponKey))
        {
            GenerateRandomWeapon();
        }
    }
    
    // Generate a weapon with specific rarity
    void GenerateWeapon(ItemRarity rarity)
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("ItemDatabase instance not found! Make sure it exists in the scene.");
            return;
        }
        
        WeaponItem weapon = ItemDatabase.Instance.GenerateRandomWeapon(WeaponType.Axe, rarity);
        
        if (weapon != null)
        {
            Debug.Log($"Generated {rarity} axe: {weapon.GetDisplayName()}");
            Debug.Log(weapon.GetTooltip());
            
            // Here you would spawn the weapon in the world
            // For now, we're just logging its details
        }
    }
    
    // Generate a completely random weapon
    void GenerateRandomWeapon()
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("ItemDatabase instance not found! Make sure it exists in the scene.");
            return;
        }
        
        WeaponItem weapon = ItemDatabase.Instance.GenerateRandomWeapon();
        
        if (weapon != null)
        {
            Debug.Log($"Generated random axe: {weapon.GetDisplayName()}");
            Debug.Log(weapon.GetTooltip());
            
            // Here you would spawn the weapon in the world
            // For now, we're just logging its details
        }
    }
}