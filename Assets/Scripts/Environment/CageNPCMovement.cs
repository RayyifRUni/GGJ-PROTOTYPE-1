using UnityEngine;

public class CageNPCMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 180f;
    
    [Header("Wandering")]
    [SerializeField] private float changeDirectionTime = 2f;
    
    private Vector2 moveDirection;
    private float changeDirectionTimer;
    private CageManager cageManager;
    private Vector2 cageBounds;
    
    public void Initialize(CageManager manager)
    {
        cageManager = manager;
        cageBounds = manager.CageSize / 2f;
        
        ChooseRandomDirection();
        changeDirectionTimer = changeDirectionTime;
    }
    
    private void Update()
    {
        if (cageManager == null) return;
        
        Move();
        CheckCageBoundaries();
        ChangeDirectionRandomly();
        UpdateRotation();
    }
    
    private void Move()
    {
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
        transform.localPosition += movement;
    }
    
    private void ChooseRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;
    }
    
    private void ChangeDirectionRandomly()
    {
        changeDirectionTimer -= Time.deltaTime;
        
        if (changeDirectionTimer <= 0)
        {
            ChooseRandomDirection();
            changeDirectionTimer = changeDirectionTime + Random.Range(-0.5f, 0.5f);
        }
    }
    
    private void CheckCageBoundaries()
    {
        Vector3 localPos = transform.localPosition;
        bool bounced = false;
        
        // Bounce off cage walls
        if (localPos.x > cageBounds.x || localPos.x < -cageBounds.x)
        {
            moveDirection.x = -moveDirection.x;
            localPos.x = Mathf.Clamp(localPos.x, -cageBounds.x, cageBounds.x);
            bounced = true;
        }
        
        if (localPos.y > cageBounds.y || localPos.y < -cageBounds.y)
        {
            moveDirection.y = -moveDirection.y;
            localPos.y = Mathf.Clamp(localPos.y, -cageBounds.y, cageBounds.y);
            bounced = true;
        }
        
        if (bounced)
        {
            transform.localPosition = localPos;
            changeDirectionTimer = changeDirectionTime;
        }
    }
    
    private void UpdateRotation()
    {
        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}