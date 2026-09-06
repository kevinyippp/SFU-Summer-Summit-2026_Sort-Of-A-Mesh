using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public static TrashSpawner Instance;

    [SerializeField] public GameObject trashPrefab;


    [SerializeField] private Transform previewPosition;
    [SerializeField] private GameObject previewPrefab;

    private TrashType nextTrashType;
    private GameObject previewInstance;

    void Awake()
    {
        Instance = this;

        GenerateNextTrash();
    }

    void GenerateNextTrash()
    {
        nextTrashType = (TrashType)Random.Range(
            0,
            System.Enum.GetValues(typeof(TrashType)).Length
        );

        UpdatePreview();
    }

    void UpdatePreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        previewInstance = Instantiate(
            previewPrefab,
            previewPosition.position,
            Quaternion.identity
        );

        Trash previewTrash = previewInstance.GetComponent<Trash>();

        if (previewTrash != null)
        {
            previewTrash.SetTrashType(nextTrashType);
        }

        SpriteRenderer renderer = previewInstance.GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            renderer.sortingOrder = -1;
        }
    }

    public Trash Spawn(float x, float y)
    {
        Vector3 spawnPos = new Vector3(x, y, 0f);
        GameObject instance = Instantiate(trashPrefab, spawnPos, Quaternion.identity);

        Trash trash = instance.GetComponent<Trash>();

        Trash previewTrash = previewInstance.GetComponent<Trash>();

        trash.SetTrashType(nextTrashType, previewTrash.spriteName + "_0");
        GenerateNextTrash();

        return trash;
    }
}
