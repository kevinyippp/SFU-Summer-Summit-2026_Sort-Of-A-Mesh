using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    [SerializeField] private ComboDisplay comboDisplay;

    public int CurrentCombo { get; private set; }

    private void Awake()
    {
        if (comboDisplay == null)
        {
            comboDisplay = FindAnyObjectByType<ComboDisplay>(
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
            Debug.LogError("ComboSystem could not find an ComboDisplay combo display.");
            return;
        }

        comboDisplay.SetCombo(CurrentCombo);
    }
}
