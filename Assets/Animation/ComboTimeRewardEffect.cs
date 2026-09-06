using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboTimeRewardEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform comboText;
    [SerializeField] private RectTransform timerBar;
    [SerializeField] private TMP_FontAsset font;

    [Header("Flight")]
    [SerializeField] private float flightDuration = 0.65f;
    [SerializeField] private float arcHeight = 120f;
    [SerializeField] private float fontSize = 42f;
    [SerializeField]
    private Color gold =
        new Color(1f, 0.78f, 0.15f, 1f);

    [Header("Trail")]
    [SerializeField] private float trailLifetime = 0.2f;
    [SerializeField] private float trailSize = 18f;
    [SerializeField, Range(8, 40)] private int trailCount = 24;

    [Header("Bar Punch")]
    [SerializeField] private float punchDuration = 0.22f;
    [SerializeField] private float punchScale = 1.08f;

    private readonly List<GameObject> activeEffects =
        new List<GameObject>();

    private Sprite glowSprite;
    private Texture2D glowTexture;
    private Coroutine punchRoutine;
    private Vector3 restingBarScale;

    private void Awake()
    {
        if (timerBar != null)
        {
            restingBarScale = timerBar.localScale;
        }
    }

    public void Play(float seconds)
    {
        if (seconds <= 0f || !isActiveAndEnabled)
            return;

        if (canvas == null || comboText == null || timerBar == null)
        {
            Debug.LogWarning("Assign Canvas, Combo Text and Timer Bar.", this);
            return;
        }

        StartCoroutine(Fly(seconds));
    }

    private Camera GetUICamera(Canvas targetCanvas)
    {
        Canvas root = targetCanvas.rootCanvas;

        return root.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : root.worldCamera;
    }

    private Vector2 ToCanvasPosition(RectTransform target)
    {
        Canvas sourceCanvas = target.GetComponentInParent<Canvas>();
        Camera sourceCamera = sourceCanvas != null
            ? GetUICamera(sourceCanvas)
            : null;

        Vector3 center = target.TransformPoint(target.rect.center);
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(
            sourceCamera, center
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            screen,
            GetUICamera(canvas),
            out Vector2 position
        );

        return position;
    }

    private Vector2 Curve(Vector2 start, Vector2 end, float progress)
    {
        Vector2 control = (start + end) * 0.5f +
            Vector2.up * arcHeight;

        float inverse = 1f - progress;

        return inverse * inverse * start +
            2f * inverse * progress * control +
            progress * progress * end;
    }

    private IEnumerator Fly(float seconds)
    {
        RectTransform canvasRect = (RectTransform)canvas.transform;

        GameObject root = new GameObject(
            "ComboTimeReward",
            typeof(RectTransform)
        );

        activeEffects.Add(root);

        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.SetParent(canvasRect, false);
        rootRect.anchorMin = rootRect.anchorMax = canvasRect.pivot;
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.SetAsLastSibling();

        int count = Mathf.Clamp(trailCount, 8, 40);
        Image[] trail = new Image[count];

        for (int i = 0; i < count; i++)
        {
            GameObject dot = new GameObject(
                "GoldenTrail",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            dot.transform.SetParent(rootRect, false);

            trail[i] = dot.GetComponent<Image>();
            trail[i].sprite = GetGlowSprite();
            trail[i].raycastTarget = false;
            trail[i].color = Color.clear;
        }

        GameObject textObject = new GameObject(
            "RewardText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        textObject.transform.SetParent(rootRect, false);

        TextMeshProUGUI label =
            textObject.GetComponent<TextMeshProUGUI>();

        if (font != null)
            label.font = font;

        label.text = $"+{seconds:0.##}s";
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = gold;
        label.raycastTarget = false;
        label.rectTransform.sizeDelta = new Vector2(240f, 90f);

        Vector2 start = ToCanvasPosition(comboText);
        float duration = Mathf.Max(0.1f, flightDuration);
        float tailDuration = Mathf.Max(0.02f, trailLifetime);
        float elapsed = 0f;
        bool arrived = false;

        while (elapsed < duration + tailDuration)
        {
            if (timerBar == null)
                break;

            elapsed += Time.unscaledDeltaTime;

            // Follow the bar while it shrinks or shakes.
            Vector2 end = ToCanvasPosition(timerBar);
            float progress = Mathf.Clamp01(elapsed / duration);

            label.rectTransform.anchoredPosition =
                Curve(start, end, progress);

            float textScale = Mathf.Lerp(
                1f, 0.35f,
                Mathf.InverseLerp(0.65f, 1f, progress)
            );

            label.rectTransform.localScale =
                Vector3.one * textScale;

            for (int i = 0; i < count; i++)
            {
                float age = (i + 1f) / count;
                float sampleTime = elapsed - age * tailDuration;

                bool visible = sampleTime >= 0f &&
                    sampleTime < duration;

                float sampleProgress =
                    Mathf.Clamp01(sampleTime / duration);

                trail[i].rectTransform.anchoredPosition =
                    Curve(start, end, sampleProgress);

                float size = Mathf.Lerp(trailSize, 2f, age);
                trail[i].rectTransform.sizeDelta =
                    Vector2.one * size;

                Color tint = gold;
                tint.a *= visible ? (1f - age) * 0.85f : 0f;
                trail[i].color = tint;
            }

            if (!arrived && elapsed >= duration)
            {
                arrived = true;
                label.enabled = false;
                PunchBar();
            }

            yield return null;
        }

        activeEffects.Remove(root);
        Destroy(root);
    }

    private Sprite GetGlowSprite()
    {
        if (glowSprite != null)
            return glowSprite;

        const int size = 32;
        glowTexture = new Texture2D(
            size, size, TextureFormat.RGBA32, false
        );

        glowTexture.wrapMode = TextureWrapMode.Clamp;
        glowTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(
                    (x + 0.5f) / size * 2f - 1f,
                    (y + 0.5f) / size * 2f - 1f
                );

                float alpha = Mathf.Pow(
                    Mathf.Clamp01(1f - point.magnitude), 2f
                );

                pixels[y * size + x] =
                    new Color(1f, 1f, 1f, alpha);
            }
        }

        glowTexture.SetPixels(pixels);
        glowTexture.Apply(false, true);

        glowSprite = Sprite.Create(
            glowTexture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );

        return glowSprite;
    }

    private void PunchBar()
    {
        if (timerBar == null)
            return;

        if (punchRoutine != null)
        {
            StopCoroutine(punchRoutine);
        }
        else
        {
            restingBarScale = timerBar.localScale;
        }

        timerBar.localScale = restingBarScale;
        punchRoutine = StartCoroutine(AnimatePunch());
    }

    private IEnumerator AnimatePunch()
    {
        float duration = Mathf.Max(0.05f, punchDuration);
        float elapsed = 0f;

        while (elapsed < duration && timerBar != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            float scale = 1f +
                Mathf.Sin(progress * Mathf.PI) * (punchScale - 1f);

            timerBar.localScale = restingBarScale * scale;

            yield return null;
        }

        if (timerBar != null)
            timerBar.localScale = restingBarScale;

        punchRoutine = null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (punchRoutine != null && timerBar != null)
            timerBar.localScale = restingBarScale;

        punchRoutine = null;

        foreach (GameObject effect in activeEffects)
        {
            if (effect != null)
                Destroy(effect);
        }

        activeEffects.Clear();
    }

    private void OnDestroy()
    {
        if (glowSprite != null)
            Destroy(glowSprite);

        if (glowTexture != null)
            Destroy(glowTexture);
    }
}