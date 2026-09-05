using UnityEngine;

[RequireComponent(typeof(Score))]
[RequireComponent(typeof(ComboSystem))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Score scoreSystem;
    private ComboSystem comboSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        scoreSystem = GetComponent<Score>();
        comboSystem = GetComponent<ComboSystem>();
    }

    public void ProcessTrashResult(int points)
    {
        scoreSystem.AddPoints(points);

        if (points > 0)
        {
            comboSystem.AddCombo();
        }
        else
        {
            comboSystem.BreakCombo();
        }
    }
}
