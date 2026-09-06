using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsGameOver { get; private set; }

<<<<<<< HEAD
=======
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;

>>>>>>> origin/main
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
<<<<<<< HEAD
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
=======
        IsGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
>>>>>>> origin/main
        }
    }

    public void EndGame(int finalScore)
    {
<<<<<<< HEAD
        if (Score.ScoreInstance == null)
        {
            Debug.LogError("Score system was not found.");
            return;
        }

        if (ComboDisplay.Instance == null)
        {
            Debug.LogError("ComboDisplay was not found.");
            return;
=======
        if (IsGameOver)
            return;

        IsGameOver = true;

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
>>>>>>> origin/main
        }

        Score.ScoreInstance.AddPoints(points);
        ComboDisplay.Instance.RegisterResult(points);
    }
}