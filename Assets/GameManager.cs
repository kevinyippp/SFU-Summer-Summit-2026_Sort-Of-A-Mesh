using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsGameOver { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void EndGame(int finalScore)
    {
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
        }
    }
}
