using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public static TrashSpawner Instance;

    [SerializeField] private GameObject trashPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRadius = 2.8f;

    void Awake()
    {
        Instance = this;
        
        int amountToSpawn = 5;

        for (int i = 0; i < amountToSpawn; i++)
        {
            Spawn();
        }
    }
    public void Spawn()
{
    GameObject instance = Instantiate(
        trashPrefab,
        spawnPoint.position,
        Quaternion.identity
    );

    Trash trash = instance.GetComponent<Trash>();

    TrashType randomType = (TrashType)Random.Range(
        0,
        System.Enum.GetValues(typeof(TrashType)).Length
    );

    trash.SetTrashType(randomType);

    // Get the object's collider
    Collider2D collider = instance.GetComponent<Collider2D>();

    // Use the largest extent as the object's radius
    float objectRadius = Mathf.Max(
        collider.bounds.extents.x,
        collider.bounds.extents.y
    );

    // Make sure the entire object stays inside the circle
    float allowedRadius = spawnRadius - objectRadius;

    // Pick a random point inside the allowed circle
    Vector2 randomOffset = Random.insideUnitCircle * allowedRadius;

    instance.transform.position = new Vector3(
        spawnPoint.position.x + randomOffset.x,
        spawnPoint.position.y + randomOffset.y,
        spawnPoint.position.z
    );
}


}
