using UnityEngine;
using TMPro;


public class AnimationTest : MonoBehaviour
{

    public TMP_Text tmp;
    [SerializeField] private float animationSpeed = 20f;
    [SerializeField] private float waveSpacing = 1f;
    [SerializeField] private float movementAmplitude = 3f;

    void Start()
    {
        Debug.Log("AnimationTest is running!");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        //refresh the mesh to get the latest character info
        tmp.ForceMeshUpdate();


        var textInfo = tmp.textInfo;

        // Loop through each character in the texts
        for (int i = 0; i < textInfo.characterCount; i++)
        {

            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible)
            {
                continue;
            }

            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            // Loop through the 4 vertices of the character's quad
            for (int j = 0; j < 4/*todo*/; j++)
            {
                var orig = verts[charInfo.vertexIndex + j];


                verts[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time * animationSpeed + orig.x * waveSpacing) * movementAmplitude, 0);
            }

        }
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            tmp.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
