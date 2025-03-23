using UnityEngine;

// Extension of the base Item class specifically for weapons
[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponItem : Item
{
    [Header("Weapon Properties")]
    public WeaponType weaponType;
    public float baseDamage;
    public float attackSpeed;
    public float attackRange;
    
    // Additional properties that could be specific to weapons
    public bool isTwoHanded;
    public GameObject weaponPrefab; // Separate prefab for equipped weapon
    public ParticleSystem hitEffect; // Optional effect when hitting enemies
    
    // Calculate DPS (Damage Per Second)
    public float CalculateDPS()
    {
        return baseDamage * attackSpeed;
    }
    
    // Generate weapon stats based on rarity
    public void GenerateWeaponStats(WeaponType type, ItemRarity rarity)
    {
        // Set the weapon type
        this.weaponType = type;
        this.type = ItemType.Weapon;
        this.rarity = rarity;
        
        // Base stats depend on weapon type
        SetBaseStats(type);
        
        // Apply rarity multipliers
        ApplyRarityMultipliers(rarity);
        
        // Update the stats list for UI display
        UpdateStatsList();
    }
    
    // Set base stats according to weapon type
    private void SetBaseStats(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                baseDamage = 10f;
                attackSpeed = 1.2f;
                attackRange = 2f;
                isTwoHanded = false;
                break;
                
            case WeaponType.Axe:
                baseDamage = 15f;
                attackSpeed = 0.8f;
                attackRange = 2f;
                isTwoHanded = false;
                break;
                
            case WeaponType.Mace:
                baseDamage = 12f;
                attackSpeed = 0.7f;
                attackRange = 2f;
                isTwoHanded = false;
                break;
                
            case WeaponType.Dagger:
                baseDamage = 6f;
                attackSpeed = 1.8f;
                attackRange = 1.5f;
                isTwoHanded = false;
                break;
                
            case WeaponType.Bow:
                baseDamage = 8f;
                attackSpeed = 1.0f;
                attackRange = 15f;
                isTwoHanded = true;
                break;
                
            case WeaponType.Staff:
                baseDamage = 14f;
                attackSpeed = 0.6f;
                attackRange = 10f;
                isTwoHanded = true;
                break;
                
            case WeaponType.Wand:
                baseDamage = 7f;
                attackSpeed = 1.5f;
                attackRange = 8f;
                isTwoHanded = false;
                break;
        }
    }
    
    // Apply stat multipliers based on item rarity
    private void ApplyRarityMultipliers(ItemRarity rarity)
    {
        float rarityMultiplier = 1.0f;
        
        switch (rarity)
        {
            case ItemRarity.Common:
                rarityMultiplier = 1.0f;
                // Add random small variation
                rarityMultiplier += Random.Range(-0.1f, 0.1f);
                break;
                
            case ItemRarity.Rare:
                rarityMultiplier = 1.5f;
                // Add random variation
                rarityMultiplier += Random.Range(-0.1f, 0.2f);
                break;
                
            case ItemRarity.Legendary:
                rarityMultiplier = 2.25f;
                // Add random variation
                rarityMultiplier += Random.Range(0f, 0.25f);
                break;
                
            case ItemRarity.Unique:
                rarityMultiplier = 3.0f;
                // Add random variation
                rarityMultiplier += Random.Range(0.1f, 0.5f);
                break;
        }
        
        // Apply the multiplier to the stats
        baseDamage *= rarityMultiplier;
        attackSpeed *= 1 + (rarityMultiplier - 1) * 0.5f; // Smaller boost to attack speed
    }
    
    // Update the stats list for UI display
    private void UpdateStatsList()
    {
        // Clear existing stats
        stats.Clear();
        
        // Add weapon-specific stats
        stats.Add(new ItemStat("Damage", baseDamage));
        stats.Add(new ItemStat("Attack Speed", attackSpeed));
        stats.Add(new ItemStat("Range", attackRange));
        stats.Add(new ItemStat("DPS", CalculateDPS()));
        
        // Add two-handed info
        if (isTwoHanded)
        {
            stats.Add(new ItemStat("Requires Two Hands", 1));
        }
    }
    
    // Override the clone method to include weapon-specific properties
    public override Item Clone()
    {
        WeaponItem newWeapon = CreateInstance<WeaponItem>();
        
        // Copy base item properties
        newWeapon.itemName = this.itemName;
        newWeapon.description = this.description;
        newWeapon.icon = this.icon;
        newWeapon.prefab = this.prefab;
        newWeapon.type = this.type;
        newWeapon.rarity = this.rarity;
        newWeapon.isStackable = this.isStackable;
        newWeapon.maxStackSize = this.maxStackSize;
        
        // Copy weapon-specific properties
        newWeapon.weaponType = this.weaponType;
        newWeapon.baseDamage = this.baseDamage;
        newWeapon.attackSpeed = this.attackSpeed;
        newWeapon.attackRange = this.attackRange;
        newWeapon.isTwoHanded = this.isTwoHanded;
        newWeapon.weaponPrefab = this.weaponPrefab;
        newWeapon.hitEffect = this.hitEffect;
        
        // Clone the stats
        foreach (ItemStat stat in this.stats)
        {
            newWeapon.stats.Add(new ItemStat(stat.statName, stat.value));
        }
        
        return newWeapon;
    }
    
    // Get a more detailed tooltip for weapons
    public new string GetTooltip()
    {
        string tooltip = GetDisplayName() + "\n";
        tooltip += $"<color=#999999>{description}</color>\n";
        tooltip += $"<color=#CCCCCC>{weaponType} ({type})</color>\n";
        tooltip += $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}>{rarity}</color>\n\n";
        
        // Add weapon-specific information
        tooltip += $"Damage: {baseDamage:F1}\n";
        tooltip += $"Attack Speed: {attackSpeed:F2}/sec\n";
        tooltip += $"Range: {attackRange:F1}m\n";
        tooltip += $"DPS: {CalculateDPS():F1}\n";
        
        // Extra info for two-handed weapons
        if (isTwoHanded)
        {
            tooltip += "\nRequires Two Hands\n";
        }
        
        return tooltip;
    }
}