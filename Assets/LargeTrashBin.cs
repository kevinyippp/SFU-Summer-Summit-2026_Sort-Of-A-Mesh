using UnityEngine;

public class LargeTrashBin : MonoBehaviour
{
    public static LargeTrashBin Instance { get; private set; }

    [SerializeField] private float spawnRadius = 2.8f;

    private Camera cam;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
