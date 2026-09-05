using UnityEngine;
using UnityEngine.InputSystem; // add this

public class DragObject2D : MonoBehaviour
{
    private Vector3 offset;
    private Camera cam;
    protected bool isDragging = false;

    protected void Awake()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        StartDragging();
    }

    public void StartDragging()
    {
        offset = transform.position - GetMouseWorldPos();
        isDragging = true;
    }

    protected virtual void StopDragging()
    {
        isDragging = false;
    }

    void Update()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                StopDragging();
            }
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        return GetMouseWorldPosition(cam);
    }

    public static Vector3 GetMouseWorldPosition(Camera cam)
    {
        Vector3 mousePoint = Mouse.current.position.ReadValue();
        mousePoint.z = -cam.transform.position.z;
        return cam.ScreenToWorldPoint(mousePoint);
    }

}