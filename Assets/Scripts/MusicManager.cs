using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages background music across different scenes in the game.
/// Ensures music plays continuously during scene transitions.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    private static MusicManager instance;
    
    /// <summary>
    /// Sets up the singleton pattern to maintain a single music manager across scenes.
    /// </summary>
    void Awake()
    {
        // Check for duplicate instances of MusicManager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep the MusicManager between scenes
            SceneManager.sceneLoaded += OnSceneLoaded; // Register for scene change events
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate MusicManager
        }
    }
    
    /// <summary>
    /// Initializes the audio source and starts playing music.
    /// </summary>
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
    
    /// <summary>
    /// Called when a scene is loaded. Uses the newer SceneManager API.
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ensure the AudioSource is assigned
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }
        
        // Make sure music continues playing and doesn't restart
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
    
    /// <summary>
    /// This function is called when a scene is loaded using the legacy API.
    /// Kept for backward compatibility.
    /// </summary>
    void OnLevelWasLoaded(int level)
    {
        // Reassign the AudioSource after scene change
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// Stops the background music.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    /// <summary>
    /// Unregisters scene load event when this object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Provides access to the singleton instance.
    /// </summary>
    public static MusicManager Instance
    {
        get { return instance; }
    }
}
