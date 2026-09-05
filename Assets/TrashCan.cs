using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [SerializeField] public TrashType typeOfTrash;

    public TrashType Type => typeOfTrash;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.name + " entered the trash can!");
    }
}