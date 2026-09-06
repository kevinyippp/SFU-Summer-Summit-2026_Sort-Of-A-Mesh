using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public event Action TimedUp;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float startingTime = 10f;

    [SerializeField] private Image countdownBarFill;

    [Header("Time Limit Visualization")]
    [Tooltip("Assign the background containing the fill and mask, not the whole timer UI.")]
    [SerializeField] private RectTransform timeLimitBar;
    [SerializeField, Min(0.01f)] private float barResizeDuration = 0.25f;

    private float originalBarWidth;
    private float referenceTimeLimit;
    private float displayedBarRatio = 1f;
    private bool barInitialized;

    private bool isRunning;

    public float RemainingTime { get; private set; }

    public bool HasFinished { get; private set; }

    public float StartingTime => startingTime;
    public bool IsRunning => isRunning;
    public TMP_Text TimerText => timerText;

    // Change the capacity without refilling the countdown or unpausing it.
    public void SetTimeLimit(float seconds)
    {
        InitializeBar();
        startingTime = Mathf.Max(0f, seconds);
        RemainingTime = Mathf.Min(RemainingTime, startingTime);
        UpdateTimerText();
        if (startingTime <= 0f && !HasFinished)
        {
            isRunning = false;
            FinishTimer();
        }
    }

    private void Start()
    {
        InitializeBar();
        ResetTimer();
    }

    private void InitializeBar()
    {
        if (barInitialized) return;
        referenceTimeLimit = Mathf.Max(0.1f, startingTime);
        if (countdownBarFill != null &&
            (timeLimitBar == null || timeLimitBar == countdownBarFill.rectTransform))
        {
            Transform candidate = countdownBarFill.transform.parent;
            while (candidate != null && candidate.GetComponent<Canvas>() == null)
            {
                if (candidate.name == "CountdownBarBackground")
                {
                    timeLimitBar = candidate as RectTransform;
                    break;
                }
                candidate = candidate.parent;
            }
        }
        if (timeLimitBar != null)
        {
            originalBarWidth = timeLimitBar.rect.width;
            if (countdownBarFill != null && countdownBarFill.transform.parent == timeLimitBar)
            {
                // Keep the fill within the background as its width changes.
                RectTransform fill = countdownBarFill.rectTransform;
                fill.anchorMin = Vector2.zero;
                fill.anchorMax = Vector2.one;
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;
                fill.localScale = Vector3.one;
            }
        }
        barInitialized = true;
    }

    private void Update()
    {
        if (timeLimitBar != null && barInitialized)
        {
            float targetRatio = startingTime / referenceTimeLimit;
            displayedBarRatio = Mathf.MoveTowards(displayedBarRatio, targetRatio,
                Time.unscaledDeltaTime / Mathf.Max(0.01f, barResizeDuration));
            timeLimitBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                originalBarWidth * displayedBarRatio);
        }

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
        int totalHundredths = Mathf.CeilToInt(Mathf.Max(0f, RemainingTime) * 100f);

        int seconds = totalHundredths / 100;
        int hundredths = totalHundredths % 100;

        if (timerText != null) timerText.text = $"{seconds:00}:{hundredths:00}";

        if (countdownBarFill != null)
        {
            countdownBarFill.fillAmount = Mathf.Clamp01(RemainingTime / Mathf.Max(0.1f, startingTime));
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
        InitializeBar();
        startingTime = Mathf.Max(0.1f, startingTime);
        RemainingTime = startingTime;
        isRunning = true;
        HasFinished = false;

        UpdateTimerText();
    }

    public void ResetTimer(float newStartingTime)
    {
        InitializeBar();
        startingTime = Mathf.Max(0.1f, newStartingTime);
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
