using UnityEngine;
using System.Collections.Generic;

// Enum for different item rarities
public enum ItemRarity
{
    Common,
    Rare,
    Legendary,
    Unique
}

// Enum for different item types
public enum ItemType
{
    Weapon,
    Armor,
    Accessory,
    Consumable
}

// Enum for weapon types - can be expanded as needed
public enum WeaponType
{
    Sword,
    Axe,
    Mace,
    Dagger,
    Bow,
    Staff,
    Wand
}

// Base class for all items in the game
[CreateAssetMenu(fileName = "New Item", menuName = "Items/Basic Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite icon;
    public GameObject prefab; // The 3D model when dropped in the world
    
    [Header("Item Properties")]
    public ItemType type;
    public ItemRarity rarity;
    public bool isStackable;
    public int maxStackSize = 1;
    
    [Header("Item Stats")]
    public List<ItemStat> stats = new List<ItemStat>();
    
    // Get the color associated with this item's rarity
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return Color.white;
            case ItemRarity.Rare:
                return new Color(0f, 0.5f, 1f); // Blue
            case ItemRarity.Legendary:
                return new Color(0.5f, 0f, 0.5f); // Purple
            case ItemRarity.Unique:
                return new Color(1f, 0.5f, 0f); // Orange
            default:
                return Color.white;
        }
    }
    
    // Get a formatted display string for this item
    public string GetDisplayName()
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}>{itemName}</color>";
    }
    
    // Generate a tooltip for this item
    public string GetTooltip()
    {
        string tooltip = GetDisplayName() + "\n";
        tooltip += $"<color=#999999>{description}</color>\n";
        tooltip += $"<color=#CCCCCC>Type: {type}</color>\n";
        tooltip += $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}>{rarity}</color>\n\n";
        
        foreach (ItemStat stat in stats)
        {
            tooltip += $"{stat.statName}: {stat.value}\n";
        }
        
        return tooltip;
    }
    
    // Clone this item (for instantiating new item objects)
    public virtual Item Clone()
    {
        Item newItem = CreateInstance<Item>();
        newItem.itemName = this.itemName;
        newItem.description = this.description;
        newItem.icon = this.icon;
        newItem.prefab = this.prefab;
        newItem.type = this.type;
        newItem.rarity = this.rarity;
        newItem.isStackable = this.isStackable;
        newItem.maxStackSize = this.maxStackSize;
        
        // Clone the stats
        foreach (ItemStat stat in this.stats)
        {
            newItem.stats.Add(new ItemStat(stat.statName, stat.value));
        }
        
        return newItem;
    }
}

// Class to represent a single stat on an item
[System.Serializable]
public class ItemStat
{
    public string statName;
    public float value;
    
    public ItemStat(string name, float value)
    {
        this.statName = name;
        this.value = value;
    }
}