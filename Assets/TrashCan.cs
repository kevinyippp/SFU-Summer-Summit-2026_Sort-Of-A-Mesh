using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [SerializeField] public TrashType typeOfTrash;

    public TrashType Type => typeOfTrash;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.name + " entered the trash can!");
    }

    public void HandleTrashDropped(Trash trash)
    {
        int points = -2;
        if (trash.Type == typeOfTrash) points = 1;

        Score.ScoreInstance.AddPoints(points);

        string feedbackText = points > 0 ? $"+{points}" : points.ToString();
        TextSpawner.Instance.Spawn(feedbackText, transform.position.x, transform.position.y);

        Destroy(trash.gameObject);
    }
}