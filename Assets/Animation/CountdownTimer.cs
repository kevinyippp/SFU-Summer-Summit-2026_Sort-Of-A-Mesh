using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public event Action TimedUp;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float startingTime = 60f;

    [SerializeField] private Image countdownBarFill;

    private bool isRunning;

    public float RemainingTime { get; private set; }

    public bool HasFinished { get; private set; }

    public float StartingTime => startingTime;

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


        RemainingTime -= Time.deltaTime;

        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            isRunning = false;

            UpdateTimerText();
            FinishTimer();
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        int totalSeconds = Mathf.CeilToInt(RemainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";

        if (countdownBarFill != null)
        {
            countdownBarFill.fillAmount = Mathf.Clamp01(RemainingTime / startingTime);
        }
    }

    private void FinishTimer()
    {
        if (HasFinished)
            return;

        HasFinished = true;

        Debug.Log("Time is up!");

        TimedUp?.Invoke();
    }

    public void ResetTimer()
    {
        RemainingTime = startingTime;
        isRunning = true;
        HasFinished = false;

        UpdateTimerText();
    }

    public void ResetTimer(float newStartingTime)
    {
        startingTime = newStartingTime;
        ResetTimer();
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        if (RemainingTime > 0f)
        {
            isRunning = true;
        }
    }
}
