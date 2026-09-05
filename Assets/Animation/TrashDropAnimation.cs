using System.Collections;
using UnityEngine;

public class TrashDropAnimation : MonoBehaviour
{
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float fallDistance = 1.5f;
    [SerializeField] private float spinTurns = 2f;

    private bool isPlaying;

    public void Play(Vector3 binPosition)
    {
        if (isPlaying)
            return;

        StartCoroutine(DropAnimation(binPosition));
    }

    private IEnumerator DropAnimation(Vector3 binPosition)
    {
        isPlaying = true;

        Collider2D trashCollider = GetComponent<Collider2D>();

        if (trashCollider != null)
        {
            trashCollider.enabled = false;
        }

        Vector3 startingPosition = transform.position;
        Vector3 startingScale = transform.localScale;
        float startingRotation = transform.eulerAngles.z;

        Vector3 endingPosition = new Vector3(
            binPosition.x,
            binPosition.y - fallDistance,
            startingPosition.z
        );

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / duration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            transform.position = Vector3.Lerp(
                startingPosition,
                endingPosition,
                smoothProgress
            );

            transform.eulerAngles = new Vector3(
                0f,
                0f,
                startingRotation + spinTurns * 360f * progress
            );

            transform.localScale = Vector3.Lerp(
                startingScale,
                Vector3.zero,
                smoothProgress
            );

            yield return null;
        }

        Destroy(gameObject);
    }
}