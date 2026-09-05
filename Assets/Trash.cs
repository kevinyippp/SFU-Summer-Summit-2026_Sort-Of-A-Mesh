using System;
using UnityEngine;

public class Trash : DragObject2D
{
    [SerializeField] public TrashType typeOfTrash;
    private TrashCan currentTrashCan;

    private void Start()
    {
        Debug.Log("Start is called");
    }

    # overwrites dragObject behaviour
    void OnMouseUp()
    {
        isDragging = false;

        if (currentTrashCan != null)
        {
            int points = -2;
            if (typeOfTrash == currentTrashCan.Type) points = 1;
            Score.ScoreInstance.AddPoints(points);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrashCan trashCan = other.GetComponent<TrashCan>();
        if (other.GetComponent<TrashCan>() != null)
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
