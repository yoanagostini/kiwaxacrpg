using UnityEngine;
using TMPro;

// This script handles an item existing in the game world
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
    
    [Tooltip("Collider for detecting clicks")]
    public Collider interactionCollider;
    
    [Header("Hover Effects")]
    [Tooltip("How fast the item rotates")]
    public float rotationSpeed = 30f;
    
    [Tooltip("How high the item hovers")]
    public float hoverHeight = 0.5f;
    
    [Tooltip("How fast the item bobs up and down")]
    public float hoverSpeed = 1f;
    
    [Tooltip("How far the item moves up and down")]
    public float hoverAmount = 0.1f;
    
    [Header("Interaction")]
    [Tooltip("Distance at which player can pick up the item")]
    public float pickupDistance = 2f;
    
    // Reference to player transform for distance check
    private Transform playerTransform;
    
    // Starting position for hover effect
    private Vector3 startPosition;
    
    // Initialize the world item with an actual item
    public void Initialize(Item newItem)
    {
        // Store the item
        this.item = newItem;
        
        // Set up the visual appearance
        SetupVisuals();
        
        // Find the player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Store the starting position for hover effect
        startPosition = transform.position;
    }
    
    // Set up the visual elements based on the item
    private void SetupVisuals()
    {
        if (item == null) return;
        
        // Set the item name with appropriate color
        if (nameText != null)
        {
            nameText.text = item.itemName;
            nameText.color = item.GetRarityColor();
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
    
    // Update is called once per frame
    void Update()
    {
        // Apply visual effects
        ApplyHoverEffect();
        ApplyRotationEffect();
        
        // Make text face camera
        if (nameText != null && Camera.main != null)
        {
            nameText.transform.rotation = Quaternion.LookRotation(nameText.transform.position - Camera.main.transform.position);
        }
        
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
                
                if (Physics.Raycast(ray, out hit) && hit.collider == interactionCollider)
                {
                    // Player clicked on this item
                    PickupItem();
                }
            }
        }
    }
    
    // Pickup the item
    private void PickupItem()
    {
        // This will be expanded when we implement the inventory system
        // For now, just destroy the world item
        Debug.Log($"Picked up {item.GetDisplayName()}");
        
        // Here we would add the item to player's inventory
        // For example: InventoryManager.Instance.AddItem(item);
        
        // Destroy the world item
        Destroy(gameObject);
    }
    
    // Get the item this world object represents
    public Item GetItem()
    {
        return item;
    }
}