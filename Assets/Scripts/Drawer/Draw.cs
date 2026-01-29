using UnityEngine;
using System.Collections.Generic;

public class Draw : MonoBehaviour
{
    private LineRenderer currentLine;
    private Vector3 previousPosition;
    private List<Vector3> linePoints = new List<Vector3>();
    
    [Header("Drawing Settings")]
    [SerializeField] private float minDistance = 0.1f;
    [SerializeField, Range(0.1f, 2f)] private float width = 0.5f;
    [SerializeField] private Color lineColor = Color.white;
    
    [Header("Circle Detection")]
    [SerializeField] private float circleThreshold = 0.8f; // How close to perfect circle (0-1)
    [SerializeField] private int minPointsForCircle = 20; // Minimum points to form circle
    [SerializeField] private float maxCircleCheckDistance = 1f; // How close end must be to start
    
    [Header("Pool Settings")]
    [SerializeField] private int maxLines = 1; // Only 1 line at a time
    
    private GameObject linePoolObject;
    
    private void Start()
    {
        // Create pooled line object
        CreatePooledLine();
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartNewLine();
        }
        
        if (Input.GetMouseButton(0) && currentLine != null)
        {
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentPosition.z = -1f;
            
            if (Vector3.Distance(currentPosition, previousPosition) > minDistance)
            {
                AddPoint(currentPosition);
                previousPosition = currentPosition;
            }
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            FinishDrawing();
        }
    }
    
    private void CreatePooledLine()
    {
        linePoolObject = new GameObject("Line");
        currentLine = linePoolObject.AddComponent<LineRenderer>();
        
        // Setup LineRenderer
        currentLine.startWidth = width;
        currentLine.endWidth = width;
        currentLine.useWorldSpace = true;
        currentLine.sortingOrder = 10;
        
        // Material
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = lineColor;
        currentLine.material = mat;
        
        linePoolObject.SetActive(false);
    }
    
    private void StartNewLine()
    {
        linePoints.Clear();
        currentLine.positionCount = 0;
        linePoolObject.SetActive(true);
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = -1f;
        previousPosition = mousePos;
        
        AddPoint(mousePos);
    }
    
    private void AddPoint(Vector3 point)
    {
        linePoints.Add(point);
        currentLine.positionCount++;
        currentLine.SetPosition(currentLine.positionCount - 1, point);
    }
    
    private void FinishDrawing()
    {
        if (linePoints.Count >= minPointsForCircle)
        {
            bool isCircle = CheckIfCircle();
            
            if (isCircle)
            {
                Debug.Log("Circle detected! Catching...");
                OnCircleDrawn();
            }
            else
            {
                Debug.Log("Not a circle, try again!");
            }
        }
        
        // Reset line after short delay
        Invoke("ResetLine", 0.2f);
    }
    
    private bool CheckIfCircle()
    {
        // Check 1: End point must be close to start point (closed shape)
        float distanceToStart = Vector3.Distance(linePoints[0], linePoints[linePoints.Count - 1]);
        if (distanceToStart > maxCircleCheckDistance)
        {
            Debug.Log("Not closed! Distance: " + distanceToStart);
            return false;
        }
        
        // Check 2: Calculate center and check if all points are roughly same distance from center
        Vector3 center = CalculateCenter();
        float averageRadius = CalculateAverageRadius(center);
        
        // Check variance - how much points deviate from average radius
        float variance = CalculateRadiusVariance(center, averageRadius);
        float normalizedVariance = 1f - Mathf.Clamp01(variance / averageRadius);
        
        Debug.Log($"Circle Score: {normalizedVariance} (need {circleThreshold})");
        
        return normalizedVariance >= circleThreshold;
    }
    
    private Vector3 CalculateCenter()
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 point in linePoints)
        {
            sum += point;
        }
        return sum / linePoints.Count;
    }
    
    private float CalculateAverageRadius(Vector3 center)
    {
        float totalDistance = 0f;
        foreach (Vector3 point in linePoints)
        {
            totalDistance += Vector3.Distance(center, point);
        }
        return totalDistance / linePoints.Count;
    }
    
    private float CalculateRadiusVariance(Vector3 center, float averageRadius)
    {
        float totalVariance = 0f;
        foreach (Vector3 point in linePoints)
        {
            float distance = Vector3.Distance(center, point);
            totalVariance += Mathf.Abs(distance - averageRadius);
        }
        return totalVariance / linePoints.Count;
    }
    
    private void OnCircleDrawn()
    {
        // THIS IS WHERE YOU CATCH!
        Vector3 center = CalculateCenter();
        float radius = CalculateAverageRadius(center);
        
        // Example: Check if any catchable objects are inside the circle
        Collider2D[] objectsInCircle = Physics2D.OverlapCircleAll(center, radius);
        
        foreach (Collider2D obj in objectsInCircle)
        {
            if (obj.CompareTag("Catchable")) // Tag your catchable objects!
            {
                Debug.Log("Caught: " + obj.name);
                // Add your catch logic here
                // obj.GetComponent<CatchableObject>().Catch();
            }
        }
        
        // Visual feedback - make line green for success
        currentLine.material.color = Color.green;
    }
    
    private void ResetLine()
    {
        linePoolObject.SetActive(false);
        linePoints.Clear();
        currentLine.positionCount = 0;
        currentLine.material.color = lineColor; // Reset color
    }
    
    // Visualize detection radius in editor
    private void OnDrawGizmos()
    {
        if (linePoints.Count > minPointsForCircle)
        {
            Vector3 center = CalculateCenter();
            float radius = CalculateAverageRadius(center);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, radius);
        }
    }
}