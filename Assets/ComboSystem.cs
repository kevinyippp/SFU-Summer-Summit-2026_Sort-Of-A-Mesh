using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    [SerializeField] private AnimationTest comboDisplay;

    public int CurrentCombo { get; private set; }

    private void Awake()
    {
        if (comboDisplay == null)
        {
            comboDisplay = FindAnyObjectByType<AnimationTest>(
                FindObjectsInactive.Include
            );
        }
    }

    private void Start()
    {
        CurrentCombo = 0;
        UpdateDisplay();
    }

    public void AddCombo()
    {
        CurrentCombo++;
        UpdateDisplay();
    }

    public void BreakCombo()
    {
        if (CurrentCombo == 0)
        {
            return;
        }

        CurrentCombo = 0;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (comboDisplay == null)
        {
            Debug.LogError("ComboSystem could not find an AnimationTest combo display.");
            return;
        }

        comboDisplay.SetCombo(CurrentCombo);
    }
}
