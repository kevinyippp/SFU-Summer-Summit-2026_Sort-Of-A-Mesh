using UnityEngine;

public class Trash : DragObject2D
{
    [SerializeField] public TrashType typeOfTrash;

    public TrashType Type => typeOfTrash;

    private TrashCan currentTrashCan;
    private SpriteRenderer spriteRenderer;

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

        // Debug.Log(currentTrashCan);
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