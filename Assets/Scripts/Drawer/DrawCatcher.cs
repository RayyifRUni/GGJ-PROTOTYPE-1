using UnityEngine;
using System.Collections.Generic;

public class DrawCatcher : MonoBehaviour
{
    private LineRenderer currentLine;
    private Vector3 previousPosition;
    private List<Vector3> linePoints = new List<Vector3>();
    
    [Header("Drawing Settings")]
    [SerializeField] private float minDistance = 0.1f;
    [SerializeField, Range(0.1f, 2f)] private float width = 0.5f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private float drawingZPosition = -1f; // Z position for drawing
    
    [Header("Catching Settings")]
    [SerializeField] private int minPointsForCatch = 10;
    
    private GameObject linePoolObject;
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
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
            Vector3 currentPosition = GetMouseWorldPosition();
            
            if (Vector3.Distance(currentPosition, previousPosition) > minDistance)
            {
                AddPoint(currentPosition);
                previousPosition = currentPosition;
                CheckForIntersection();
            }
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            FinishDrawing();
        }
    }
    
    // FIXED: Proper mouse position for angled camera
    private Vector3 GetMouseWorldPosition()
    {
        // Create a plane at the drawing Z position
        Plane drawPlane = new Plane(Vector3.forward, new Vector3(0, 0, drawingZPosition));
        
        // Cast ray from mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // Find intersection with plane
        if (drawPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        // Fallback (shouldn't happen)
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = drawingZPosition;
        return mousePos;
    }
    
    private void CreatePooledLine()
    {
        linePoolObject = new GameObject("DrawnLine");
        currentLine = linePoolObject.AddComponent<LineRenderer>();
        
        currentLine.startWidth = width;
        currentLine.endWidth = width;
        currentLine.useWorldSpace = true;
        currentLine.sortingOrder = 10;
        
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
        
        previousPosition = GetMouseWorldPosition();
        AddPoint(previousPosition);
    }
    
    private void AddPoint(Vector3 point)
    {
        linePoints.Add(point);
        currentLine.positionCount++;
        currentLine.SetPosition(currentLine.positionCount - 1, point);
    }
    
    private void CheckForIntersection()
    {
        if (linePoints.Count < minPointsForCatch) return;
        
        Vector3 newStart = linePoints[linePoints.Count - 2];
        Vector3 newEnd = linePoints[linePoints.Count - 1];
        
        for (int i = 0; i < linePoints.Count - 4; i++)
        {
            Vector3 oldStart = linePoints[i];
            Vector3 oldEnd = linePoints[i + 1];
            
            if (LineSegmentsIntersect(newStart, newEnd, oldStart, oldEnd))
            {
                Debug.Log("Line closed! Catching...");
                OnLineClosed();
                return;
            }
        }
    }
    
    private bool LineSegmentsIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        float x1 = p1.x, y1 = p1.y;
        float x2 = p2.x, y2 = p2.y;
        float x3 = p3.x, y3 = p3.y;
        float x4 = p4.x, y4 = p4.y;
        
        float denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (Mathf.Abs(denom) < 0.0001f) return false;
        
        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denom;
        
        return (t >= 0 && t <= 1 && u >= 0 && u <= 1);
    }
    
    private void FinishDrawing()
    {
        Invoke("ResetLine", 0.3f);
    }
    
    private void OnLineClosed()
    {
        currentLine.material.color = successColor;
        CatchObjectsInArea(linePoints);
        Invoke("ResetLine", 0.5f);
    }
    
    private void CatchObjectsInArea(List<Vector3> areaPoints)
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 point in areaPoints)
        {
            center += point;
        }
        center /= areaPoints.Count;
        
        float maxRadius = 0f;
        foreach (Vector3 point in areaPoints)
        {
            float dist = Vector3.Distance(center, point);
            if (dist > maxRadius) maxRadius = dist;
        }
        
        Collider2D[] objectsInArea = Physics2D.OverlapCircleAll(center, maxRadius);
        
        foreach (Collider2D obj in objectsInArea)
        {
            if (obj.CompareTag("Catchable"))
            {
                if (IsPointInPolygon(obj.transform.position, areaPoints))
                {
                    Debug.Log("Caught: " + obj.name);
                    CatchObject(obj.gameObject);
                }
            }
        }
    }
    
    private bool IsPointInPolygon(Vector3 point, List<Vector3> polygon)
    {
        int intersections = 0;
        
        for (int i = 0; i < polygon.Count - 1; i++)
        {
            if (RayIntersectsSegment(point, polygon[i], polygon[i + 1]))
            {
                intersections++;
            }
        }
        
        return intersections % 2 == 1;
    }
    
    private bool RayIntersectsSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        if (a.y > b.y)
        {
            Vector3 temp = a;
            a = b;
            b = temp;
        }
        
        if (point.y < a.y || point.y > b.y) return false;
        if (point.x > Mathf.Max(a.x, b.x)) return false;
        if (point.x < Mathf.Min(a.x, b.x)) return true;
        
        float red = (point.y - a.y) / (b.y - a.y);
        float blue = (point.x - a.x) / (b.x - a.x);
        
        return blue >= red;
    }
    
    private void CatchObject(GameObject obj)
    {
        Catchable catchable = obj.GetComponent<Catchable>();
        if (catchable != null)
        {
            catchable.OnCaught();
        }
        else
        {
            Destroy(obj);
        }
    }
    
    private void ResetLine()
    {
        linePoolObject.SetActive(false);
        linePoints.Clear();
        currentLine.positionCount = 0;
        currentLine.material.color = lineColor;
    }
}