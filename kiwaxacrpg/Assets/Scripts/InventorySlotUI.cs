using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single slot in the inventory UI
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Image to display the item icon")]
    public Image itemIcon;
    
    [Tooltip("Text for showing item name or count")]
    public TextMeshProUGUI itemText;
    
    [Tooltip("Background image for the slot")]
    public Image slotBackground;
    
    [Tooltip("Selection indicator")]
    public GameObject selectionIndicator;
    
    [Header("Settings")]
    [Tooltip("Color for empty slots")]
    public Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    [Tooltip("Color for filled slots")]
    public Color filledSlotColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);
    
    [Tooltip("Alpha for the item icon when slot is empty")]
    [Range(0f, 1f)]
    public float emptyIconAlpha = 0.4f;
    
    // Slot data
    //[HideInInspector]
    public int slotIndex = 0;
    
    //[HideInInspector]
    public bool isEquipSlot = false;
    
    // Current item in this slot
    private Item currentItem = null;
    
    // Whether this slot is currently selected
    private bool isSelected = false;
    
    // Initialize the slot
    private void Awake()
    {
        // Find UI components if not already assigned
        if (itemIcon == null)
            itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        
        if (itemText == null)
            itemText = transform.Find("ItemText")?.GetComponent<TextMeshProUGUI>();
        
        if (slotBackground == null)
            slotBackground = GetComponent<Image>();
        
        if (selectionIndicator == null)
            selectionIndicator = transform.Find("SelectionIndicator")?.gameObject;
        
        // Set initial state
        SetSelected(false);
        SetItem(null);
    }
    
    /// <summary>
    /// Set the item displayed in this slot
    /// </summary>
    /// <param name="item">Item to display, or null for empty slot</param>
    public void SetItem(Item item)
    {
        currentItem = item;
        
        if (item != null)
        {
            // Show the item icon
            if (itemIcon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.color = new Color(1f, 1f, 1f, 1f); // Full opacity
                itemIcon.enabled = true;
            }
            
            // Show item name or stack count
            if (itemText != null)
            {
                if (item.isStackable)
                {
                    // Would show stack count here if implemented
                    itemText.text = "";
                }
                else
                {
                    itemText.text = "";
                }
                
                // Color text by rarity
                itemText.color = item.GetRarityColor();
            }
            
            // Color slot based on rarity if we have a background
            if (slotBackground != null)
            {
                // Use a more subtle version of the rarity color
                Color rarityColor = item.GetRarityColor();
                Color slotColor = new Color(
                    rarityColor.r * 0.5f + 0.5f,
                    rarityColor.g * 0.5f + 0.5f,
                    rarityColor.b * 0.5f + 0.5f,
                    filledSlotColor.a
                );
                slotBackground.color = slotColor;
            }
        }
        else
        {
            // Empty slot
            if (itemIcon != null)
            {
                // Keep the icon for equipment slots to show what goes there
                if (isEquipSlot && itemIcon.sprite != null)
                {
                    itemIcon.color = new Color(1f, 1f, 1f, emptyIconAlpha); // Reduced opacity
                    itemIcon.enabled = true;
                }
                else
                {
                    itemIcon.sprite = null;
                    itemIcon.enabled = false;
                }
            }
            
            // Clear text
            if (itemText != null)
            {
                itemText.text = "";
            }
            
            // Use empty slot color for background
            if (slotBackground != null)
            {
                slotBackground.color = emptySlotColor;
            }
        }
    }
    
    /// <summary>
    /// Set whether this slot is currently selected
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        // Show/hide selection indicator if we have one
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
        
        // Could also change slot background color when selected
        if (selected && slotBackground != null)
        {
            // Store original color and apply a highlight effect
            // Implementation depends on your UI design
        }
    }
    
    /// <summary>
    /// Get the current item in this slot
    /// </summary>
    /// <returns>Current item or null if empty</returns>
    public Item GetItem()
    {
        return currentItem;
    }
    
    /// <summary>
    /// Check if this slot is empty
    /// </summary>
    /// <returns>True if slot contains no item</returns>
    public bool IsEmpty()
    {
        return currentItem == null;
    }
}