using System;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public static Score ScoreInstance;

    public event Action<int> Scored;

    [SerializeField] private TMP_Text scoreText;

    private int score = 0;

    public int CurrentScore => score;

    private float lastPointTime = -1f;

    private void Awake()
    {
        ScoreInstance = this;
        UpdateScoreText();
    }

    public int AddPoints(int points)
    {
        if (points > 0)  {
            int combo = ComboDisplay.Instance != null
                ? ComboDisplay.Instance.CurrentCombo
                : 0;

            float comboMultiplier = Mathf.Pow(1.05f, combo);


            float responseTime = lastPointTime < 0f
                ? 1f
                : Time.time - lastPointTime;

            lastPointTime = Time.time;

            float difficulty = 1f + (combo * 0.5f / 6f);

            float expectedTime = 5f / difficulty;

            float responseRatio = expectedTime / Mathf.Max(responseTime, 0.01f);

            float speedMultiplier = Mathf.Clamp(
                responseRatio,
                1f,
                1.3f
            );
            
            points = Mathf.FloorToInt(
                points * comboMultiplier * speedMultiplier
            );
        }

        score += points;
        UpdateScoreText();
        Scored?.Invoke(points);
        return points;
    }

    public void AddBonusPoints(int points)
    {
        score += points;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}
