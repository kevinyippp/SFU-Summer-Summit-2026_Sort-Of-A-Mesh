using UnityEngine;
using UnityEngine.InputSystem; // add this

public class DragObject2D : MonoBehaviour
{
    private Vector3 offset;
    private Camera cam;
    private bool isDragging = false;
    private bool inTrashCan = false;

    void Start()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        offset = transform.position - GetMouseWorldPos();
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (inTrashCan)
        {
            Score.ScoreInstance.AddPoint();
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
        
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Mouse.current.position.ReadValue();
        mousePoint.z = -cam.transform.position.z;
        return cam.ScreenToWorldPoint(mousePoint);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<TrashCan>() != null)
        {
            inTrashCan = true;
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<TrashCan>() != null)
        {
            inTrashCan = false;
        }
    }
}