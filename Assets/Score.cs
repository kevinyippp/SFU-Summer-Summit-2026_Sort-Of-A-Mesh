using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public static Score ScoreInstance;

    [SerializeField] private TMP_Text scoreText;

    private int score = 0;

    private void Awake()
    {
        ScoreInstance = this;
        UpdateScoreText();
    }

    public void AddPoint()
    {
        score++;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}
