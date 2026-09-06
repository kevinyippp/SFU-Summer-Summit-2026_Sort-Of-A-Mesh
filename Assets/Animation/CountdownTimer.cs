using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float startingTime = 60f;

    [SerializeField] private Image countdownBarFill;

    private float remainingTime;
    private bool isRunning;
    private bool hasFinished;

    public float RemainingTime => remainingTime;
    public bool HasFinished => hasFinished;

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }
            

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;

            UpdateTimerText();
            FinishTimer();
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        int totalSeconds = Mathf.CeilToInt(remainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";

        if (countdownBarFill != null)
        {
            countdownBarFill.fillAmount = Mathf.Clamp01(remainingTime / startingTime);
        }
    }

    private void FinishTimer()
    {
        if (hasFinished)
            return;

        hasFinished = true;

        Debug.Log("Time is up!");

        if (GameManager.Instance != null)
        {
            int finalScore = Score.ScoreInstance != null ? Score.ScoreInstance.CurrentScore : 0;
            GameManager.Instance.EndGame(finalScore);
        }
    }

    public void ResetTimer()
    {
        remainingTime = startingTime;
        isRunning = true;
        hasFinished = false;

        UpdateTimerText();
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        if (remainingTime > 0f)
        {
            isRunning = true;
        }
    }
}
