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
        if (other.GetComponent<TrashCan>() != null)
        {
            currentTrashCan = null;
        }
    }
}