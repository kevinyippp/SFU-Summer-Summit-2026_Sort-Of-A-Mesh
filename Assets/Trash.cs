using System;
using UnityEngine;

public class Trash : DragObject2D
{
    [SerializeField] public TrashType typeOfTrash;

    public TrashType Type => typeOfTrash;

    private TrashCan currentTrashCan;

    void OnMouseUp()
    {
        isDragging = false;

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