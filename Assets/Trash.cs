using UnityEngine;

public class Trash : DragObject2D
{
    [SerializeField] public TrashType typeOfTrash;

    public TrashType Type => typeOfTrash;

    private TrashCan currentTrashCan;
    private SpriteRenderer spriteRenderer;
    public string spriteName;
    [SerializeField] public AudioClip trashDropSound;

    private void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        updateSprite();
        
    }

    public void SetTrashType(TrashType newType, string spriteName = null)
    {
        typeOfTrash = newType;
        updateSprite(newType, spriteName);

    }

    private void CapObjectSize()
    {
        float maxSize = 4f;

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        float largestDimension = Mathf.Max(spriteSize.x, spriteSize.y);

        if (largestDimension > maxSize)
        {
            float scale = maxSize / largestDimension;
            transform.localScale = Vector3.one * scale;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    string cleanItemName(string itemName) {
        int underscoreIndex = itemName.LastIndexOf('_');
        if (underscoreIndex >= 0)
        {
            string suffix = itemName.Substring(underscoreIndex + 1);

            if (int.TryParse(suffix, out _))
            {
                itemName = itemName.Substring(0, underscoreIndex);
            }
        }
        return itemName;
    }

    void LoadDropSound(string itemName)
    {   
        itemName = cleanItemName(itemName);

        string folder = typeOfTrash switch
        {
            TrashType.Garbage => "Sounds/Trash/Garbage",
            TrashType.Organic => "Sounds/Trash/Organic",
            TrashType.Recyclable => "Sounds/Trash/Recyclable",
            TrashType.Paper => "Sounds/Trash/Paper",
            _ => ""
        };

        trashDropSound = Resources.Load<AudioClip>($"{folder}/{itemName}");

        if (trashDropSound == null)
        {
            Debug.LogWarning(
                $"No drop sound found for {itemName} at Resources/{folder}/{itemName}"
            );
        }
    }


    private void updateSprite(TrashType? trashType = null, string newSpriteName = null)
    {
        TrashType selectedTrashType = trashType ?? typeOfTrash;

        string folder = selectedTrashType switch
        {
            TrashType.Garbage => "Trash/Garbage",
            TrashType.Organic => "Trash/Organic",
            TrashType.Recyclable => "Trash/Recyclable",
            TrashType.Paper => "Trash/Paper",
            _ => ""
        };

        Sprite[] sprites = Resources.LoadAll<Sprite>(folder);

        Sprite selectedSprite = null;  

        if (!string.IsNullOrEmpty(newSpriteName))
        {
            selectedSprite = System.Array.Find(
                sprites,
                sprite => sprite.name == newSpriteName
            );

            if (selectedSprite == null)
            {
                Debug.LogError(
                    $"Could not find sprite '{newSpriteName}' in Resources/{folder}"
                );
                return;
            }
        }
        else
        {
            selectedSprite = sprites[Random.Range(0, sprites.Length)];
        }

        spriteName = cleanItemName(selectedSprite.name);
        spriteRenderer.sprite = selectedSprite;

        LoadDropSound(selectedSprite.name);

        BoxCollider2D col = GetComponent<BoxCollider2D>();

        CapObjectSize();

        if (col != null)
        {
            col.size = selectedSprite.bounds.size;
            col.offset = selectedSprite.bounds.center;
        }
    }

    protected override void StopDragging()
    {
        base.StopDragging();

        if (currentTrashCan != null)
        {
            currentTrashCan.HandleTrashDropped(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrashCan trashCan = other.GetComponent<TrashCan>();
        if (trashCan != null)
        {
            currentTrashCan = trashCan;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TrashCan trashCan = other.GetComponent<TrashCan>();

        if (trashCan != null && trashCan == currentTrashCan)
        {
            currentTrashCan = null;
        }
    }
}