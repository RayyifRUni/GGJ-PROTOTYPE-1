using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 180f;
    
    [Header("Wandering Behavior")]
    [SerializeField] private float changeDirectionTime = 3f;
    [SerializeField] private float boundaryPadding = 2f;
    
    [Header("Boundary Behavior")]
    [SerializeField] private BoundaryMode boundaryMode = BoundaryMode.WrapAround;
    
    public enum BoundaryMode
    {
        Bounce,      // Bounce off edges
        WrapAround   // Teleport to opposite side
    }
    
    private Vector2 moveDirection;
    private float changeDirectionTimer;
    private Camera mainCamera;
    private Vector2 screenBounds;
    
    [Header("Visual Adjustment")]
    [SerializeField] private bool adjustForCameraAngle = true;
    [SerializeField] private float verticalMovementScale = 0.6f;
    
    private void Start()
    {
        mainCamera = Camera.main;
        CalculateScreenBounds();
        ChooseRandomDirection();
        changeDirectionTimer = changeDirectionTime;
    }
    
    private void Update()
    {
        Move();
        CheckBoundaries();
        ChangeDirectionRandomly();
        UpdateVisualRotation();
    }
    
    private void CalculateScreenBounds()
    {
        if (mainCamera != null && mainCamera.orthographic)
        {
            float height = mainCamera.orthographicSize * 2;
            float width = height * mainCamera.aspect;
            screenBounds = new Vector2(width / 2 + boundaryPadding, height / 2 + boundaryPadding);
            
            Debug.Log($"Screen Bounds: {screenBounds.x} x {screenBounds.y}");
        }
    }
    
    private void Move()
    {
        Vector2 adjustedDirection = moveDirection;
        
        if (adjustForCameraAngle)
        {
            adjustedDirection.y *= verticalMovementScale;
        }
        
        transform.Translate(adjustedDirection * moveSpeed * Time.deltaTime, Space.World);
    }
    
    private void ChooseRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
    }
    
    private void ChangeDirectionRandomly()
    {
        changeDirectionTimer -= Time.deltaTime;
        
        if (changeDirectionTimer <= 0)
        {
            ChooseRandomDirection();
            changeDirectionTimer = changeDirectionTime + Random.Range(-1f, 1f);
        }
    }
    
    private void CheckBoundaries()
    {
        Vector3 pos = transform.position;
        bool changed = false;
        
        if (boundaryMode == BoundaryMode.WrapAround)
        {
            // WRAP AROUND - teleport to opposite side
            if (pos.x > screenBounds.x)
            {
                pos.x = -screenBounds.x;
                changed = true;
            }
            else if (pos.x < -screenBounds.x)
            {
                pos.x = screenBounds.x;
                changed = true;
            }
            
            if (pos.y > screenBounds.y)
            {
                pos.y = -screenBounds.y;
                changed = true;
            }
            else if (pos.y < -screenBounds.y)
            {
                pos.y = screenBounds.y;
                changed = true;
            }
        }
        else
        {
            // BOUNCE - reflect direction
            if (pos.x > screenBounds.x)
            {
                moveDirection.x = -Mathf.Abs(moveDirection.x);
                pos.x = screenBounds.x;
                changed = true;
            }
            else if (pos.x < -screenBounds.x)
            {
                moveDirection.x = Mathf.Abs(moveDirection.x);
                pos.x = -screenBounds.x;
                changed = true;
            }
            
            if (pos.y > screenBounds.y)
            {
                moveDirection.y = -Mathf.Abs(moveDirection.y);
                pos.y = screenBounds.y;
                changed = true;
            }
            else if (pos.y < -screenBounds.y)
            {
                moveDirection.y = Mathf.Abs(moveDirection.y);
                pos.y = -screenBounds.y;
                changed = true;
            }
            
            if (changed)
            {
                moveDirection = moveDirection.normalized;
            }
        }
        
        if (changed)
        {
            transform.position = pos;
        }
    }
    
    private void UpdateVisualRotation()
    {
        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    // Public methods for other scripts
    public Vector2 GetMoveDirection()
    {
        return moveDirection;
    }
    
    public void SetMoveDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
        changeDirectionTimer = changeDirectionTime;
    }
    
    // Visualize boundaries
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            Gizmos.color = Color.cyan;
            
            Vector3 topRight = new Vector3(screenBounds.x, screenBounds.y, 0);
            Vector3 topLeft = new Vector3(-screenBounds.x, screenBounds.y, 0);
            Vector3 bottomRight = new Vector3(screenBounds.x, -screenBounds.y, 0);
            Vector3 bottomLeft = new Vector3(-screenBounds.x, -screenBounds.y, 0);
            
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
    }
}
