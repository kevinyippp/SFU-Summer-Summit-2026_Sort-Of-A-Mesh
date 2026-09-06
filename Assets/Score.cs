using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public static Score ScoreInstance;

    [SerializeField] private TMP_Text scoreText;

    private int score = 0;

    public int CurrentScore => score;

    private void Awake()
    {
        ScoreInstance = this;
        UpdateScoreText();
    }

    public void AddPoints(int points)
    {
        score += points;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}
