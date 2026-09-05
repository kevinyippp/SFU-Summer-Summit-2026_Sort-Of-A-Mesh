using UnityEngine;

public class TextSpawner : MonoBehaviour
{
    public static TextSpawner Instance;

    public GameObject floatingTextPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void Spawn(string text, float x, float y)
    {
        Vector3 spawnPos = new Vector3(x, y, 0f);
        GameObject instance = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);

        UiTextBehavior behavior = instance.GetComponent<UiTextBehavior>();
        behavior.SetText(text);
    }
}