using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// EnemySpawner.cs
// This script handles spawning enemies outside of the camera view

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Enemy prefab to spawn")]
    public GameObject enemyPrefab;
    
    [Tooltip("Maximum number of enemies allowed at once")]
    public int maxEnemies = 10;
    
    [Tooltip("Time between spawn attempts in seconds")]
    public float spawnInterval = 3f;
    
    [Tooltip("Minimum distance from player to spawn enemies")]
    public float minSpawnDistance = 10f;
    
    [Tooltip("Maximum distance from player to spawn enemies")]
    public float maxSpawnDistance = 20f;
    
    [Tooltip("Size of the spawn area (width and length)")]
    public Vector2 spawnAreaSize = new Vector2(40f, 40f);
    
    [Header("Spawn Visualization")]
    [Tooltip("If true, will draw gizmos to show spawn area in the editor")]
    public bool showSpawnArea = true;
    
    [Tooltip("Color for the spawn area gizmo")]
    public Color spawnAreaColor = new Color(1f, 0f, 0f, 0.3f);
    
    // Reference to the player transform
    private Transform playerTransform;
    
    // Reference to the main camera
    private Camera mainCamera;
    
    // List to keep track of all spawned enemies
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag assigned.");
        }
        
        // Get the main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
        
        // Start the spawning coroutine
        StartCoroutine(SpawnEnemiesRoutine());
    }
    
    // Coroutine for spawning enemies
    private IEnumerator SpawnEnemiesRoutine()
    {
        // Wait for a short time before starting to spawn
        yield return new WaitForSeconds(1f);
        
        while (true)
        {
            // Remove any destroyed enemies from our list
            spawnedEnemies.RemoveAll(enemy => enemy == null);
            
            // Only spawn if we haven't reached the max number of enemies
            if (spawnedEnemies.Count < maxEnemies)
            {
                // Spawn a new enemy
                SpawnEnemy();
            }
            
            // Wait for the specified interval
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    // Spawn a single enemy outside the camera view
    private void SpawnEnemy()
    {
        if (enemyPrefab == null || playerTransform == null || mainCamera == null)
        {
            Debug.LogError("Missing references for enemy spawning!");
            return;
        }
        
        // Get a position outside the camera view
        Vector3 spawnPosition = GetSpawnPositionOutsideCamera();
        
        // Instantiate the enemy at the spawn position
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Add to our list of spawned enemies
        spawnedEnemies.Add(enemy);
        
        Debug.Log($"Spawned enemy at position: {spawnPosition}. Total enemies: {spawnedEnemies.Count}");
    }
    
    // Get a spawn position outside the camera view
    private Vector3 GetSpawnPositionOutsideCamera()
    {
        Vector3 spawnPosition;
        bool validPosition = false;
        int attempts = 0;
        const int MAX_ATTEMPTS = 30;
        
        do
        {
            // Get a random angle around the player
            float angle = Random.Range(0f, 360f);
            
            // Get a random distance from the player (between min and max spawn distance)
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            
            // Calculate position based on angle and distance
            float x = playerTransform.position.x + distance * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = playerTransform.position.z + distance * Mathf.Sin(angle * Mathf.Deg2Rad);
            
            // Use the same Y position as the player
            spawnPosition = new Vector3(x, playerTransform.position.y, z);
            
            // Check if the position is outside the camera's view
            validPosition = IsPositionOutsideCamera(spawnPosition);
            
            // Prevent infinite loops
            attempts++;
            
        } while (!validPosition && attempts < MAX_ATTEMPTS);
        
        // If we couldn't find a valid position, use the last calculated position anyway
        if (!validPosition)
        {
            Debug.LogWarning("Could not find a valid spawn position outside camera view after " + MAX_ATTEMPTS + " attempts.");
        }
        
        return spawnPosition;
    }
    
    // Check if a world position is outside the camera's view
    private bool IsPositionOutsideCamera(Vector3 worldPosition)
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);
        
        // ViewportPoint returns values from 0 to 1 for positions on screen
        // If x or y is less than 0 or greater than 1, the position is off-screen
        // If z is negative, the position is behind the camera
        return viewportPosition.x < 0 || viewportPosition.x > 1 ||
               viewportPosition.y < 0 || viewportPosition.y > 1 ||
               viewportPosition.z < 0;
    }
    
    // Draw gizmos for the spawn area in the editor
    private void OnDrawGizmos()
    {
        if (showSpawnArea && playerTransform != null)
        {
            Gizmos.color = spawnAreaColor;
            
            // Draw the min spawn distance circle
            DrawCircle(playerTransform.position, minSpawnDistance, 32);
            
            // Draw the max spawn distance circle
            DrawCircle(playerTransform.position, maxSpawnDistance, 32);
        }
    }
    
    // Helper method to draw a circle gizmo
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;
            
            Gizmos.DrawLine(point1, point2);
        }
    }
}