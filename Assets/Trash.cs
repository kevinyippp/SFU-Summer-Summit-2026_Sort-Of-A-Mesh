using UnityEngine;

public class Trash : DragObject2D
{
    [SerializeField] public TrashType typeOfTrash;

    public TrashType Type => typeOfTrash;

    private TrashCan currentTrashCan;
    private SpriteRenderer spriteRenderer;
    [SerializeField] public AudioClip trashDropSound;

    private void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        updateSprite();
        
    }

    public void SetTrashType(TrashType newType)
    {
        typeOfTrash = newType;
        updateSprite();

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

    void LoadDropSound(string itemName)
    {   
        int underscoreIndex = itemName.LastIndexOf('_');
        if (underscoreIndex >= 0)
        {
            string suffix = itemName.Substring(underscoreIndex + 1);

            if (int.TryParse(suffix, out _))
            {
                itemName = itemName.Substring(0, underscoreIndex);
            }
        }

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


    private void updateSprite()
    {
        string folder = typeOfTrash switch
        {
            TrashType.Garbage => "Trash/Garbage",
            TrashType.Organic => "Trash/Organic",
            TrashType.Recyclable => "Trash/Recyclable",
            TrashType.Paper => "Trash/Paper",
            _ => ""
        };

        Sprite[] sprites = Resources.LoadAll<Sprite>(folder);

        if (sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];

            LoadDropSound(spriteRenderer.sprite.name);

            BoxCollider2D col = GetComponent<BoxCollider2D>();

            CapObjectSize();

            if (col != null)
            {
                col.size = spriteRenderer.sprite.bounds.size;
                col.offset = spriteRenderer.sprite.bounds.center;
            }
            
        }
        else
        {
            Debug.LogError($"No sprites found in Resources/{folder}");
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