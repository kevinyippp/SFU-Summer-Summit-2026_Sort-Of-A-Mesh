using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private GameObject restartButton;


    [Header("Combo Time Rewards")]
    [SerializeField, Min(1)] private int timeRewardComboInterval = 5;
    [SerializeField, Min(0f)] private float timeRewardSeconds = 0.25f;
    [SerializeField, Min(0.1f)] private float maximumTimerDuration = 20f;

    private float currentTimerDuration;

    [Header("Round Pressure")]
    [SerializeField, Min(5f)] private float targetRoundSeconds = 30f;
    [SerializeField, Min(0f)] private float maximumComboExtensionSeconds = 5f;
    private float timeLimitDecayPerSecond;
    private float remainingTimeRewardBudget;

    [SerializeField] private ComboTimeRewardEffect timeRewardEffect;

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
            float target = Mathf.Max(5f, targetRoundSeconds);
            float extension = Mathf.Clamp(maximumComboExtensionSeconds, 0f, target * 0.5f);
            timeLimitDecayPerSecond = Mathf.Max(0.1f, currentTimerDuration) / (target - extension);
            remainingTimeRewardBudget = timeLimitDecayPerSecond * extension;
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
        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }

    }

    private void Update()
    {
        if (Instance != this || IsGameOver || countdownTimer == null ||
            !countdownTimer.IsRunning || !countdownTimer.isActiveAndEnabled)
            return;

        currentTimerDuration = Mathf.Max(0f,
            currentTimerDuration - timeLimitDecayPerSecond * Time.deltaTime);
        countdownTimer.SetTimeLimit(currentTimerDuration);
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

            int interval = Mathf.Max(1, timeRewardComboInterval);
            int milestones = currentCombo / interval - previousCombo / interval;
            if (points > 0 && milestones > 0 && countdownTimer != null)
            {
                float limit = Mathf.Max(currentTimerDuration, maximumTimerDuration);
                // A round-wide budget prevents combo rewards from cancelling the pressure.
                float reward = Mathf.Min(milestones * Mathf.Max(0f, timeRewardSeconds),
                    remainingTimeRewardBudget, limit - currentTimerDuration);
                currentTimerDuration += reward;
                remainingTimeRewardBudget -= reward;

                if (reward > 0f && timeRewardEffect != null)
                {
                    timeRewardEffect.Play(reward);
                }
            }

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
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
