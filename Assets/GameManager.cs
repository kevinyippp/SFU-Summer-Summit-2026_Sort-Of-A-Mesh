using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsGameOver { get; private set; }

    [Header("Systems")]
    [SerializeField] private Score scoreSystem;
    [SerializeField] private CountdownTimer countdownTimer;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Timer Scaling")]
    [SerializeField] private float timerShrinkFactor = 0.9f;
    [SerializeField] private float minTimerDuration = 2f;

    private float currentTimerDuration;

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
        else
        {
            scoreSystem.Scored += HandleScored;
        }

        if (countdownTimer == null)
        {
            countdownTimer = FindAnyObjectByType<CountdownTimer>();
        }

        if (countdownTimer != null)
        {
            currentTimerDuration = countdownTimer.StartingTime;
            countdownTimer.TimedUp += HandleTimedUp;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (scoreSystem != null)
        {
            scoreSystem.Scored -= HandleScored;
        }

        if (countdownTimer != null)
        {
            countdownTimer.TimedUp -= HandleTimedUp;
        }
    }

    private void HandleScored(int points)
    {
        if (IsGameOver)
        {
            return;
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
                scoreSystem.AddBonusPoints(10);

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

        if (countdownTimer != null && points > 0)
        {
            currentTimerDuration = Mathf.Max(
                minTimerDuration,
                currentTimerDuration * timerShrinkFactor
            );

            countdownTimer.ResetTimer(currentTimerDuration);
        }
    }

    private void HandleTimedUp()
    {
        EndGame(scoreSystem != null ? scoreSystem.CurrentScore : 0);
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
            finalScoreText.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}