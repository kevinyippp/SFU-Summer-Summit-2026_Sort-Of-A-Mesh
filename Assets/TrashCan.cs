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
        if (trash == null || GameManager.IsGameOver)
        {
            return;
        }

        bool isCorrect =
            trash.Type == typeOfTrash;

        int points = isCorrect ? 1 : -2;

        if (Score.ScoreInstance != null)
        {
            Score.ScoreInstance.AddPoints(points);
        }
        else
        {
            Debug.LogError(
                "No active Score was found in the scene."
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

        if (AudioManager.Instance != null)
        {
            // Delay is to simulate the time it takes to hit the 'bottom' of the trashcan
            AudioManager.Instance.PlaySound(trash.trashDropSound, 0.45f);

            if (isCorrect)
            {
                AudioManager.Instance.PlaySound(AudioManager.Instance.correctDropSound);
            } else
            {
                AudioManager.Instance.PlaySound(AudioManager.Instance.incorrectDropSound);
            }
        }
        else
        {
            Debug.LogWarning(
                "AudioManager was not found."
            );
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
