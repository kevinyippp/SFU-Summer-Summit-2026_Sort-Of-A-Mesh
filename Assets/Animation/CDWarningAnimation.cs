using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CDWarningAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CountdownTimer timer;
    [SerializeField] private RectTransform timerBar;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text countdownText;

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

    [Header("Bar Break")]
    [SerializeField, Range(4, 40)] private int barPieceCount = 16;
    [SerializeField] private float barBurstSpeed = 180f;
    [SerializeField] private float barGravity = 700f;
    [SerializeField, Min(0.1f)] private float barBreakDuration = 1.3f;

    private readonly List<Image> hiddenBarImages = new List<Image>();
    private readonly List<BarPiece> barPieces = new List<BarPiece>();
    private GameObject barPieceRoot;
    private float barElapsed;

    private class BarPiece
    {
        public RectTransform rect;
        public Image image;
        public Vector2 start;
        public Vector2 velocity;
        public float spin;
        public Color color;
    }

    private void Start()
    {
        if (timerBar != null)
            originalBarPosition = timerBar.anchoredPosition;
        if (countdownText == null && timer != null)
            countdownText = timer.TimerText;
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
            BeginBarBreak();
        }

        breakDistance += breakSpeed * Time.unscaledDeltaTime;
        AnimateBarBreak();

        AnimateText(timerText);
        if (countdownText != timerText)
            AnimateText(countdownText);
    }

    private void AnimateText(TMP_Text text)
    {
        if (text == null || !text.gameObject.activeInHierarchy)
            return;

        text.ForceMeshUpdate();
        TMP_TextInfo textInfo = text.textInfo;

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

        text.UpdateVertexData(
            TMP_VertexDataUpdateFlags.Vertices
        );

        if (breakDistance >= hideDistance)
        {
            text.gameObject.SetActive(false);
        }
    }

    private void ResetBreakAnimation()
    {
        if (!breakStarted)
            return;

        breakStarted = false;
        ClearBarBreak();
        breakDistance = 0f;
        timerText.gameObject.SetActive(true);
        timerText.ForceMeshUpdate();
        if (countdownText != null && countdownText != timerText)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.ForceMeshUpdate();
        }
    }

    private void BeginBarBreak()
    {
        ClearBarBreak();
        barElapsed = 0f;
        Image background = timerBar.GetComponent<Image>();
        Color color = background != null ? background.color : Color.white;

        // Use a sibling so the bar's own mask cannot clip falling fragments.
        barPieceRoot = new GameObject("Timer Bar Fragments", typeof(RectTransform));
        RectTransform root = (RectTransform)barPieceRoot.transform;
        root.SetParent(timerBar.parent, false);
        root.anchorMin = timerBar.anchorMin;
        root.anchorMax = timerBar.anchorMax;
        root.pivot = timerBar.pivot;
        root.sizeDelta = timerBar.sizeDelta;
        root.anchoredPosition3D = timerBar.anchoredPosition3D;
        root.localRotation = timerBar.localRotation;
        root.localScale = timerBar.localScale;
        root.SetSiblingIndex(timerBar.GetSiblingIndex() + 1);

        Rect bounds = timerBar.rect;
        int count = Mathf.Clamp(barPieceCount, 4, 40);
        float width = bounds.width / count;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = new GameObject("Bar Fragment", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = (RectTransform)obj.transform;
            rect.SetParent(root, false);
            rect.anchorMin = rect.anchorMax = root.pivot;
            rect.sizeDelta = new Vector2(width, bounds.height);
            Vector2 start = new Vector2(bounds.xMin + width * (i + 0.5f), bounds.center.y);
            rect.anchoredPosition = start;
            Image image = obj.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            barPieces.Add(new BarPiece
            {
                rect = rect, image = image, start = start, color = color,
                velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(0.6f, 1.4f)) * barBurstSpeed,
                spin = Random.Range(-240f, 240f)
            });
        }

        // Hide graphics only; keep the timer and animation components running.
        foreach (Image image in timerBar.GetComponentsInChildren<Image>())
        {
            if (!image.enabled) continue;
            hiddenBarImages.Add(image);
            image.enabled = false;
        }
    }

    private void AnimateBarBreak()
    {
        if (barPieceRoot == null) return;
        barElapsed += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(barElapsed / Mathf.Max(0.1f, barBreakDuration));
        foreach (BarPiece piece in barPieces)
        {
            piece.rect.anchoredPosition = piece.start + piece.velocity * barElapsed
                + Vector2.down * (0.5f * barGravity * barElapsed * barElapsed);
            piece.rect.localRotation = Quaternion.Euler(0f, 0f, piece.spin * barElapsed);
            Color color = piece.color;
            color.a *= 1f - progress;
            piece.image.color = color;
        }
        if (progress >= 1f)
        {
            Destroy(barPieceRoot);
            barPieceRoot = null;
            barPieces.Clear();
        }
    }

    private void ClearBarBreak()
    {
        if (barPieceRoot != null) Destroy(barPieceRoot);
        barPieceRoot = null;
        barPieces.Clear();
        foreach (Image image in hiddenBarImages)
            if (image != null) image.enabled = true;
        hiddenBarImages.Clear();
    }

    private void OnDisable()
    {
        ClearBarBreak();
    }
}
