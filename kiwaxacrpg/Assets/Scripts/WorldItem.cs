using UnityEngine;
using TMPro;

// This script handles an item existing in the game world with improved pickup functionality
public class WorldItem : MonoBehaviour
{
    [Header("Item Reference")]
    [Tooltip("The actual item this world object represents")]
    private Item item;
    
    [Header("Visual Elements")]
    [Tooltip("The 3D model of the item")]
    public GameObject itemModel;
    
    [Tooltip("Text above the item showing its name")]
    public TextMeshPro nameText;
    
    [Header("Hover Effects")]
    [Tooltip("How fast the item rotates")]
    public float rotationSpeed = 30f;
    
    [Tooltip("How high the item hovers")]
    public float hoverHeight = 0.5f;
    
    [Tooltip("How fast the item bobs up and down")]
    public float hoverSpeed = 1f;
    
    [Tooltip("How far the item moves up and down")]
    public float hoverAmount = 0.1f;
    
    [Header("Text Settings")]
    [Tooltip("Direction the text should face (angled slightly upward and forward)")]
    public Vector3 textFacingDirection = new Vector3(0, 0.3f, 1);
    
    [Header("Text Background")]
    [Tooltip("Whether to create a background quad behind text")]
    public bool createTextBackground = true;
    
    [Tooltip("Background color (will be tinted by rarity)")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    
    [Tooltip("Size multiplier for the background compared to text")]
    public Vector2 backgroundPadding = new Vector2(0.2f, 0.1f);
    
    [Header("Interaction")]
    [Tooltip("Distance at which player can pick up the item")]
    public float pickupDistance = 2f;
    
    [Tooltip("Whether to auto-create a collider if none is found")]
    public bool autoCreateCollider = true;
    
    [Tooltip("Size of auto-created collider")]
    public Vector3 colliderSize = new Vector3(1.5f, 1.5f, 1.5f);
    
    // Reference to player transform for distance check
    private Transform playerTransform;
    
    // Starting position for hover effect
    private Vector3 startPosition;
    
    // Background quad reference
    private GameObject textBackground;
    
    // Collider for interaction
    private Collider itemCollider;
    
    // Initialize the world item with an actual item
    public void Initialize(Item newItem)
    {
        // Store the item
        this.item = newItem;
        
        // Set up the visual appearance
        SetupVisuals();
        
        // Set up the collider for interaction
        SetupCollider();
        
        // Find the player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found. Make sure your player has the 'Player' tag.");
        }
        
        // Store the starting position for hover effect
        startPosition = transform.position;
        
        // Set the fixed orientation for text if we have a text component
        if (nameText != null)
        {
            // Set text to face the specified direction
            nameText.transform.rotation = Quaternion.LookRotation(textFacingDirection);
            
            // Create text background if enabled
            if (createTextBackground)
            {
                CreateTextBackground();
            }
        }
        
        // Log item creation for debugging
        Debug.Log($"WorldItem initialized: {item.itemName}, Rarity: {item.rarity}");
    }
    
    // Set up the collider for click detection
    private void SetupCollider()
    {
        // First check if we already have a collider
        itemCollider = GetComponent<Collider>();
        
        // If no collider and auto-create is enabled, add a box collider
        if (itemCollider == null && autoCreateCollider)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = colliderSize;
            boxCollider.center = new Vector3(0, hoverHeight, 0); // Center at hover height
            boxCollider.isTrigger = true; // Use trigger to avoid physics interactions
            
            itemCollider = boxCollider;
            Debug.Log("Created collider for item interaction");
        }
        
