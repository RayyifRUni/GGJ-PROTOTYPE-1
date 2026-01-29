using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [Header("Billboard Settings")]
    [SerializeField] private bool lockX = true;
    [SerializeField] private bool lockY = true;
    [SerializeField] private bool lockZ = false;
    
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
    }
    
    private void LateUpdate()
    {
        if (mainCamera == null) return;
        
        // Make sprite face camera
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraUp = mainCamera.transform.up;
        
        // Lock specific axes if needed
        if (lockX) cameraForward.x = 0;
        if (lockY) cameraForward.y = 0;
        if (lockZ) cameraForward.z = 0;
        
        // Face the camera
        transform.rotation = Quaternion.LookRotation(cameraForward, cameraUp);
    }
}