using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Main inventory system that stores and manages the player's items
/// </summary>
public class Inventory : MonoBehaviour
{
    // Static instance for easy access from other scripts
    public static Inventory Instance { get; private set; }
    
    // Maximum number of items the inventory can hold
    public int inventorySize = 20;
    
    // Maximum number of equipped items (Weapon, Armor, Accessory1, Accessory2)
    public int equipSlotCount = 10;
    
    // Event triggered when inventory changes
    public event Action OnInventoryChanged;
    
    // Event triggered when equipment changes
    public event Action OnEquipmentChanged;
    
    // Item slots for the main inventory
    private Item[] inventorySlots;
    
    // Item slots for equipped items
    private Item[] equippedItems;
    
    // Dictionary to track slots by item for quick lookup
    private Dictionary<Item, int> itemSlotMap = new Dictionary<Item, int>();
    
    // Initialize the inventory
    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Initialize inventory arrays
        inventorySlots = new Item[inventorySize];
        equippedItems = new Item[equipSlotCount];
        
        // Don't destroy the inventory between scenes
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Add an item to the inventory
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns>True if the item was added successfully, false if inventory is full</returns>
    public bool AddItem(Item item)
    {
        if (item == null) return false;
        
        Debug.Log($"Attempting to add {item.itemName} to inventory");
        
        // Check if we can stack this item with an existing one
        if (item.isStackable)
        {
            // Look for an existing stack of this item
            for (int i = 0; i < inventorySize; i++)
            {
                Item existingItem = inventorySlots[i];
                
                // If we find the same stackable item with room in the stack
                if (existingItem != null && 
                    existingItem.itemName == item.itemName && 
                    existingItem.type == item.type)
                {
                    // Implement stacking logic here when you add stack count property
                    // For now, we'll just log it
                    Debug.Log($"Found existing stack of {item.itemName}");
                    
                    // Notify listeners that inventory has changed
                    if (OnInventoryChanged != null)
                        OnInventoryChanged.Invoke();
                    return true;
                }
            }
        }
        
        // Find an empty slot
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventorySlots[i] == null)
            {
                // We found an empty slot, add the item here
                inventorySlots[i] = item;
                itemSlotMap[item] = i;
                
                Debug.Log($"Added {item.itemName} to inventory slot {i}");
                
                // Notify listeners that inventory has changed
                if (OnInventoryChanged != null)
                    OnInventoryChanged.Invoke();
                return true;
            }
        }
        
        // If we get here, the inventory is full
        Debug.LogWarning("Inventory is full, couldn't add item: " + item.itemName);
        return false;
    }
    
    /// <summary>
    /// Remove an item from the inventory
    /// </summary>
    /// <param name="item">The item to remove</param>
    /// <returns>True if item was removed, false if not found</returns>
    public bool RemoveItem(Item item)
    {
        if (item == null) return false;
        
        // Check if we know which slot this item is in
        if (itemSlotMap.ContainsKey(item))
        {
            int slot = itemSlotMap[item];
            
            // Verify the item is still in this slot
            if (inventorySlots[slot] == item)
            {
                inventorySlots[slot] = null;
                itemSlotMap.Remove(item);
                
                Debug.Log($"Removed {item.itemName} from inventory slot {slot}");
                
                // Notify listeners that inventory has changed
                if (OnInventoryChanged != null)
                    OnInventoryChanged.Invoke();
                return true;
            }
        }
        
        // If we don't have it mapped or the mapping is wrong, search all slots
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventorySlots[i] == item)
            {
                inventorySlots[i] = null;
                
                // Update the mapping if it exists
                if (itemSlotMap.ContainsKey(item))
                {
                    itemSlotMap.Remove(item);
                }
                
                Debug.Log($"Removed {item.itemName} from inventory slot {i}");
                
                // Notify listeners that inventory has changed
                if (OnInventoryChanged != null)
                    OnInventoryChanged.Invoke();
                return true;
            }
        }
        
        // Item not found in inventory
        Debug.LogWarning("Item not found in inventory: " + item.itemName);
        return false;
    }
    
    /// <summary>
    /// Equip an item from the inventory
    /// </summary>
    /// <param name="item">The item to equip</param>
    /// <param name="slotIndex">The equipment slot to use (optional)</param>
    /// <returns>True if equipped successfully</returns>
    public bool EquipItem(Item item, int slotIndex = -1)
    {
        if (item == null) return false;
        
        // Determine the appropriate equipment slot based on item type
        if (slotIndex < 0)
        {
            // Auto-assign slot based on item type
            switch (item.type)
            {
                case ItemType.Weapon:
                    slotIndex = 0; // Weapon slot
                    break;
                case ItemType.Armor:
                    slotIndex = 1; // Armor slot
                    break;
                case ItemType.Accessory:
                    // Find the first empty accessory slot (2 or 3)
                    if (equippedItems[2] == null)
                        slotIndex = 2;
                    else if (equippedItems[3] == null)
                        slotIndex = 3;
                    else
                        slotIndex = 2; // Replace first accessory if both are full
                    break;
                default:
                    Debug.LogWarning($"Cannot equip item of type {item.type}: {item.itemName}");
                    return false;
            }
        }
        
        // Validate slot index
        if (slotIndex < 0 || slotIndex >= equipSlotCount)
        {
            Debug.LogError($"Invalid equipment slot index: {slotIndex}");
            return false;
        }
        
        // If there's already an item in this slot, unequip it first
        if (equippedItems[slotIndex] != null)
        {
            UnequipItem(slotIndex);
        }
        
        // Remove the item from inventory first
        if (!RemoveItem(item))
        {
            Debug.LogWarning($"Could not remove item from inventory to equip: {item.itemName}");
            return false;
        }
        
        // Equip the item
        equippedItems[slotIndex] = item;
        
        Debug.Log($"Equipped {item.itemName} in slot {slotIndex}");
        
        // Notify listeners that equipment has changed
        if (OnEquipmentChanged != null)
            OnEquipmentChanged.Invoke();
        
        return true;
    }
    
    /// <summary>
    /// Unequip an item and put it back in the inventory
    /// </summary>
    /// <param name="slotIndex">The equipment slot to unequip</param>
    /// <returns>True if unequipped successfully</returns>
    public bool UnequipItem(int slotIndex)
    {
        // Validate slot index
        if (slotIndex < 0 || slotIndex >= equipSlotCount)
        {
            Debug.LogError($"Invalid equipment slot index: {slotIndex}");
            return false;
        }
        
        // Get the equipped item
        Item itemToUnequip = equippedItems[slotIndex];
        
        // Check if there's an item to unequip
        if (itemToUnequip == null)
        {
            Debug.LogWarning($"No item equipped in slot {slotIndex}");
            return false;
        }
        
        // Try to add the item to inventory
        if (!AddItem(itemToUnequip))
        {
            Debug.LogWarning($"Inventory full, cannot unequip {itemToUnequip.itemName}");
            return false;
        }
        
        // Clear the equipment slot
        equippedItems[slotIndex] = null;
        
        Debug.Log($"Unequipped {itemToUnequip.itemName} from slot {slotIndex}");
        
        // Notify listeners that equipment has changed
        if (OnEquipmentChanged != null)
            OnEquipmentChanged.Invoke();
        
        return true;
    }
    
    /// <summary>
    /// Drop an item from the inventory into the world
    /// </summary>
    /// <param name="item">The item to drop</param>
    /// <param name="dropPosition">Position to drop the item (defaults to player position)</param>
    /// <returns>True if dropped successfully</returns>
    public bool DropItem(Item item, Vector3? dropPosition = null)
    {
        if (item == null) return false;
        
        // Remove the item from inventory
        if (!RemoveItem(item))
        {
            Debug.LogWarning($"Could not remove item from inventory to drop: {item.itemName}");
            return false;
        }
        
        // Determine drop position if not specified
        Vector3 position = dropPosition ?? GetDropPosition();
        
        // Spawn the item in the world (if WorldItemFactory exists)
        if (WorldItemFactory.Instance != null)
        {
            WorldItemFactory.Instance.CreateWorldItem(item, position);
            Debug.Log($"Dropped {item.itemName} into the world at {position}");
            return true;
        }
        else
        {
            Debug.LogError("WorldItemFactory not found, cannot drop item");
            
            // Add the item back to inventory since we couldn't drop it
            AddItem(item);
            return false;
        }
    }
    
    /// <summary>
    /// Get a position to drop items, typically in front of the player
    /// </summary>
    private Vector3 GetDropPosition()
    {
        // Try to find the player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Drop 1.5 units in front of the player
            return player.transform.position + player.transform.forward * 1.5f;
        }
        
        // If no player found, use this transform's position
        return transform.position;
    }
    
    /// <summary>
    /// Check if the inventory contains a specific item
    /// </summary>
    /// <param name="item">The item to check for</param>
    /// <returns>True if the item is in the inventory</returns>
    public bool HasItem(Item item)
    {
        if (item == null) return false;
        
        // Check the mapping first for efficiency
        if (itemSlotMap.ContainsKey(item))
        {
            return true;
        }
        
        // If not in mapping, check all slots (slower)
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventorySlots[i] == item)
            {
                // Add to mapping for future lookups
                itemSlotMap[item] = i;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Get all items in the inventory
    /// </summary>
    /// <returns>Array of items (can contain null entries for empty slots)</returns>
    public Item[] GetAllItems()
    {
        return inventorySlots;
    }
    
    /// <summary>
    /// Get all equipped items
    /// </summary>
    /// <returns>Array of equipped items (can contain null entries for empty slots)</returns>
    public Item[] GetEquippedItems()
    {
        return equippedItems;
    }
    
    /// <summary>
    /// Get item at a specific inventory slot
    /// </summary>
    /// <param name="slotIndex">The slot to check</param>
    /// <returns>The item in that slot, or null if empty</returns>
    public Item GetItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
        {
            Debug.LogError($"Invalid inventory slot index: {slotIndex}");
            return null;
        }
        
        return inventorySlots[slotIndex];
    }
    
    /// <summary>
    /// Get equipped item at a specific slot
    /// </summary>
    /// <param name="slotIndex">The equipment slot to check</param>
    /// <returns>The equipped item, or null if empty</returns>
    public Item GetEquippedItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equipSlotCount)
        {
            Debug.LogError($"Invalid equipment slot index: {slotIndex}");
            return null;
        }
        
        return equippedItems[slotIndex];
    }
    
    /// <summary>
    /// Get the count of items in the inventory (non-empty slots)
    /// </summary>
    /// <returns>Number of items in inventory</returns>
    public int GetItemCount()
    {
        int count = 0;
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventorySlots[i] != null)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Check if the inventory is full
    /// </summary>
    /// <returns>True if all slots are filled</returns>
    public bool IsFull()
    {
        return GetItemCount() >= inventorySize;
    }
    
    /// <summary>
    /// Clear the entire inventory (remove all items)
    /// </summary>
    public void ClearInventory()
    {
        // Clear all inventory slots
        for (int i = 0; i < inventorySize; i++)
        {
            inventorySlots[i] = null;
        }
        
        // Clear mapping
        itemSlotMap.Clear();
        
        Debug.Log("Inventory cleared");

        // Notify listeners that inventory has changed
        OnInventoryChanged?.Invoke();
    }
}