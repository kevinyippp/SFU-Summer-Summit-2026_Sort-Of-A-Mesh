using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsGameOver { get; private set; }

    [Header("Systems")]
    [SerializeField] private Score scoreSystem;

    [Header("Game Over")]
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

        if (scoreSystem == null)
        {
            scoreSystem = FindAnyObjectByType<Score>();
        }

        if (scoreSystem == null)
        {
            Debug.LogError(
                "No Score component was found in the scene."
            );
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
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
        if (IsGameOver)
        {
            return;
        }

        if (scoreSystem != null)
        {
            scoreSystem.AddPoints(points);
        }
        else
        {
            Debug.LogError(
                "Cannot update the score because Score was not found."
            );
        }

        if (ComboDisplay.Instance != null)
        {
            ComboDisplay comboDisplay = ComboDisplay.Instance;
            int previousCombo = comboDisplay.CurrentCombo;

            comboDisplay.RegisterResult(points);

            int currentCombo = comboDisplay.CurrentCombo;

            // Reward each newly reached multiple of ten.
            if (points > 0 &&
                currentCombo > previousCombo &&
                currentCombo / 10 > previousCombo / 10 &&
                scoreSystem != null)
            {
                scoreSystem.AddPoints(10);

                if (TrashToScoreAnimation.Instance != null)
                {
                    TrashToScoreAnimation.Instance.PlayComboBonus(
                        comboDisplay.ComboTarget
                    );
                }
            }
        }
        else
        {
            Debug.LogWarning(
                "ComboDisplay was not found."
            );
        }
    }

    public void EndGame(int finalScore)
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}