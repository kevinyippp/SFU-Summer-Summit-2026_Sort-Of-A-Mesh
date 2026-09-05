using System;
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
        UpdateColour();
    }

    public void SetTrashType(TrashType newType)
    {
        typeOfTrash = newType;
        UpdateColour();
    }

    private void UpdateColour()
    {
        switch (typeOfTrash)
        {
            case TrashType.Organic:
                spriteRenderer.color = Color.green;
                break;

            case TrashType.Recyclable:
                spriteRenderer.color = Color.blue;
                break;

            case TrashType.Hazardous:
                spriteRenderer.color = Color.red;
                break;

            case TrashType.General:
                spriteRenderer.color = Color.gray;
                break;
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