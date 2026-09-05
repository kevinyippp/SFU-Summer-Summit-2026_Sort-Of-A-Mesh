using UnityEngine;
using TMPro;

public class AnimationTest : MonoBehaviour
{
    public static AnimationTest Instance { get; private set; }

    [SerializeField] private TMP_Text tmp;

    [Header("Wave")]
    [SerializeField] private float animationSpeed = 6f;
    [SerializeField] private float waveSpacing = 0.02f;
    [SerializeField] private float movementAmplitude = 30f;

    private float startingAnimationSpeed = 6f;
    private float startingWaveSpacing = 0.02f;
    private float startingMovementAmplitude = 30f;

    [Header("Break")]
    [SerializeField] private float breakSpeed = 200f;
    [SerializeField] private float hideDistance = 500f;

    private int currentCombo = -1;
    private bool isBreaking;
    private float breakDistance;
    private TMP_Text scoreText;
    private int previousScore;
    private bool isScoreTrackingReady;

    public int CurrentCombo => Mathf.Max(0, currentCombo);

    private void Awake()
    {

        startingAnimationSpeed = animationSpeed;
        startingWaveSpacing = waveSpacing;
        startingMovementAmplitude = movementAmplitude;

        if (Instance != null && Instance != this)
        {
            Debug.LogError("More than one AnimationTest combo system exists in the scene.");
            enabled = false;
            return;
        }

        Instance = this;

        if (tmp == null)
        {
            tmp = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void Start()
    {
        SetCombo(0);
        InitializeScoreTracking();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RegisterResult(int points)
    {
        if (points > 0)
        {
            AddCombo();
        }
        else
        {
            BreakCombo();
        }
    }

    private void speedUptheWave()
    {
        animationSpeed += 0.5f;
        waveSpacing += 0.002f;
        movementAmplitude += 0.5f;
    }

    private void InitializeScoreTracking()
    {
        if (TryReadScore(out int score))
        {
            previousScore = score;
            isScoreTrackingReady = true;
        }
    }

    public void UpdateComboFromScore()
    {
        if (!TryReadScore(out int score))
            return;

        if (!isScoreTrackingReady)
        {
            previousScore = score;
            isScoreTrackingReady = true;
            return;
        }

        if (score == previousScore)
            return;

        int scoreChange = score - previousScore;
        previousScore = score;
        RegisterResult(scoreChange);
    }

    private bool TryReadScore(out int score)
    {
        score = 0;

        if (Score.ScoreInstance == null)
            return false;

        if (scoreText == null)
            scoreText = Score.ScoreInstance.GetComponent<TMP_Text>();

        if (scoreText == null)
            return false;

        string displayedScore = scoreText.text;
        int colonPosition = displayedScore.LastIndexOf(':');

        if (colonPosition >= 0)
            displayedScore = displayedScore.Substring(colonPosition + 1);

        return int.TryParse(displayedScore.Trim(), out score);
    }

    public void AddCombo()
    {
        SetCombo(CurrentCombo + 1);
        speedUptheWave();
    }

    public void BreakCombo()
    {
        animationSpeed = startingAnimationSpeed;
        waveSpacing = startingWaveSpacing;
        movementAmplitude = startingMovementAmplitude;
        SetCombo(0);
    }

    public void SetCombo(int combo)
    {
        if (tmp == null)
            return;

        // Repeated calls must not restart the break effect.
        if (combo == currentCombo)
            return;

        int previousCombo = currentCombo;
        currentCombo = combo;

        // Break only when a visible combo drops to zero.
        if (combo == 0 && previousCombo >= 1)
        {
            isBreaking = true;
            breakDistance = 0f;
            return;
        }

        // Any other combo change cancels the break.
        isBreaking = false;
        breakDistance = 0f;

        tmp.gameObject.SetActive(combo >= 1);// Only show the text when combo is at least 5 ( for testing, set to 1).

        if (combo >= 1)
            tmp.text = $"COMBO x{combo}!";
    }

    private void Update()
    {
        UpdateComboFromScore();

        if (tmp == null || !tmp.gameObject.activeInHierarchy)
            return;

        if (isBreaking)
        {
            breakDistance += breakSpeed * Time.deltaTime;

            if (breakDistance >= hideDistance)
            {
                isBreaking = false;
                tmp.gameObject.SetActive(false);
                return;
            }
        }

        // Restore the base mesh before applying this frame's offsets.
        tmp.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmp.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            Vector3[] verts =
                textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                int vertexIndex = charInfo.vertexIndex + j;
                Vector3 orig = verts[vertexIndex];

                if (isBreaking)
                {
                    float direction = i % 2 == 0 ? -1f : 1f;

                    float moveX = direction * breakDistance * 0.3f;

                    // Slightly different launch heights for each letter
                    float launch = 1.5f + (i % 3) * 0.3f;

                    float moveY =
                        launch * breakDistance
                        - 0.01f * breakDistance * breakDistance;

                    verts[vertexIndex] =
                        orig + new Vector3(moveX, moveY, 0f);
                }
                else
                {
                    float waveY = Mathf.Sin(
                        Time.time * animationSpeed +
                        orig.x * waveSpacing
                    ) * movementAmplitude;

                    verts[vertexIndex] =
                        orig + new Vector3(0f, waveY, 0f);
                }
            }
        }

        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
