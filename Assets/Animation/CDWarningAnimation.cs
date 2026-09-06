using TMPro;
using UnityEngine;

public class CDWarningAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CountdownTimer timer;
    [SerializeField] private RectTransform timerBar;
    [SerializeField] private TMP_Text timerText;

    [Header("Shake")]
    [SerializeField] private float warningTime = 10f;
    [SerializeField] private float minimumShake = 1f;
    [SerializeField] private float maximumShake = 10f;

    [Header("Text Break")]
    [SerializeField] private float breakSpeed = 200f;
    [SerializeField] private float hideDistance = 500f;

    private Vector2 originalBarPosition;
    private bool breakStarted;
    private float breakDistance;

    private void Start()
    {
        originalBarPosition = timerBar.anchoredPosition;
    }

    private void LateUpdate()
    {
        if (timer == null || timerBar == null || timerText == null)
            return;

        if (!timer.HasFinished)
        {
            ResetBreakAnimation();
            UpdateBarShake();
            return;
        }

        timerBar.anchoredPosition = originalBarPosition;
        PlayTextBreakAnimation();
    }

    private void UpdateBarShake()
    {
        if (timer.RemainingTime > warningTime)
        {
            timerBar.anchoredPosition = originalBarPosition;
            return;
        }

        float urgency = 1f -
            Mathf.Clamp01(timer.RemainingTime / warningTime);

        float shakeStrength = Mathf.Lerp(
            minimumShake,
            maximumShake,
            urgency
        );

        Vector2 shakeOffset =
            Random.insideUnitCircle * shakeStrength;

        timerBar.anchoredPosition =
            originalBarPosition + shakeOffset;
    }

    private void PlayTextBreakAnimation()
    {
        if (!breakStarted)
        {
            breakStarted = true;
            breakDistance = 0f;
        }

        breakDistance += breakSpeed * Time.unscaledDeltaTime;

        timerText.ForceMeshUpdate();
        TMP_TextInfo textInfo = timerText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            if (!character.isVisible)
                continue;

            Vector3[] vertices =
                textInfo.meshInfo[character.materialReferenceIndex].vertices;

            float direction = i % 2 == 0 ? -1f : 1f;
            float launchStrength = 1.5f + (i % 3) * 0.3f;

            float moveX =
                direction * breakDistance * 0.3f;

            float moveY =
                launchStrength * breakDistance
                - 0.01f * breakDistance * breakDistance;

            for (int corner = 0; corner < 4; corner++)
            {
                int vertexIndex = character.vertexIndex + corner;

                vertices[vertexIndex] += new Vector3(
                    moveX,
                    moveY,
                    0f
                );
            }
        }

        timerText.UpdateVertexData(
            TMP_VertexDataUpdateFlags.Vertices
        );

        if (breakDistance >= hideDistance)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    private void ResetBreakAnimation()
    {
        if (!breakStarted)
            return;

        breakStarted = false;
        breakDistance = 0f;
        timerText.gameObject.SetActive(true);
        timerText.ForceMeshUpdate();
    }
}