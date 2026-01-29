using UnityEngine;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int maxNPCsOnScreen = 10;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnPadding = 1f;
    
    [Header("Pool Reference")]
    [SerializeField] private NPCPool npcPool;
    
    private List<GameObject> activeNPCs = new List<GameObject>();
    private float spawnTimer;
    private Camera mainCamera;
    private Vector2 screenBounds;
    
    private void Start()
    {
        mainCamera = Camera.main;
        CalculateScreenBounds();
        
        // Spawn initial NPCs
        for (int i = 0; i < maxNPCsOnScreen; i++)
        {
            SpawnNPC();
        }
    }
    
    private void Update()
    {
        // Remove null/inactive references
        activeNPCs.RemoveAll(npc => npc == null || !npc.activeInHierarchy);
        
        // Spawn new NPCs if below max
        if (activeNPCs.Count < maxNPCsOnScreen)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnNPC();
                spawnTimer = spawnInterval;
            }
        }
    }
    
    private void CalculateScreenBounds()
    {
        float height = mainCamera.orthographicSize * 2;
        float width = height * mainCamera.aspect;
        screenBounds = new Vector2(width / 2 - spawnPadding, height / 2 - spawnPadding);
    }
    
    private void SpawnNPC()
    {
        if (npcPool == null)
        {
            Debug.LogError("NPC Pool not assigned!");
            return;
        }
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject npc = npcPool.SpawnFromPool(spawnPosition, Quaternion.identity);
        
        if (npc != null)
        {
            activeNPCs.Add(npc);
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-screenBounds.x, screenBounds.x);
        float y = Random.Range(-screenBounds.y, screenBounds.y);
        return new Vector3(x, y, 0);
    }
}
