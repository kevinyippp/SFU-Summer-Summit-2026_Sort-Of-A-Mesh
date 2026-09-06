using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ProcessTrashResult(int points)
    {
        if (Score.ScoreInstance == null)
        {
            Debug.LogError("Score system was not found.");
            return;
        }

        if (ComboDisplay.Instance == null)
        {
            Debug.LogError("ComboDisplay was not found.");
            return;
        }

        Score.ScoreInstance.AddPoints(points);
        ComboDisplay.Instance.RegisterResult(points);
    }
}