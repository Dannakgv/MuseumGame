using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;

    private static MusicManager instance;

    void Awake()
    {
        // Check for duplicate instances of MusicManager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep the MusicManager between scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate MusicManager
        }
    }

    void Start()
    {
        // Automatically assign the AudioSource if it's missing
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        // Start playing the music if it's not already playing
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    // This function is called when a scene is loaded
    void OnLevelWasLoaded(int level)
    {
        // Reassign the AudioSource after scene change
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }
}
