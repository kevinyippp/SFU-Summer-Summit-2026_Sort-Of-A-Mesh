using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] public AudioClip correctDropSound;
    [SerializeField] public AudioClip incorrectDropSound;
    [SerializeField] public AudioClip backgroundMusic;
    [SerializeField] public AudioClip gameOverSound;

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

        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayBackgroundMusic();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBackgroundMusic();
    }

    private void PlayBackgroundMusic()
    {
        if (backgroundMusic == null)
        {
            Debug.LogError("Background music not found");
            return;
        }

        audioSource.Stop();
        audioSource.clip = backgroundMusic;
        // This doesn't affect the other sounds, since they use PlayOneShot (which ignores loop)
        audioSource.loop = true;
        audioSource.Play();
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

        audioSource.PlayOneShot(scoreIncreaseSounds[soundIndex], 2.0f);
    }

    public void PlaySound(AudioClip clip, float delay = 0f, float volumeScale = 1.0f)
    {
        if (clip == null)
        {
            return;
        }
        
        if (delay > 0f)
            StartCoroutine(PlaySoundDelayed(clip, delay, volumeScale));
        else
            audioSource.PlayOneShot(clip, volumeScale);
        
    }

    private IEnumerator PlaySoundDelayed(AudioClip clip, float delay, float volumeScale = 1.0f)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip, volumeScale);
    }

}
