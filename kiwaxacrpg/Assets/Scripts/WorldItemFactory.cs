using UnityEngine;
using TMPro;

// This class is responsible for creating WorldItem objects from Items
public class WorldItemFactory : MonoBehaviour
{
    // Singleton instance
    public static WorldItemFactory Instance { get; private set; }
    
    [Header("World Item Settings")]
    [Tooltip("Prefab for world items")]
    public GameObject worldItemPrefab;
    
    [Tooltip("Default model to use if an item doesn't have one")]
    public GameObject defaultItemModel;
    
    [Header("Text Settings")]
    [Tooltip("Font asset for item name text")]
    public TMP_FontAsset itemNameFont;
    
    [Tooltip("Height of the text above the item")]
    public float textHeight = 1f;
    
    [Tooltip("Scale of the text")]
    public float textScale = 1f;
    
    [Header("Default Models")]
    [Tooltip("Default weapon model")]
    public GameObject defaultWeaponModel;
    
    [Tooltip("Default armor model")]
    public GameObject defaultArmorModel;
    
    [Tooltip("Default accessory model")]
    public GameObject defaultAccessoryModel;
    
    [Tooltip("Default consumable model")]
    public GameObject defaultConsumableModel;
    
    private void Awake()
    {
        // Set up singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // This object should persist between scenes
        DontDestroyOnLoad(gameObject);
    }
    
    // Create a world item at the specified position
    public GameObject CreateWorldItem(Item item, Vector3 position)
    {
        if (item == null)
        {
            Debug.LogError("Cannot create world item from null item");
            return null;
        }
        
        if (worldItemPrefab == null)
        {
            Debug.LogError("World item prefab is not assigned");
            return null;
        }
        
        // Instantiate the world item prefab
        GameObject worldItemObject = Instantiate(worldItemPrefab, position, Quaternion.identity);
        
        // Set a proper name for the object
        worldItemObject.name = $"WorldItem_{item.itemName}";
        
        // Get the WorldItem component
        WorldItem worldItem = worldItemObject.GetComponent<WorldItem>();
        if (worldItem == null)
        {
            Debug.LogError("World item prefab is missing WorldItem component");
            Destroy(worldItemObject);
            return null;
        }
        
        // If the item doesn't have a prefab, assign a default one based on type
        if (item.prefab == null)
        {
            AssignDefaultModel(item, worldItem);
        }
        
        // Set up the text component if it doesn't exist
        SetupNameText(worldItem);
        
        // Initialize the world item with the actual item data
        worldItem.Initialize(item);
        
        return worldItemObject;
    }
    
    // Assign a default model based on item type
    private void AssignDefaultModel(Item item, WorldItem worldItem)
    {
        GameObject modelPrefab = null;
        
        // Select model based on item type
        switch (item.type)
        {
            case ItemType.Weapon:
                modelPrefab = defaultWeaponModel;
                break;
            case ItemType.Armor:
                modelPrefab = defaultArmorModel;
                break;
            case ItemType.Accessory:
                modelPrefab = defaultAccessoryModel;
                break;
            case ItemType.Consumable:
                modelPrefab = defaultConsumableModel;
                break;
            default:
                modelPrefab = defaultItemModel;
                break;
        }
        
        // If we have a model, instantiate it
        if (modelPrefab != null)
        {
            GameObject model = Instantiate(modelPrefab, worldItem.transform.position, Quaternion.identity);
            model.transform.SetParent(worldItem.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            worldItem.itemModel = model;
        }
    }
    
    // Set up the name text component
    private void SetupNameText(WorldItem worldItem)
    {
        if (worldItem.nameText == null)
        {
            // Create a new GameObject for the text
            GameObject textObject = new GameObject("ItemNameText");
            textObject.transform.SetParent(worldItem.transform);
            textObject.transform.localPosition = new Vector3(0, textHeight, 0);
            textObject.transform.localRotation = Quaternion.identity;
            textObject.transform.localScale = new Vector3(textScale, textScale, textScale);
            
            // Add TextMeshPro component
            TextMeshPro tmp = textObject.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 5;
            
            // Assign font if available
            if (itemNameFont != null)
            {
                tmp.font = itemNameFont;
            }
            
            worldItem.nameText = tmp;
        }
    }
    
    // Create a random item at the specified position (for testing)
    public GameObject CreateRandomItem(Vector3 position)
    {
        // Generate a random weapon using the item database
        if (ItemDatabase.Instance != null)
        {
            // Generate a random weapon
            WeaponItem randomWeapon = ItemDatabase.Instance.GenerateRandomWeapon();
            
            // Create a world item from it
            return CreateWorldItem(randomWeapon, position);
        }
        
        return null;
    }
}