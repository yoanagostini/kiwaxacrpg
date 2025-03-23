using UnityEngine;
using System.Collections.Generic;

// An enemy manager that coordinates positions for multiple enemies
public class EnemyManager : MonoBehaviour
{
    [Tooltip("Reference to the player transform")]
    public Transform playerTransform;
    
    [Tooltip("How many concentric rings around the player")]
    public int maxRings = 3;
    
    [Tooltip("Base distance from player for first ring")]
    public float baseStoppingDistance = 2.5f;
    
    [Tooltip("Distance between rings")]
    public float ringSpacing = 1.5f;
    
    [Tooltip("How many enemies can fit in each ring")]
    public int[] enemiesPerRing = { 8, 12, 16 };
    
    // Internal mapping of enemies to their assigned positions
    private Dictionary<BasicEnemy, Vector3> enemyPositions = new Dictionary<BasicEnemy, Vector3>();
    
    // Static reference so enemies can find the manager
    public static EnemyManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }
    
    private void Update()
    {
        // Periodically reassign positions (not every frame)
        if (Time.frameCount % 60 == 0)
        {
            AssignPositionsToAllEnemies();
        }
    }
    
    // Register a new enemy with the manager
    public void RegisterEnemy(BasicEnemy enemy)
    {
        if (!enemyPositions.ContainsKey(enemy))
        {
            enemyPositions.Add(enemy, Vector3.zero);
            // Immediately assign a position
            AssignPositionToEnemy(enemy);
        }
    }
    
    // Unregister an enemy (when it dies)
    public void UnregisterEnemy(BasicEnemy enemy)
    {
        if (enemyPositions.ContainsKey(enemy))
        {
            enemyPositions.Remove(enemy);
        }
    }
    
    // Get the assigned position for an enemy
    public Vector3 GetAssignedPosition(BasicEnemy enemy)
    {
        if (enemyPositions.ContainsKey(enemy))
        {
            return enemyPositions[enemy];
        }
        
        // If not registered, register now
        RegisterEnemy(enemy);
        return enemyPositions[enemy];
    }
    
    // Assign positions to all registered enemies
    private void AssignPositionsToAllEnemies()
    {
        // Get all enemies using the non-deprecated method
        BasicEnemy[] allEnemies = Object.FindObjectsByType<BasicEnemy>(FindObjectsSortMode.None);
        
        // Create list of enemies to assign positions to
        List<BasicEnemy> enemiesNeedingPositions = new List<BasicEnemy>();
        
        foreach (BasicEnemy enemy in allEnemies)
        {
            // Make sure enemy is registered
            if (!enemyPositions.ContainsKey(enemy))
            {
                enemyPositions.Add(enemy, Vector3.zero);
            }
            
            enemiesNeedingPositions.Add(enemy);
        }
        
        // Sort enemies by distance to player (closest first)
        enemiesNeedingPositions.Sort((a, b) => 
            Vector3.Distance(a.transform.position, playerTransform.position)
            .CompareTo(Vector3.Distance(b.transform.position, playerTransform.position)));
        
        // Position counter for each ring
        int[] positionCounter = new int[maxRings];
        
        // Assign positions - closest enemies get priority for inner rings
        foreach (BasicEnemy enemy in enemiesNeedingPositions)
        {
            // Find the first ring that has space
            int targetRing = 0;
            while (targetRing < maxRings && positionCounter[targetRing] >= enemiesPerRing[targetRing])
            {
                targetRing++;
            }
            
            // If all rings are full, add to outer ring
            if (targetRing >= maxRings)
            {
                targetRing = maxRings - 1;
            }
            
            // Calculate position in this ring
            float ringDistance = baseStoppingDistance + (targetRing * ringSpacing);
            int positionInRing = positionCounter[targetRing]++;
            int totalPositionsInRing = enemiesPerRing[targetRing];
            
            // Calculate angle
            float angle = (360f / totalPositionsInRing) * positionInRing;
            float rad = angle * Mathf.Deg2Rad;
            
            // Calculate position at this angle and distance
            Vector3 position = playerTransform.position + 
                new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * ringDistance;
            
            // Assign this position to the enemy
            enemyPositions[enemy] = position;
        }
    }
    
    // Assign a position to a specific enemy
    private void AssignPositionToEnemy(BasicEnemy enemy)
    {
        // Calculate a reasonable position for this enemy
        Vector3 directionFromPlayer = (enemy.transform.position - playerTransform.position).normalized;
        if (directionFromPlayer == Vector3.zero)
        {
            // If enemy is at same position as player, pick a random direction
            float randomAngle = Random.Range(0, 360) * Mathf.Deg2Rad;
            directionFromPlayer = new Vector3(Mathf.Sin(randomAngle), 0, Mathf.Cos(randomAngle));
        }
        
        // Position at base stopping distance
        Vector3 position = playerTransform.position + directionFromPlayer * baseStoppingDistance;
        
        // Assign position
        enemyPositions[enemy] = position;
    }
    
    // Visualization for debugging
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        
        // Draw rings
        for (int ring = 0; ring < maxRings; ring++)
        {
            float ringDistance = baseStoppingDistance + (ring * ringSpacing);
            
            // Draw ring
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            DrawCircle(playerTransform.position, ringDistance, 32);
            
            // Draw positions in this ring
            Gizmos.color = new Color(0, 1, 0, 0.7f);
            int positionsInRing = ring < enemiesPerRing.Length ? enemiesPerRing[ring] : 0;
            
            for (int pos = 0; pos < positionsInRing; pos++)
            {
                float angle = (360f / positionsInRing) * pos;
                float rad = angle * Mathf.Deg2Rad;
                
                Vector3 position = playerTransform.position + 
                    new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * ringDistance;
                
                Gizmos.DrawSphere(position, 0.2f);
            }
        }
    }
    
    // Helper method to draw a circle
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float nextAngle = angle + angleStep;
            
            float radAngle = angle * Mathf.Deg2Rad;
            float radNextAngle = nextAngle * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Sin(radAngle), 0, Mathf.Cos(radAngle)) * radius;
            Vector3 point2 = center + new Vector3(Mathf.Sin(radNextAngle), 0, Mathf.Cos(radNextAngle)) * radius;
            
            Gizmos.DrawLine(point1, point2);
            
            angle = nextAngle;
        }
    }
}