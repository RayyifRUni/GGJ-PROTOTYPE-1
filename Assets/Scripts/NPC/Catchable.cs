using UnityEngine;

public class Catchable : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int points = 10;
    [SerializeField] private GameObject catchEffect;
    
    [Header("Cage System")]
    [SerializeField] private bool sendToCage = true;
    
    private static CageManager cageManager;
    
    private void Start()
    {
        // Find cage manager (only once)
        if (cageManager == null)
        {
            cageManager = FindObjectOfType<CageManager>();
        }
    }
    
    public void OnCaught()
    {
        Debug.Log($"{gameObject.name} caught! +{points} points");
        
        // Play catch effect at current position
        if (catchEffect != null)
        {
            Instantiate(catchEffect, transform.position, Quaternion.identity);
        }
        
        // Add points to score
        // GameManager.Instance?.AddScore(points);
        
        if (sendToCage && cageManager != null)
        {
            // Send to cage instead of destroying
            if (!cageManager.IsFull)
            {
                cageManager.CaptureNPC(gameObject);
            }
            else
            {
                Debug.Log("Cage is full! NPC destroyed instead.");
                DestroyNPC();
            }
        }
        else
        {
            // Original behavior - destroy or return to pool
            DestroyNPC();
        }
    }
    
    private void DestroyNPC()
    {
        NPCPoolObject poolObj = GetComponent<NPCPoolObject>();
        if (poolObj != null)
        {
            poolObj.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}