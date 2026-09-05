using UnityEngine;

public class TrashBin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.name + " entered the trash can!");
    }
}