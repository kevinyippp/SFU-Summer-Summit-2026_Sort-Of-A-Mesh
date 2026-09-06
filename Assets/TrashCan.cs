using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [SerializeField] private TrashType typeOfTrash;

    public TrashType Type => typeOfTrash;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(
            other.gameObject.name +
            " entered the trash can!"
        );
    }

    public void HandleTrashDropped(Trash trash)
    {
        if (trash == null)
        {
            return;
        }

        bool isCorrect =
            trash.Type == typeOfTrash;

        int points = isCorrect ? 1 : -2;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessTrashResult(points);
        }
        else
        {
            Debug.LogError(
                "No active GameManager was found in the scene."
            );
        }

        if (TextSpawner.Instance != null)
        {
            string feedbackText =
                points > 0
                    ? "+" + points
                    : points.ToString();

            TextSpawner.Instance.Spawn(
                feedbackText,
                transform.position.x,
                transform.position.y
            );
        }
        else
        {
            Debug.LogWarning(
                "TextSpawner was not found."
            );
        }

        if (isCorrect)
        {
            if (TrashToScoreAnimation.Instance != null)
            {
                TrashToScoreAnimation.Instance.PlayEffect(trash);
            }
            else
            {
                Debug.LogWarning(
                    "TrashToScoreAnimation was not found."
                );
            }
        }

        if (!isCorrect)
        {
            WrongTrashBurst burst = GetComponent<WrongTrashBurst>();
            if (burst == null)
                burst = gameObject.AddComponent<WrongTrashBurst>();
            burst.Play(transform.position);
        }

        TrashDropAnimation dropAnimation =
            trash.GetComponent<TrashDropAnimation>();

        if (dropAnimation != null)
        {
            dropAnimation.Play(transform.position);
        }
        else
        {
            Debug.LogWarning(
                trash.gameObject.name +
                " does not have TrashDropAnimation."
            );

            Destroy(trash.gameObject);
        }
    }
}
