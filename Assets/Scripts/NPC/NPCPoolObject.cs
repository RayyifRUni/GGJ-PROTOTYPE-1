using UnityEngine;

public class NPCPoolObject : MonoBehaviour
{
    private NPCPool pool;
    private GameObject prefabReference;
    
    public void Initialize(NPCPool poolReference, GameObject prefab)
    {
        pool = poolReference;
        prefabReference = prefab;
    }
    
    public void ReturnToPool()
    {
        if (pool != null)
        {
            pool.ReturnToPool(gameObject, prefabReference);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}