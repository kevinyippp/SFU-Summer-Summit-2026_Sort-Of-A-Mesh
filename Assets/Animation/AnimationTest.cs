using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class AnimationTest : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp;

    [Header("Wave")]
    [SerializeField] private float animationSpeed = 6f;
    [SerializeField] private float waveSpacing = 0.02f;
    [SerializeField] private float movementAmplitude = 30f;

    [Header("Break")]
    [SerializeField] private float breakSpeed = 200f;
    [SerializeField] private float hideDistance = 500f;

    private int currentCombo = -1;
    private bool isBreaking;
    private float breakDistance;

    void Start()
    {
        // Change to SetCombo(0) when finished testing.
        SetCombo(25);
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
        if (combo == 0 && previousCombo >= 5)
        {
            isBreaking = true;
            breakDistance = 0f;
            return;
        }

        // Any other combo change cancels the break.
        isBreaking = false;
        breakDistance = 0f;

        tmp.gameObject.SetActive(combo >= 5);

        if (combo >= 5)
            tmp.text = $"COMBO x{combo}!";
    }

    void Update()
    {
        // Temporary testing: Space breaks, Enter restores.
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                SetCombo(0);

            if (Keyboard.current.enterKey.wasPressedThisFrame)
                SetCombo(25);
        }

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