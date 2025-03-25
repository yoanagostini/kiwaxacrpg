using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles the UI representation of the player's inventory
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent object containing inventory slot UI elements")]
    public Transform inventorySlotsParent;
    
    [Tooltip("Parent object containing equipment slot UI elements")]
    public Transform equipmentSlotsParent;
    
    [Tooltip("Prefab for inventory slot UI")]
    public GameObject inventorySlotPrefab;
    
    [Tooltip("Selected item name text")]
    public TextMeshProUGUI selectedItemName;
    
    [Tooltip("Selected item description text")]
    public TextMeshProUGUI selectedItemDescription;
    
    [Tooltip("Selected item stats panel")]
    public Transform itemStatsPanel;
    
    [Tooltip("Prefab for item stat display")]
    public GameObject itemStatPrefab;
    
    [Header("UI Controls")]
    [Tooltip("Button to equip selected item")]
    public Button equipButton;
    
    [Tooltip("Button to use selected item")]
    public Button useButton;
    
    [Tooltip("Button to drop selected item")]
    public Button dropButton;
    
    [Header("Settings")]
    [Tooltip("Key to toggle inventory visibility")]
    public KeyCode toggleInventoryKey = KeyCode.I;
    
    [Tooltip("Whether inventory starts visible")]
    public bool startVisible = false;
    
    // Reference to inventory system
    private Inventory inventory;
    
    // UI slot objects for inventory
    private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
    
    // UI slot objects for equipment
    private List<InventorySlotUI> equipmentSlots = new List<InventorySlotUI>();
    
    // Currently selected item/slot
    private InventorySlotUI selectedSlot;
    
    // Item stat UI elements for reuse
    private List<GameObject> statUIElements = new List<GameObject>();
    
    // Whether the inventory is currently visible
    private bool isVisible;
    
    // Initialize the UI
    private void Start()
    {
        // Find inventory if not set
        if (inventory == null)
        {
            inventory = Inventory.Instance;
            if (inventory == null)
            {
                Debug.LogError("Inventory system not found. Make sure it exists in the scene.");
                gameObject.SetActive(false);
                return;
            }
        }
        
        // Initialize UI components
        CreateInventorySlots();
        CreateEquipmentSlots();
        
        // Set up event listeners for inventory changes
        inventory.OnInventoryChanged += UpdateInventoryUI;
        inventory.OnEquipmentChanged += UpdateEquipmentUI;
        
        // Set up action buttons
        SetupButtons();
        
        // Set initial visibility
        SetInventoryVisible(startVisible);
        
        // Initial UI update
        UpdateInventoryUI();
        UpdateEquipmentUI();
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Check for toggle key press
        if (Input.GetKeyDown(toggleInventoryKey))
        {
            ToggleInventory();
        }
    }
    
    /// <summary>
    /// Create UI slots for the inventory
    /// </summary>
    private void CreateInventorySlots()
    {
        // Clear any existing slots
        foreach (Transform child in inventorySlotsParent)
        {
            Destroy(child.gameObject);
        }
        inventorySlots.Clear();
        
        // Create new slots based on inventory size
        for (int i = 0; i < inventory.inventorySize; i++)
        {
            GameObject slotObject = Instantiate(inventorySlotPrefab, inventorySlotsParent);
            slotObject.name = $"InventorySlot_{i}";
            
            InventorySlotUI slot = slotObject.GetComponent<InventorySlotUI>();
            if (slot == null)
            {
                slot = slotObject.AddComponent<InventorySlotUI>();
            }
            
            // Initialize slot
            slot.slotIndex = i;
            slot.isEquipSlot = false;
            
            // Add click event
            Button button = slotObject.GetComponent<Button>();
            if (button != null)
            {
                int index = i; // Store for lambda
                button.onClick.AddListener(() => OnSlotClicked(slot));
            }
            
            inventorySlots.Add(slot);
        }
    }
    
    /// <summary>
    /// Create UI slots for equipped items
    /// </summary>
    private void CreateEquipmentSlots()
    {
        // Clear any existing slots
        foreach (Transform child in equipmentSlotsParent)
        {
            Destroy(child.gameObject);
        }
        equipmentSlots.Clear();
        
        // Create slots for equipment (weapon, armor, accessories)
        string[] slotNames = { "Weapon", "Armor", "Accessory 1", "Accessory 2" };
        
        for (int i = 0; i < inventory.equipSlotCount; i++)
        {
            GameObject slotObject = Instantiate(inventorySlotPrefab, equipmentSlotsParent);
            slotObject.name = $"EquipSlot_{slotNames[i]}";
            
            InventorySlotUI slot = slotObject.GetComponent<InventorySlotUI>();
            if (slot == null)
            {
                slot = slotObject.AddComponent<InventorySlotUI>();
            }
            
            // Initialize slot
            slot.slotIndex = i;
            slot.isEquipSlot = true;
            
            // Add label if there's a TextMeshProUGUI component
            TextMeshProUGUI label = slotObject.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = slotNames[i];
            }
            
            // Add click event
            Button button = slotObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnSlotClicked(slot));
            }
            
            equipmentSlots.Add(slot);
        }
    }
    
    /// <summary>
    /// Update the inventory UI to reflect current inventory state
    /// </summary>
    private void UpdateInventoryUI()
    {
        Item[] items = inventory.GetAllItems();
        
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < items.Length)
            {
                inventorySlots[i].SetItem(items[i]);
            }
            else
            {
                inventorySlots[i].SetItem(null);
            }
        }
        
        // Refresh selected item display if we have one selected
        if (selectedSlot != null)
        {
            UpdateSelectedItemInfo();
        }
    }
    
    /// <summary>
    /// Update the equipment UI to reflect currently equipped items
    /// </summary>
    private void UpdateEquipmentUI()
    {
        Item[] equippedItems = inventory.GetEquippedItems();
        
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            if (i < equippedItems.Length)
            {
                equipmentSlots[i].SetItem(equippedItems[i]);
            }
            else
            {
                equipmentSlots[i].SetItem(null);
            }
        }
        
        // Refresh selected item display if we have one selected
        if (selectedSlot != null)
        {
            UpdateSelectedItemInfo();
        }
    }
    
    /// <summary>
    /// Handle slot clicked event
    /// </summary>
    private void OnSlotClicked(InventorySlotUI slot)
    {
        // Select this slot
        SelectSlot(slot);
    }
    
    /// <summary>
    /// Select an inventory slot and show its item details
    /// </summary>
    private void SelectSlot(InventorySlotUI slot)
    {
        // If the same slot is clicked again, deselect it
        if (selectedSlot == slot)
        {
            selectedSlot = null;
            UpdateSelectedItemInfo();
            return;
        }
        
        // Track the new selected slot
        selectedSlot = slot;
        
        // Update the display
        UpdateSelectedItemInfo();
    }
    
    /// <summary>
    /// Update the item details panel based on selected item
    /// </summary>
    private void UpdateSelectedItemInfo()
    {
        // Get the selected item
        Item selectedItem = null;
        if (selectedSlot != null)
        {
            if (selectedSlot.isEquipSlot)
            {
                selectedItem = inventory.GetEquippedItemAt(selectedSlot.slotIndex);
            }
            else
            {
                selectedItem = inventory.GetItemAt(selectedSlot.slotIndex);
            }
        }
        
        // Update UI elements
        if (selectedItem != null)
        {
            // Show item name and description
            if (selectedItemName != null)
            {
                selectedItemName.text = selectedItem.GetDisplayName();
                selectedItemName.color = selectedItem.GetRarityColor();
            }
            
            if (selectedItemDescription != null)
            {
                selectedItemDescription.text = selectedItem.description;
            }
            
            // Show item stats
            UpdateItemStats(selectedItem);
            
            // Set button interactivity based on context
            UpdateActionButtons(selectedItem, selectedSlot.isEquipSlot);
        }
        else
        {
            // Clear UI when no item is selected
            if (selectedItemName != null)
            {
                selectedItemName.text = "No item selected";
                selectedItemName.color = Color.white;
            }
            
            if (selectedItemDescription != null)
            {
                selectedItemDescription.text = "";
            }
            
            // Clear stats
            ClearItemStats();
            
            // Disable action buttons
            if (equipButton != null) equipButton.interactable = false;
            if (useButton != null) useButton.interactable = false;
            if (dropButton != null) dropButton.interactable = false;
        }
    }
    
    /// <summary>
    /// Update stats display for the selected item
    /// </summary>
    private void UpdateItemStats(Item item)
    {
        // First clear existing stats
        ClearItemStats();
        
        if (item == null || itemStatsPanel == null || itemStatPrefab == null)
            return;
        
        // Add a stat UI element for each stat on the item
        for (int i = 0; i < item.stats.Count; i++)
        {
            // Create or reuse a stat UI element
            GameObject statUI;
            if (i < statUIElements.Count)
            {
                statUI = statUIElements[i];
                statUI.SetActive(true);
            }
            else
            {
                statUI = Instantiate(itemStatPrefab, itemStatsPanel);
                statUIElements.Add(statUI);
            }
            
            // Set the stat text
            TextMeshProUGUI statText = statUI.GetComponent<TextMeshProUGUI>();
            if (statText != null)
            {
                ItemStat stat = item.stats[i];
                statText.text = $"{stat.statName}: {stat.value}";
            }
        }
    }
    
    /// <summary>
    /// Clear all stat UI elements
    /// </summary>
    private void ClearItemStats()
    {
        foreach (GameObject statUI in statUIElements)
        {
            statUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Set up action buttons with appropriate handlers
    /// </summary>
    private void SetupButtons()
    {
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnEquipButtonClicked);
            equipButton.interactable = false;
        }
        
        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
            useButton.interactable = false;
        }
        
        if (dropButton != null)
        {
            dropButton.onClick.AddListener(OnDropButtonClicked);
            dropButton.interactable = false;
        }
    }
    
    /// <summary>
    /// Update button interaction states based on selected item
    /// </summary>
    private void UpdateActionButtons(Item item, bool isEquipSlot)
    {
        if (item == null)
        {
            // Disable all buttons if no item is selected
            if (equipButton != null) equipButton.interactable = false;
            if (useButton != null) useButton.interactable = false;
            if (dropButton != null) dropButton.interactable = false;
            return;
        }
        
        // Equipment slot item vs inventory item
        if (isEquipSlot)
        {
            // Change equip button to unequip
            if (equipButton != null)
            {
                equipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unequip";
                equipButton.interactable = true;
            }
        }
        else
        {
            // Regular inventory item
            if (equipButton != null)
            {
                equipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Equip";
                // Enable equip button only for equippable items
                equipButton.interactable = (item.type == ItemType.Weapon || 
                                            item.type == ItemType.Armor || 
                                            item.type == ItemType.Accessory);
            }
        }
        
        // Use button - only enabled for consumables in inventory
        if (useButton != null)
        {
            useButton.interactable = (!isEquipSlot && item.type == ItemType.Consumable);
        }
        
        // Drop button - enabled for all items
        if (dropButton != null)
        {
            dropButton.interactable = true;
        }
    }
    
    /// <summary>
    /// Handle equip/unequip button click
    /// </summary>
    private void OnEquipButtonClicked()
    {
        if (selectedSlot == null || inventory == null)
            return;
        
        if (selectedSlot.isEquipSlot)
        {
            // Unequip the item
            inventory.UnequipItem(selectedSlot.slotIndex);
        }
        else
        {
            // Equip the item 
            Item selectedItem = inventory.GetItemAt(selectedSlot.slotIndex);
            if (selectedItem != null)
            {
                inventory.EquipItem(selectedItem);
            }
        }
    }
    
    /// <summary>
    /// Handle use button click
    /// </summary>
    private void OnUseButtonClicked()
    {
        if (selectedSlot == null || selectedSlot.isEquipSlot || inventory == null)
            return;
        
        Item selectedItem = inventory.GetItemAt(selectedSlot.slotIndex);
        if (selectedItem != null && selectedItem.type == ItemType.Consumable)
        {
            // Here you would implement the logic for using consumable items
            // For now, we'll just remove the item from inventory
            Debug.Log($"Using item: {selectedItem.itemName}");
            inventory.RemoveItem(selectedItem);
        }
    }
    
    /// <summary>
    /// Handle drop button click
    /// </summary>
    private void OnDropButtonClicked()
    {
        if (selectedSlot == null || inventory == null)
            return;
        
        Item selectedItem;
        if (selectedSlot.isEquipSlot)
        {
            selectedItem = inventory.GetEquippedItemAt(selectedSlot.slotIndex);
            if (selectedItem != null)
            {
                inventory.UnequipItem(selectedSlot.slotIndex);
                inventory.DropItem(selectedItem);
            }
        }
        else
        {
            selectedItem = inventory.GetItemAt(selectedSlot.slotIndex);
            if (selectedItem != null)
            {
                inventory.DropItem(selectedItem);
            }
        }
    }
    
    /// <summary>
    /// Toggle inventory visibility
    /// </summary>
    public void ToggleInventory()
    {
        SetInventoryVisible(!isVisible);
    }
    
    /// <summary>
    /// Set inventory visibility
    /// </summary>
    public void SetInventoryVisible(bool visible)
    {
        isVisible = visible;
        gameObject.SetActive(visible);
        
        // When closing inventory, deselect current item
        if (!visible && selectedSlot != null)
        {
            selectedSlot = null;
            UpdateSelectedItemInfo();
        }
    }
}