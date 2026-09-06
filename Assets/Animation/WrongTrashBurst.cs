using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WrongTrashBurst : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Sprite iconSprite;
    [SerializeField, Range(1, 30)] private int iconCount = 10;
    [SerializeField] private float iconSize = 38f;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float spreadSpeed = 220f;
    [SerializeField] private float launchSpeed = 260f;
    [SerializeField] private float gravity = 750f;
    [SerializeField] private Color iconColor = new Color(1f, 0.4f, 0.25f, 1f);

    private readonly List<GameObject> bursts = new List<GameObject>();

    public void Play(Vector3 worldPosition)
    {
        if (canvas == null)
        {
            foreach (Canvas candidate in FindObjectsByType<Canvas>())
            {
                if (candidate.isActiveAndEnabled && candidate.isRootCanvas)
                {
                    canvas = candidate;
                    break;
                }
            }
        }

        Camera worldCamera = Camera.main;
        if (canvas == null || worldCamera == null)
        {
            Debug.LogWarning("WrongTrashBurst needs an active Canvas and Main Camera.");
            return;
        }

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : canvas.worldCamera;
        RectTransform canvasRect = (RectTransform)canvas.transform;
        Vector2 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPosition, uiCamera, out Vector2 position))
            StartCoroutine(Animate(canvasRect, position));
    }

    private IEnumerator Animate(RectTransform canvasRect, Vector2 position)
    {
        GameObject root = new GameObject("WrongTrashThumbsDown", typeof(RectTransform));
        bursts.Add(root);
        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.SetParent(canvasRect, false);
        rootRect.anchorMin = rootRect.anchorMax = canvasRect.pivot;
        rootRect.anchoredPosition = position;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.SetAsLastSibling();

        int count = Mathf.Clamp(iconCount, 1, 30);
        Graphic[] icons = new Graphic[count];
        Vector2[] velocities = new Vector2[count];
        float[] sizes = new float[count];
        for (int i = 0; i < count; i++)
        {
            GameObject item = new GameObject("ThumbDown", typeof(RectTransform));
            item.transform.SetParent(rootRect, false);
            if (iconSprite != null)
            {
                Image image = item.AddComponent<Image>();
                image.sprite = iconSprite;
                image.preserveAspect = true;
                icons[i] = image;
            }
            else
            {
                icons[i] = item.AddComponent<Image>();
            }
            icons[i].raycastTarget = false;
            icons[i].color = iconColor;
            sizes[i] = Mathf.Max(1f, iconSize) * Random.Range(0.75f, 1.25f);
            icons[i].rectTransform.sizeDelta = Vector2.one * sizes[i];
            float horizontal = Mathf.Lerp(-1f, 1f, (i + 0.5f) / count);
            velocities[i] = new Vector2(horizontal * spreadSpeed,
                launchSpeed * Random.Range(0.65f, 1.25f));
        }

        float lifetime = Mathf.Max(0.1f, duration);
        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / lifetime);
            float fade = 1f - Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(0.45f, 1f, progress));
            for (int i = 0; i < count; i++)
            {
                RectTransform rect = icons[i].rectTransform;
                rect.anchoredPosition = velocities[i] * elapsed +
                    Vector2.down * (0.5f * gravity * elapsed * elapsed);
                rect.localRotation = Quaternion.identity;
                float pop = Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(progress / 0.1f));
                rect.localScale = Vector3.one * pop * Mathf.Lerp(1f, 0.65f, progress);
                Color tint = iconColor;
                tint.a *= fade;
                icons[i].color = tint;
            }
            yield return null;
        }
        bursts.Remove(root);
        Destroy(root);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        foreach (GameObject burst in bursts)
            if (burst != null) Destroy(burst);
        bursts.Clear();
    }
}

// UI geometry keeps the fallback icon sharp at any Canvas scale.
public class ThumbDownGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        AddQuad(vh, new Vector2(0.05f, 0.43f), new Vector2(0.19f, 0.43f),
            new Vector2(0.19f, 0.94f), new Vector2(0.05f, 0.94f));
        AddQuad(vh, new Vector2(0.23f, 0.42f), new Vector2(0.71f, 0.42f),
            new Vector2(0.71f, 0.92f), new Vector2(0.23f, 0.92f));
        for (int i = 0; i < 4; i++)
        {
            float y = 0.44f + i * 0.12f;
            float end = i == 0 || i == 3 ? 0.87f : 0.95f;
            AddQuad(vh, new Vector2(0.6f, y), new Vector2(end, y),
                new Vector2(end, y + 0.105f), new Vector2(0.6f, y + 0.105f));
        }
        AddQuad(vh, new Vector2(0.24f, 0.47f), new Vector2(0.51f, 0.44f),
            new Vector2(0.62f, 0.10f), new Vector2(0.47f, 0.04f));
    }

    private void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Rect rect = rectTransform.rect;
        int start = vh.currentVertCount;
        Vector2[] points = { a, b, c, d };
        foreach (Vector2 point in points)
        {
            vh.AddVert(new Vector3(rect.xMin + point.x * rect.width,
                rect.yMin + point.y * rect.height, 0f), color, Vector2.zero);
        }
        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }
}
