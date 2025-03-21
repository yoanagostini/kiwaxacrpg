using UnityEngine;
using System.Collections.Generic;

// PlayerController.cs
// This script handles the player's movement and camera control
// It's designed to be extendable for future character classes

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed of the player character")]
    public float moveSpeed = 5f;
    
    [Tooltip("How quickly the player rotates to face movement direction")]
    public float rotationSpeed = 10f;
    
    [Header("Camera Settings")]
    [Tooltip("The main camera transform - assigned automatically if child of player")]
    public Transform cameraTransform;
    
    [Tooltip("Distance from player to camera - larger values create more top-down view")]
    public float cameraDistance = 10f;
    
    [Tooltip("Camera height above player - higher values create more top-down view")]
    public float cameraHeight = 8f;
    
    [Tooltip("Camera pitch angle in degrees - adjust for desired viewing angle")]
    public float cameraPitch = 45f;
    
    [Tooltip("Optional camera offset from player's center")]
    public Vector3 cameraOffset = new Vector3(0f, 0f, 0f);
    
    [Header("Character Stats - Extendable")]
    [Tooltip("Base stats for the character - can be extended per class")]
    protected Dictionary<string, float> characterStats = new Dictionary<string, float>();

    // Reference to the character's Rigidbody component
    protected Rigidbody rb;
    
    // The current movement direction
    protected Vector3 movementDirection;

    // Virtual method for initialization to be overridden by child classes
    protected virtual void Start()
    {
        // Get rigidbody component
        rb = GetComponent<Rigidbody>();
        
        // Initialize with default stats - child classes can add more
        InitializeDefaultStats();
        
        // Find camera if not set
        if (cameraTransform == null)
        {
            // Try to find camera in children
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
            {
                cameraTransform = childCamera.transform;
                Debug.Log("Camera found as child of player");
            }
            else
            {
                // Try to find main camera
                cameraTransform = Camera.main.transform;
                Debug.Log("Using main camera for player view");
            }
        }
        
        // Set up camera position and rotation
        SetupCamera();
    }
    
    // Initialize default character stats
    protected virtual void InitializeDefaultStats()
    {
        // Base stats any character would have
        characterStats.Add("Health", 100f);
        characterStats.Add("Mana", 50f);
        characterStats.Add("Strength", 10f);
        characterStats.Add("Dexterity", 10f);
        characterStats.Add("Intelligence", 10f);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Handle player input
        HandleInput();
        
        // Update camera position to follow player
        UpdateCameraPosition();
    }
    
    // Fixed update for physics calculations
    protected virtual void FixedUpdate()
    {
        // Move the player
        MovePlayer();
    }
    
    // Handle player input
    protected virtual void HandleInput()
    {
        // Get horizontal and vertical axis input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Calculate movement direction from input
        movementDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }
    
    // Move the player based on input
    protected virtual void MovePlayer()
    {
        // If there's movement input
        if (movementDirection.magnitude > 0.1f)
        {
            // Use rigidbody to move player
            rb.linearVelocity = movementDirection * moveSpeed;
            
            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Stop player if no input
            rb.linearVelocity = Vector3.zero;
        }
    }
    
    // Set up initial camera position and rotation
    protected virtual void SetupCamera()
    {
        if (cameraTransform != null)
        {
            // Position camera based on settings
            UpdateCameraPosition();
            
            // Set camera rotation (pitch)
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
        else
        {
            Debug.LogError("No camera assigned to player controller!");
        }
    }
    
    // Update camera position to follow player
    protected virtual void UpdateCameraPosition()
    {
        if (cameraTransform != null)
        {
            // Calculate position behind and above player
            Vector3 targetPosition = transform.position;
            
            // Apply height
            targetPosition.y += cameraHeight;
            
            // Apply distance (move back in z)
            targetPosition -= transform.forward * cameraDistance;
            
            // Apply any additional offset
            targetPosition += cameraOffset;
            
            // Set camera position
            cameraTransform.position = targetPosition;
            
            // Make camera look at player
            cameraTransform.LookAt(transform.position + Vector3.up * 2f); // Look slightly above player's center
        }
    }
    
    // Get a character stat - can be used by other systems
    public virtual float GetStat(string statName)
    {
        if (characterStats.ContainsKey(statName))
        {
            return characterStats[statName];
        }
        
        Debug.LogWarning($"Stat {statName} not found for this character");
        return 0f;
    }
    
    // Set a character stat - can be used by other systems
    public virtual void SetStat(string statName, float value)
    {
        if (characterStats.ContainsKey(statName))
        {
            characterStats[statName] = value;
        }
        else
        {
            // Add new stat if it doesn't exist
            characterStats.Add(statName, value);
        }
    }
}