using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShiftResultsPanel : MonoBehaviour
{
    [Header("Counters")]
    [SerializeField] private TMP_Text recycleCount;
    [SerializeField] private TMP_Text garbageCount;
    [SerializeField] private TMP_Text organicCount;
    [SerializeField] private TMP_Text paperCount;
    [SerializeField] private TMP_Text scoreValue;

    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextButton;
    [Tooltip("Leave empty until a next scene is available in the build scene list.")]
    [SerializeField] private string nextSceneName;

    [Header("Animation")]
    [SerializeField, Min(0.1f)] private float countDuration = 0.8f;

    private bool loading;

    private void Awake()
    {
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
    }

    private void OnEnable()
    {
        loading = false;
        if (retryButton != null) retryButton.interactable = true;
        if (nextButton != null)
            nextButton.interactable = !string.IsNullOrWhiteSpace(nextSceneName) &&
                Application.CanStreamedLevelBeLoaded(nextSceneName);
        StartCoroutine(ShowCounts());
    }

    private IEnumerator ShowCounts()
    {
        // Wait until the game-over state and final score are ready.
        yield return null;
        GameManager manager = GameManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("ShiftResultsPanel needs an active GameManager.", this);
            yield break;
        }

        TMP_Text[] labels = { recycleCount, garbageCount, organicCount, paperCount, scoreValue };
        int[] values = {
            manager.GetSortedCount(TrashType.Recyclable),
            manager.GetSortedCount(TrashType.Garbage),
            manager.GetSortedCount(TrashType.Organic),
            manager.GetSortedCount(TrashType.Paper),
            manager.FinalScore
        };

        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, countDuration);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            for (int i = 0; i < labels.Length; i++)
                if (labels[i] != null)
                    labels[i].text = Mathf.RoundToInt(values[i] * progress).ToString();
            yield return null;
        }
        for (int i = 0; i < labels.Length; i++)
            if (labels[i] != null) labels[i].text = values[i].ToString();
    }

    public void Retry()
    {
        LoadScene(SceneManager.GetActiveScene().path);
    }

    public void Next()
    {
        LoadScene(nextSceneName);
    }

    private void LoadScene(string scene)
    {
        if (loading) return;
        if (string.IsNullOrWhiteSpace(scene) || !Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogWarning("Add the destination scene to the build scene list.", this);
            return;
        }
        loading = true;
        if (retryButton != null) retryButton.interactable = false;
        if (nextButton != null) nextButton.interactable = false;
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(scene);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        if (retryButton != null) retryButton.onClick.RemoveListener(Retry);
        if (nextButton != null) nextButton.onClick.RemoveListener(Next);
    }
}
