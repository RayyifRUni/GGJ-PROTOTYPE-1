using UnityEngine;
using System.Collections.Generic;

public class NPCPool : MonoBehaviour
{
    [System.Serializable]
    public class NPCType
    {
        public string typeName;
        public GameObject[] variants; // 4 variants per type
    }
    
    [Header("Pool Settings")]
    [SerializeField] private NPCType[] npcTypes; // 3 types
    [SerializeField] private int poolSizePerVariant = 3; // Pre-instantiate 3 of each
    
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private Transform poolParent;
    
    private void Awake()
    {
        // Create pool parent
        poolParent = new GameObject("NPC_Pool").transform;
        poolParent.SetParent(transform);
        
        // Initialize pools
        InitializePools();
    }
    
    private void InitializePools()
    {
        foreach (NPCType type in npcTypes)
        {
            foreach (GameObject variant in type.variants)
            {
                if (variant == null) continue;
                
                Queue<GameObject> objectQueue = new Queue<GameObject>();
                
                for (int i = 0; i < poolSizePerVariant; i++)
                {
                    GameObject obj = Instantiate(variant, poolParent);
                    obj.SetActive(false);
                    objectQueue.Enqueue(obj);
                }
                
                poolDictionary.Add(variant, objectQueue);
            }
        }
        
        Debug.Log($"Pool initialized with {poolDictionary.Count} variant types");
    }
    
    public GameObject SpawnFromPool(Vector3 position, Quaternion rotation)
    {
        // Pick random type
        NPCType randomType = npcTypes[Random.Range(0, npcTypes.Length)];
        
        // Pick random variant from type
        GameObject randomVariant = randomType.variants[Random.Range(0, randomType.variants.Length)];
        
        if (randomVariant == null || !poolDictionary.ContainsKey(randomVariant))
        {
            Debug.LogError("Variant not found in pool!");
            return null;
        }
        
        Queue<GameObject> queue = poolDictionary[randomVariant];
        
        GameObject objectToSpawn;
        
        // Get from pool or create new if empty
        if (queue.Count > 0)
        {
            objectToSpawn = queue.Dequeue();
        }
        else
        {
            objectToSpawn = Instantiate(randomVariant, poolParent);
            Debug.Log("Pool empty, created new instance");
        }
        
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);
        
        // Add return to pool component
        NPCPoolObject poolObj = objectToSpawn.GetComponent<NPCPoolObject>();
        if (poolObj == null)
        {
            poolObj = objectToSpawn.AddComponent<NPCPoolObject>();
        }
        poolObj.Initialize(this, randomVariant);
        
        return objectToSpawn;
    }
    
    public void ReturnToPool(GameObject obj, GameObject prefabReference)
    {
        obj.SetActive(false);
        
        if (poolDictionary.ContainsKey(prefabReference))
        {
            poolDictionary[prefabReference].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}