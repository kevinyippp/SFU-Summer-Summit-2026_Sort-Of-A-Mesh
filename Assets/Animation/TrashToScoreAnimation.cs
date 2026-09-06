using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TrashToScoreAnimation : MonoBehaviour
{
    public static TrashToScoreAnimation Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform scoreTarget;

    [Header("Flying Effect")]
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private float arcHeight = 250f;
    [SerializeField] private float imageSize = 100f;
    [SerializeField] private float rotationAmount = 720f;

    [Header("Golden Meteor Trail")]
    [SerializeField] private bool enableTrail = true;
    [SerializeField, Min(0.01f)] private float trailLifetime = 0.22f;
    [SerializeField, Min(1f)] private float trailWidth = 26f;
    [SerializeField, Range(8, 64)] private int trailSegments = 32;
    [SerializeField] private Color trailColor = new Color(1f, 0.65f, 0.08f, 0.85f);

    private Texture2D trailTexture;
    private Sprite trailSprite;
    private readonly System.Collections.Generic.List<GameObject> activeEffects =
        new System.Collections.Generic.List<GameObject>();

    [Header("Score Punch Effect")]
    [SerializeField] private float punchDuration = 0.2f;
    [SerializeField] private float punchScale = 1.25f;

    [Header("Combo Bonus Burst")]
    [SerializeField] private int bonusTrailCount = 10;
    [SerializeField] private float bonusTrailInterval = 0.05f;
    [SerializeField] private float bonusPathSpread = 100f;

    private RectTransform canvasRect;
    private Camera mainCamera;

    private Vector3 originalScoreScale;
    private Coroutine punchCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        mainCamera = Camera.main;

        if (scoreTarget != null)
        {
            originalScoreScale = scoreTarget.localScale;
        }
    }

    private void OnDestroy()
    {
        if (trailSprite != null) Destroy(trailSprite);
        if (trailTexture != null) Destroy(trailTexture);
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        foreach (GameObject effect in activeEffects)
        {
            if (effect != null) Destroy(effect);
        }
        activeEffects.Clear();
        if (punchCoroutine != null && scoreTarget != null)
            scoreTarget.localScale = originalScoreScale;
        punchCoroutine = null;
    }

    public void PlayComboBonus(RectTransform source)
    {
        if (!isActiveAndEnabled ||
            source == null ||
            canvas == null ||
            canvasRect == null ||
            scoreTarget == null)
        {
            return;
        }

        Canvas sourceCanvas = source.GetComponentInParent<Canvas>();

        if (sourceCanvas == null)
        {
            return;
        }

        sourceCanvas = sourceCanvas.rootCanvas;

        Camera sourceCamera =
            sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : sourceCanvas.worldCamera;

        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                sourceCamera,
                source.TransformPoint(source.rect.center)
            );

        StartCoroutine(PlayBonusTrails(screenPosition));
    }

    private IEnumerator PlayBonusTrails(Vector2 screenPosition)
    {
        int count = Mathf.Clamp(bonusTrailCount, 1, 30);

        for (int i = 0; i < count; i++)
        {
            float fanPosition = count > 1
                ? (float)i / (count - 1)
                : 0.5f;

            float horizontalOffset =
                Mathf.Lerp(-bonusPathSpread, bonusPathSpread, fanPosition);

            Vector2 curveOffset = new Vector2(
                horizontalOffset,
                Random.Range(-30f, 60f)
            );

            StartCoroutine(
                FlyToScore(
                    null,
                    Color.white,
                    Vector3.zero,
                    screenPosition,
                    curveOffset
                )
            );

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.01f, bonusTrailInterval)
            );
        }
    }
    private Sprite GetTrailSprite()
    {
        if (trailSprite != null) return trailSprite;

        // A soft white core with transparent edges, tinted gold by the UI Image.
        const int size = 32;
        trailTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        trailTexture.wrapMode = TextureWrapMode.Clamp;
        trailTexture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            float edge = Mathf.Abs((y + 0.5f) / size * 2f - 1f);
            float alpha = Mathf.Pow(1f - edge, 2f);
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
        }
        trailTexture.SetPixels(pixels);
        trailTexture.Apply(false, true);
        trailSprite = Sprite.Create(trailTexture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
        return trailSprite;
    }

    public void PlayEffect(Trash trash)
    {
        if (trash == null)
        {
            return;
        }

        SpriteRenderer trashRenderer =
            trash.GetComponent<SpriteRenderer>();

        if (trashRenderer == null || trashRenderer.sprite == null)
        {
            Debug.LogWarning(
                trash.gameObject.name +
                " does not have a SpriteRenderer or Sprite."
            );

            return;
        }

        if (canvas == null || canvasRect == null)
        {
            Debug.LogError(
                "Canvas has not been assigned to TrashToScoreAnimation."
            );

            return;
        }

        if (scoreTarget == null)
        {
            Debug.LogError(
                "Score Target has not been assigned to TrashToScoreAnimation."
            );

            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera was not found.");
            return;
        }

        Sprite spriteCopy = trashRenderer.sprite;
        Color colorCopy = trashRenderer.color;
        Vector3 worldStartPosition = trash.transform.position;

        StartCoroutine(
            FlyToScore(
                spriteCopy,
                colorCopy,
                worldStartPosition
            )
        );
    }

    private IEnumerator FlyToScore(
    Sprite sprite,
    Color color,
    Vector3 worldStartPosition,
    Vector2? screenStartOverride = null,
    Vector2? curveOffset = null
)
    {
        GameObject flyingObject =
            new GameObject(
                "FlyingTrashEffect",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        RectTransform flyingRect =
            flyingObject.GetComponent<RectTransform>();

        Image flyingImage =
            flyingObject.GetComponent<Image>();

        flyingRect.SetParent(canvasRect, false);
        flyingRect.SetAsLastSibling();
        activeEffects.Add(flyingObject);
        // Match canvas-local coordinates even when the canvas pivot is customized.
        flyingRect.anchorMin = flyingRect.anchorMax = canvasRect.pivot;

        GameObject trailRoot = new GameObject("GoldenMeteorTrail", typeof(RectTransform));
        RectTransform trailRootRect = trailRoot.GetComponent<RectTransform>();
        trailRootRect.SetParent(canvasRect, false);
        trailRootRect.anchorMin = trailRootRect.anchorMax = canvasRect.pivot;
        trailRootRect.anchoredPosition = Vector2.zero;
        trailRootRect.sizeDelta = Vector2.zero;
        trailRootRect.SetSiblingIndex(flyingRect.GetSiblingIndex());
        activeEffects.Add(trailRoot);

        bool showTrail = false;

        if (enableTrail || screenStartOverride.HasValue)
        {
            showTrail = true;
        }

        int segmentCount = 0;

        if (showTrail)
        {
            segmentCount = Mathf.Clamp(trailSegments, 8, 64);
        }

        Image[] segments = new Image[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = new GameObject("TrailSegment", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            segments[i] = segment.GetComponent<Image>();
            segments[i].rectTransform.SetParent(trailRootRect, false);
            segments[i].sprite = GetTrailSprite();
            segments[i].raycastTarget = false;
            segments[i].color = Color.clear;
        }

        flyingRect.sizeDelta =
            new Vector2(imageSize, imageSize);

        flyingImage.sprite = sprite;
        // Show the image only when a sprite exists.
        if (sprite != null)
        {
            flyingImage.enabled = true;
        }
        else
        {
            flyingImage.enabled = false;
        }
        flyingImage.color = color;
        flyingImage.preserveAspect = true;
        flyingImage.raycastTarget = false;

        Camera uiCamera = null;

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector2 startScreenPosition;

        if (screenStartOverride.HasValue)
        {
            startScreenPosition = screenStartOverride.Value;
        }
        else
        {
            startScreenPosition =
                (Vector2)mainCamera.WorldToScreenPoint(worldStartPosition);
        }

        Vector2 targetScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                scoreTarget.position
            );

        Vector2 startPosition;
        Vector2 targetPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            startScreenPosition,
            uiCamera,
            out startPosition
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            targetScreenPosition,
            uiCamera,
            out targetPosition
        );

        flyingRect.anchoredPosition = startPosition;

        // Bezier
        Vector2 offset = curveOffset ?? Vector2.zero;

        Vector2 controlPointOne =
            startPosition +
            Vector2.up * arcHeight +
            offset;

        Vector2 controlPointTwo =
            targetPosition +
            new Vector2(-arcHeight, arcHeight * 0.5f) +
            offset * 0.5f;

        Vector3 startingScale = Vector3.one;

        float elapsedTime = 0f;
        float flightDuration = Mathf.Max(0.01f, duration);
        float tailDuration;

        if (showTrail)
        {
            tailDuration = Mathf.Max(0.01f, trailLifetime);
        }
        else
        {
            tailDuration = 0f;
        }
        bool arrived = false;

        while (elapsedTime < flightDuration + tailDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / flightDuration);

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            flyingRect.anchoredPosition = CalculateBezierPoint(
                smoothProgress,
                startPosition,
                controlPointOne,
                controlPointTwo,
                targetPosition
            );

            flyingRect.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    rotationAmount * progress
                );

            flyingRect.localScale =
                Vector3.Lerp(
                    startingScale,
                    Vector3.zero,
                    smoothProgress
                );

            // Sample the same flight path at earlier times for a continuous curved tail.
            for (int i = 0; i < segmentCount; i++)
            {
                float age = (float)i / segmentCount;
                float newer = Mathf.Clamp(elapsedTime - age * tailDuration, 0f, flightDuration);
                float older = Mathf.Clamp(elapsedTime - (i + 1f) / segmentCount * tailDuration,
                    0f, flightDuration);
                Vector2 a = CalculateBezierPoint(Mathf.SmoothStep(0f, 1f, newer / flightDuration),
                    startPosition, controlPointOne, controlPointTwo, targetPosition);
                Vector2 b = CalculateBezierPoint(Mathf.SmoothStep(0f, 1f, older / flightDuration),
                    startPosition, controlPointOne, controlPointTwo, targetPosition);
                Vector2 direction = a - b;
                RectTransform rect = segments[i].rectTransform;
                rect.anchoredPosition = (a + b) * 0.5f;
                rect.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                rect.sizeDelta = new Vector2(direction.magnitude + 1f,
                    Mathf.Max(1f, trailWidth) * (1f - age));
                Color tint = Color.Lerp(new Color(1f, 0.95f, 0.6f), trailColor, age);
                tint.a = direction.sqrMagnitude < 0.001f ? 0f :
                    trailColor.a * Mathf.Pow(1f - age, 1.5f);
                segments[i].color = tint;
            }

            if (!arrived && elapsedTime >= flightDuration)
            {
                arrived = true;
                flyingImage.enabled = false;
                PlayScorePunch();
            }

            yield return null;
        }

        Destroy(flyingObject);
        Destroy(trailRoot);
        activeEffects.Remove(flyingObject);
        activeEffects.Remove(trailRoot);
    }

    private Vector2 CalculateBezierPoint(
        float progress,
        Vector2 start,
        Vector2 controlOne,
        Vector2 controlTwo,
        Vector2 end
    )
    {
        float remaining = 1f - progress;

        return
            remaining * remaining * remaining * start +
            3f * remaining * remaining * progress * controlOne +
            3f * remaining * progress * progress * controlTwo +
            progress * progress * progress * end;
    }

    private void PlayScorePunch()
    {
        if (scoreTarget == null)
        {
            return;
        }

        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
            scoreTarget.localScale = originalScoreScale;
        }

        punchCoroutine = StartCoroutine(ScorePunchAnimation());
    }

    private IEnumerator ScorePunchAnimation()
    {
        float halfDuration = punchDuration * 0.5f;
        float elapsedTime = 0f;

        Vector3 enlargedScale =
            originalScoreScale * punchScale;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / halfDuration);

            scoreTarget.localScale =
                Vector3.Lerp(
                    originalScoreScale,
                    enlargedScale,
                    progress
                );

            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / halfDuration);

            scoreTarget.localScale =
                Vector3.Lerp(
                    enlargedScale,
                    originalScoreScale,
                    progress
                );

            yield return null;
        }

        scoreTarget.localScale = originalScoreScale;
        punchCoroutine = null;
    }
}
