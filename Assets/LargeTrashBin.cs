using UnityEngine;

public class LargeTrashBin : MonoBehaviour
{
    public static LargeTrashBin Instance { get; private set; }

    private Camera cam;
    private CircleCollider2D clickArea;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
        clickArea = GetComponent<CircleCollider2D>();
    }

    void OnMouseDown()
    {
        if (GameManager.IsGameOver)
            return;

        SpawnTrash();
    }

    private void SpawnTrash()
    {
        Vector3 cursorWorldPos = DragObject2D.GetMouseWorldPosition(cam);

        Trash trash = TrashSpawner.Instance.Spawn(cursorWorldPos.x, cursorWorldPos.y);
        trash.StartDragging();
    }

    private void OnDrawGizmos()
    {
        CircleCollider2D circle = clickArea != null ? clickArea : GetComponent<CircleCollider2D>();

        if (circle == null)
            return;

        float worldRadius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + circle.offset, worldRadius);
    }
}
