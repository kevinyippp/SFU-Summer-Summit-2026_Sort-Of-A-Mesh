using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboBinShuffle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ComboDisplay combo;
    [SerializeField] private CountdownTimer timer;
    [SerializeField] private TrashCan[] bins;

    [Header("Trigger")]
    [SerializeField, Min(1)] private int triggerCombo = 15;

    [Header("Animation")]
    [SerializeField, Range(2, 10)] private int shuffleRounds = 5;
    [SerializeField, Min(0.1f)] private float swapDuration = 0.45f;
    [SerializeField, Min(0f)] private float crossingArc = 0.7f;
    [SerializeField] private CDWarningAnimation warningAnimation;

    public bool IsShuffling { get; private set; }

    private bool triggeredThisStreak;
    private readonly List<Collider2D> lockedColliders = new List<Collider2D>();
    private readonly List<Behaviour> lockedBehaviours = new List<Behaviour>();
    private Vector3[] startPositions;
    private Vector3[] startScales;

    private void Start()
    {
        if (combo == null) combo = FindAnyObjectByType<ComboDisplay>();
        if (timer == null) timer = FindAnyObjectByType<CountdownTimer>();
        if (bins == null || bins.Length == 0) bins = FindObjectsByType<TrashCan>();
        if (warningAnimation == null)
            warningAnimation = FindAnyObjectByType<CDWarningAnimation>();
    }

    private void LateUpdate()
    {
        if (IsShuffling || combo == null || timer == null || GameManager.IsGameOver)
            return;

        if (combo.CurrentCombo == 0) triggeredThisStreak = false;

        if (!triggeredThisStreak && combo.CurrentCombo >= Mathf.Max(1, triggerCombo))
        {
            triggeredThisStreak = true;
            if (ValidateBins() && !timer.HasFinished)
                StartCoroutine(Shuffle());
        }
    }

    private bool ValidateBins()
    {
        HashSet<TrashCan> unique = new HashSet<TrashCan>();
        if (bins == null || bins.Length < 2)
        {
            Debug.LogWarning("ComboBinShuffle needs at least two bins.", this);
            return false;
        }
        foreach (TrashCan bin in bins)
        {
            if (bin == null || !bin.gameObject.activeInHierarchy || !unique.Add(bin))
            {
                Debug.LogWarning("Assign distinct, active bins to ComboBinShuffle.", this);
                return false;
            }
        }
        return true;
    }

    private IEnumerator Shuffle()
    {
        IsShuffling = true;
        timer.PauseTimer();
        LockBehaviour(warningAnimation);

        foreach (DragObject2D draggable in FindObjectsByType<DragObject2D>())
        {
            LockBehaviour(draggable);
            LockColliders(draggable.gameObject);
        }
        foreach (LargeTrashBin source in FindObjectsByType<LargeTrashBin>())
        {
            LockBehaviour(source);
            LockColliders(source.gameObject);
        }
        foreach (TrashCan bin in bins) LockColliders(bin.gameObject);

        int count = bins.Length;
        startPositions = new Vector3[count];
        startScales = new Vector3[count];
        int[] order = new int[count];
        for (int i = 0; i < count; i++)
        {
            startPositions[i] = bins[i].transform.position;
            startScales[i] = bins[i].transform.localScale;
            order[i] = i;
        }

        int rounds = Mathf.Clamp(shuffleRounds, 2, 10);
        Vector3[] roundStarts = new Vector3[count];
        Vector3[] targets = new Vector3[count];
        Vector3[] arcs = new Vector3[count];
        for (int round = 0; round < rounds; round++)
        {
            // Re-pair the bins each round, like a shell-game shuffle.
            for (int i = count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = order[i];
                order[i] = order[j];
                order[j] = temp;
            }
            for (int i = 0; i < count; i++)
            {
                if (bins[i] == null)
                {
                    Restore(true);
                    if (timer != null && !GameManager.IsGameOver) timer.ResumeTimer();
                    yield break;
                }
                roundStarts[i] = bins[i].transform.position;
                targets[i] = roundStarts[i];
                arcs[i] = Vector3.zero;
            }
            for (int pair = 0; pair + 1 < count; pair += 2)
            {
                int a = order[pair];
                int b = order[pair + 1];
                targets[a] = roundStarts[b];
                targets[b] = roundStarts[a];
                Vector3 delta = roundStarts[b] - roundStarts[a];
                Vector3 sideways = new Vector3(-delta.y, delta.x, 0f).normalized;
                float side = Random.value < 0.5f ? -1f : 1f;
                // Opposite arcs send one bin above and the other below the crossing.
                arcs[a] = sideways * Mathf.Max(0f, crossingArc) * side;
                arcs[b] = -arcs[a];
            }

            float elapsed = 0f;
            float duration = Mathf.Max(0.1f, swapDuration);
            while (elapsed < duration)
            {
                if (GameManager.IsGameOver || timer == null)
                {
                    Restore(true);
                    yield break;
                }
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, progress);
                for (int i = 0; i < count; i++)
                {
                    if (bins[i] == null) continue;
                    bins[i].transform.position =
                        Vector3.Lerp(roundStarts[i], targets[i], eased)
                        + arcs[i] * Mathf.Sin(eased * Mathf.PI);
                }
                yield return null;
            }
            for (int i = 0; i < count; i++)
                if (bins[i] != null) bins[i].transform.position = targets[i];
        }

        Restore(false);
        // ResetTimer also starts the timer and preserves its current round duration.
        if (timer != null && !GameManager.IsGameOver) timer.ResetTimer();
    }

    private void LockBehaviour(Behaviour component)
    {
        if (component == null || !component.enabled) return;
        lockedBehaviours.Add(component);
        component.enabled = false;
    }

    private void LockColliders(GameObject target)
    {
        foreach (Collider2D collider in target.GetComponentsInChildren<Collider2D>())
        {
            if (!collider.enabled) continue;
            lockedColliders.Add(collider);
            collider.enabled = false;
        }
    }

    private void Restore(bool cancel)
    {
        if (startScales != null)
        {
            for (int i = 0; i < startScales.Length; i++)
            {
                if (bins[i] == null) continue;
                bins[i].transform.localScale = startScales[i];
                if (cancel) bins[i].transform.position = startPositions[i];
            }
        }
        Physics2D.SyncTransforms();
        foreach (Collider2D collider in lockedColliders)
            if (collider != null) collider.enabled = true;
        foreach (Behaviour component in lockedBehaviours)
            if (component != null) component.enabled = true;
        lockedColliders.Clear();
        lockedBehaviours.Clear();
        startScales = null;
        IsShuffling = false;
    }

    private void OnDisable()
    {
        if (!IsShuffling) return;
        StopAllCoroutines();
        Restore(true);
        if (timer != null && !GameManager.IsGameOver && !timer.HasFinished)
            timer.ResumeTimer();
    }
}
