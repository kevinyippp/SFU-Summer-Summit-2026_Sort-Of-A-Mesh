using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public static TrashSpawner Instance;

    [SerializeField] private GameObject trashPrefab;

    void Awake()
    {
        Instance = this;
    }

    public Trash Spawn(float x, float y)
    {
        Vector3 spawnPos = new Vector3(x, y, 0f);
        GameObject instance = Instantiate(trashPrefab, spawnPos, Quaternion.identity);

        Trash trash = instance.GetComponent<Trash>();

        TrashType randomType = (TrashType)Random.Range(
            0,
            System.Enum.GetValues(typeof(TrashType)).Length
        );

        trash.SetTrashType(randomType);

        return trash;
    }
}
