using UnityEngine;
using System.Collections.Generic;

public class CageManager : MonoBehaviour
{
    [Header("Cage Area")]
    [SerializeField] private Transform cageContainer;
    [SerializeField] private Vector2 cageSize = new Vector2(3f, 3f); // Size of roaming area
    [SerializeField] private float npcScale = 0.3f; // Smaller NPCs
    
    [Header("Animation")]
    [SerializeField] private float flyToCageDuration = 0.5f;
    [SerializeField] private AnimationCurve flyToCageCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Capacity")]
    [SerializeField] private int maxCageCapacity = 20;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject captureEffect;
    
    private List<GameObject> capturedNPCs = new List<GameObject>();
    
    public int CapturedCount => capturedNPCs.Count;
    public int MaxCapacity => maxCageCapacity;
    public bool IsFull => capturedNPCs.Count >= maxCageCapacity;
    public Vector2 CageSize => cageSize;
    public Transform CageContainer => cageContainer;
    
    private void Start()
    {
        if (cageContainer == null)
        {
            GameObject container = new GameObject("CageContainer");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            cageContainer = container.transform;
        }
    }
    
    public void CaptureNPC(GameObject npc)
    {
        if (IsFull)
        {
            Debug.Log("Cage is full!");
            // Still destroy the NPC
            Destroy(npc);
            return;
        }
        
        StartCoroutine(FlyNPCToCage(npc));
    }
    
    private System.Collections.IEnumerator FlyNPCToCage(GameObject npc)
    {
        Vector3 startPosition = npc.transform.position;
        
        // Random position inside cage
        Vector3 targetLocalPos = new Vector3(
            Random.Range(-cageSize.x / 2, cageSize.x / 2),
            Random.Range(-cageSize.y / 2, cageSize.y / 2),
            0
        );
        Vector3 targetWorldPos = cageContainer.TransformPoint(targetLocalPos);
        
        // Disable main game behaviors temporarily
        NPCMovement movement = npc.GetComponent<NPCMovement>();
        if (movement != null) movement.enabled = false;
        
        Collider2D collider = npc.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
        
        float elapsed = 0f;
        
        while (elapsed < flyToCageDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / flyToCageDuration;
            float curveValue = flyToCageCurve.Evaluate(progress);
            
            // Fly to cage
            npc.transform.position = Vector3.Lerp(startPosition, targetWorldPos, curveValue);
            
            // Scale down
            float scale = Mathf.Lerp(1f, npcScale, curveValue);
            npc.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        // Finalize
        npc.transform.position = targetWorldPos;
        npc.transform.localScale = Vector3.one * npcScale;
        npc.transform.SetParent(cageContainer);
        
        // Change to local position
        npc.transform.localPosition = targetLocalPos;
        
        // Update sorting
        SpriteRenderer sr = npc.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 6;
        }
        
        // Add cage movement behavior
        CageNPCMovement cageMovement = npc.GetComponent<CageNPCMovement>();
        if (cageMovement == null)
        {
            cageMovement = npc.AddComponent<CageNPCMovement>();
        }
        cageMovement.Initialize(this);
        
        // Re-enable collider for cage interactions
        if (collider != null)
        {
            collider.enabled = true;
            collider.isTrigger = true; // Make trigger in cage
        }
        
        // Add to list
        capturedNPCs.Add(npc);
        
        // Visual feedback
        if (captureEffect != null)
        {
            Instantiate(captureEffect, targetWorldPos, Quaternion.identity);
        }
        
        Debug.Log($"NPC captured! {CapturedCount}/{maxCageCapacity}");
    }
    
    public void ReleaseAllNPCs()
    {
        foreach (GameObject npc in capturedNPCs)
        {
            if (npc != null) Destroy(npc);
        }
        capturedNPCs.Clear();
    }
    
    // Visualize cage bounds in editor
    private void OnDrawGizmos()
    {
        if (cageContainer != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = cageContainer.position;
            Gizmos.DrawWireCube(center, new Vector3(cageSize.x, cageSize.y, 0));
        }
    }
}