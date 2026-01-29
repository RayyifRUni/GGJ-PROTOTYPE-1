using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    [SerializeField] private float padding = 0.5f;
    private EdgeCollider2D edgeCollider;
    
    private void Start()
    {
        CreateBoundaries();
    }
    
    private void CreateBoundaries()
    {
        Camera cam = Camera.main;
        
        if (cam == null || !cam.orthographic)
        {
            Debug.LogError("Main Camera not found or not orthographic!");
            return;
        }
        
        // Calculate screen bounds
        float height = cam.orthographicSize * 2;
        float width = height * cam.aspect;
        
        float halfWidth = width / 2 - padding;
        float halfHeight = height / 2 - padding;
        
        // Create edge collider
        edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        
        // Define boundary points (rectangle)
        Vector2[] points = new Vector2[5];
        points[0] = new Vector2(-halfWidth, halfHeight);    // Top-left
        points[1] = new Vector2(halfWidth, halfHeight);     // Top-right
        points[2] = new Vector2(halfWidth, -halfHeight);    // Bottom-right
        points[3] = new Vector2(-halfWidth, -halfHeight);   // Bottom-left
        points[4] = new Vector2(-halfWidth, halfHeight);    // Back to start
        
        edgeCollider.points = points;
        
        gameObject.tag = "Boundary";
        
        Debug.Log($"Boundaries created: {halfWidth * 2} x {halfHeight * 2}");
    }
    
    private void OnDrawGizmos()
    {
        if (edgeCollider != null)
        {
            Gizmos.color = Color.yellow;
            Vector2[] points = edgeCollider.points;
            
            for (int i = 0; i < points.Length - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }
    }
}