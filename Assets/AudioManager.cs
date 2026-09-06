using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] public AudioClip correctDropSound;
    [SerializeField] public AudioClip incorrectDropSound;
    [SerializeField] public AudioClip backgroundMusic;

    [SerializeField] private AudioClip[] scoreIncreaseSounds;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            // This doesn't affect the other sounds, since they use PlayOneShot (which ignores loop)
            audioSource.loop = true;
            audioSource.Play();
        } else
        {
            Debug.LogError("Background music not found");
        }

    }

    public void PlayScoreIncrease()
    {
        int maxIndex = scoreIncreaseSounds.Length - 1;

        if (maxIndex < 0)
        {
            Debug.LogError("No score increase sounds found");
            return;
        }

        int soundIndex = Random.Range(0, maxIndex);

        audioSource.PlayOneShot(scoreIncreaseSounds[soundIndex]);
    }

    public void PlaySound(AudioClip clip, float delay = 0f)
    {
        if (clip == null)
        {
            return;
        }
        
        if (delay > 0f)
            StartCoroutine(PlaySoundDelayed(clip, delay));
        else
            audioSource.PlayOneShot(clip);
        
    }

    private IEnumerator PlaySoundDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip);
    }

}
