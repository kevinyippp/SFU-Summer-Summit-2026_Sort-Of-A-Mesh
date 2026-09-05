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

    // Pick a random point inside the allowed circle
    Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;

    instance.transform.position = new Vector3(
        spawnPoint.position.x + randomOffset.x,
        spawnPoint.position.y + randomOffset.y,
        spawnPoint.position.z
    );
}


}