        // If we still don't have a collider, warn the user
        if (itemCollider == null)
        {
            Debug.LogWarning("WorldItem has no collider assigned and autoCreateCollider is disabled. Click detection won't work.");
        }
    }
    
    // Set up the visual elements based on the item
    private void SetupVisuals()
    {
        if (item == null) return;
        
        // Set the item name with appropriate color and background settings
        if (nameText != null)
        {
            nameText.text = item.itemName;
            Color rarityColor = item.GetRarityColor();
            
            // Configure text appearance with background
            nameText.color = rarityColor;
            
            // Enable text outline for better visibility
            nameText.enableVertexGradient = false;
            nameText.fontMaterial.EnableKeyword("OUTLINE_ON");
            nameText.outlineWidth = 0.2f;
            nameText.outlineColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark outline
            
            // Set up material for better visibility
            nameText.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
            nameText.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 0);
            nameText.fontSharedMaterial.SetFloat("_UnderlayOffsetY", 0);
            nameText.fontSharedMaterial.SetFloat("_UnderlayDilate", 1);
            nameText.fontSharedMaterial.SetFloat("_UnderlaySoftness", 0);
            nameText.fontSharedMaterial.SetColor("_UnderlayColor", new Color(0.05f, 0.05f, 0.05f, 1f)); // Very dark background
        }
        
        // If there's a model prefab in the item, spawn it
        if (item.prefab != null && itemModel == null)
        {
            // Instantiate the model as a child of this object
            GameObject model = Instantiate(item.prefab, transform.position, Quaternion.identity);
            model.transform.SetParent(transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            
            // Set the model reference
            itemModel = model;
            
            // Make sure it has no colliders (we use our own)
            Collider[] colliders = model.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            
            // Make sure it has no rigidbodies
            Rigidbody[] rigidbodies = model.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
            }
        }
    }
    
    // Create a background quad behind the text for better visibility
    private void CreateTextBackground()
    {
        if (nameText == null || item == null) return;
        
        // Create the background quad
        textBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        textBackground.name = "TextBackground";
        
        // Parent it to the text (so it moves with text)
        textBackground.transform.SetParent(nameText.transform);
        
        // Position slightly behind text
        textBackground.transform.localPosition = new Vector3(0, 0, 0.01f);
        textBackground.transform.localRotation = Quaternion.identity;
        
        // Scale background based on text size with padding
        Vector2 textSize = nameText.GetPreferredValues();
        textBackground.transform.localScale = new Vector3(
            textSize.x + backgroundPadding.x,
            textSize.y + backgroundPadding.y, 
            1
        );
        
        // Get rarity color and create a darker version for background
        Color rarityColor = item.GetRarityColor();
        Color darkRarityColor = new Color(
            rarityColor.r * 0.5f, 
            rarityColor.g * 0.5f, 
            rarityColor.b * 0.5f, 
            backgroundColor.a
        );
        
        // Use a simpler approach with standard material
        Renderer bgRenderer = textBackground.GetComponent<Renderer>();
        bgRenderer.material = new Material(Shader.Find("Sprites/Default"));
        bgRenderer.material.color = darkRarityColor;
        
        // Alternative method if above doesn't work - create color texture
        if (bgRenderer != null)
        {
            // Create a texture for the background color
            Texture2D bgTexture = new Texture2D(2, 2);
            Color[] colors = new Color[4] { darkRarityColor, darkRarityColor, darkRarityColor, darkRarityColor };
            bgTexture.SetPixels(colors);
            bgTexture.Apply();
            
            // Apply texture to material
            bgRenderer.material.mainTexture = bgTexture;
        }
        
        // Debug log to ensure colors are being set correctly
        Debug.Log($"Item {item.itemName} - Rarity: {item.rarity} - Color: {rarityColor} - Background: {darkRarityColor}");
        
        // Remove collider from background (we don't want it to interfere with clicks)
        Destroy(textBackground.GetComponent<Collider>());
    }
    
    // Update is called once per frame
    void Update()
    {
        // Apply visual effects
        ApplyHoverEffect();
        ApplyRotationEffect();
        
        // Check for player interaction
        CheckPlayerInteraction();
    }
    
    // Apply hover effect (bobbing up and down)
    private void ApplyHoverEffect()
    {
        if (itemModel != null)
        {
            // Calculate hover offset based on time
            float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
            
            // Apply hover to the model
            itemModel.transform.localPosition = new Vector3(0, hoverHeight + yOffset, 0);
        }
    }
    
    // Apply rotation effect
    private void ApplyRotationEffect()
    {
        if (itemModel != null)
        {
            // Rotate around the Y axis
            itemModel.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
    
    // Check for player interaction
    private void CheckPlayerInteraction()
    {
        if (playerTransform == null) return;
        
        // Check if player is close enough
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= pickupDistance)
        {
            // Check for mouse click on this item
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                // Perform the raycast with proper layer mask
                if (Physics.Raycast(ray, out hit))
                {
                    // Check if we hit this item's collider
                    if (hit.collider == itemCollider || (hit.collider.transform.IsChildOf(transform)))
                    {
                        Debug.Log($"Clicked on item: {item.itemName}");
                        PickupItem();
                    }
                }
            }
        }
    }
    
    // Pickup the item
    private void PickupItem()
    {
        Debug.Log($"Picked up {item.GetDisplayName()}");
        
        // Here we would add the item to player's inventory
        // For example: InventoryManager.Instance.AddItem(item);
        
        // For now, just destroy the world item
        Destroy(gameObject);
    }
    
    // Get the item this world object represents
    public Item GetItem()
    {
        return item;
    }
    
    // Show pickup range in editor (for debugging)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
        
        if (itemCollider != null && itemCollider is BoxCollider)
        {
            BoxCollider box = itemCollider as BoxCollider;
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}