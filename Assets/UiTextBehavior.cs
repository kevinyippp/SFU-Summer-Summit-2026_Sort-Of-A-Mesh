using UnityEngine;
using TMPro;

public class UiTextBehavior : MonoBehaviour
{
    TextMeshPro label; 

    [Header("Movement")]
    public float duration = 1.5f;
    public float moveSpeed = 2f;

    [Header("Scale")]
    public float maxScale = 1.5f;

    Vector3 startScale;
    float timer = 0f;

    void Awake()
    {
        label = GetComponent<TextMeshPro>();
    }

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        transform.position += Vector3.up * (moveSpeed * Time.deltaTime);
        transform.localScale = Vector3.Lerp(startScale, startScale * maxScale, t);

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        label.text = text;
        timer = 0f;
        startScale = transform.localScale;
    }
}