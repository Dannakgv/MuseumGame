using UnityEngine;

/// <summary>
/// Manages haptic feedback throughout the game with different intensities.
/// </summary>
public class HapticManager : MonoBehaviour
{
    private static HapticManager instance;

    [Header("Sound Feedback Settings")]
    [SerializeField] private AudioSource hapticAudioSource;
    [SerializeField] private AudioClip lightFeedbackSound;
    [SerializeField] private AudioClip mediumFeedbackSound;
    [SerializeField] private AudioClip heavyFeedbackSound;
    [SerializeField] private AudioClip successFeedbackSound;
    
    [Header("Haptic Settings")]
    [SerializeField] private bool enableHaptics = true;
    
    /// <summary>
    /// Sets up the singleton pattern for the HapticManager.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create audio source if not assigned
            if (hapticAudioSource == null)
            {
                hapticAudioSource = gameObject.AddComponent<AudioSource>();
                hapticAudioSource.playOnAwake = false;
                hapticAudioSource.spatialBlend = 0f; // 2D sound
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Generate light haptic feedback for subtle interactions.
    /// </summary>
    public void LightFeedback()
    {
        if (!enableHaptics) return;
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
        #elif UNITY_IOS && !UNITY_EDITOR
        Taptic.Light();
        #endif
        
        // Play sound feedback as fallback on platforms without haptics
        if (lightFeedbackSound != null && hapticAudioSource != null)
        {
            hapticAudioSource.PlayOneShot(lightFeedbackSound, 0.5f);
        }
        
        Debug.Log("Light haptic feedback");
    }
    
    /// <summary>
    /// Generate medium haptic feedback for standard interactions.
    /// </summary>
    public void MediumFeedback()
    {
        if (!enableHaptics) return;
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
        #elif UNITY_IOS && !UNITY_EDITOR
        Taptic.Medium();
        #endif
        
        if (mediumFeedbackSound != null && hapticAudioSource != null)
        {
            hapticAudioSource.PlayOneShot(mediumFeedbackSound, 0.7f);
        }
        
        Debug.Log("Medium haptic feedback");
    }
    
    /// <summary>
    /// Generate heavy haptic feedback for significant interactions.
    /// </summary>
    public void HeavyFeedback()
    {
        if (!enableHaptics) return;
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
        #elif UNITY_IOS && !UNITY_EDITOR
        Taptic.Heavy();
        #endif
        
        if (heavyFeedbackSound != null && hapticAudioSource != null)
        {
            hapticAudioSource.PlayOneShot(heavyFeedbackSound, 0.9f);
        }
        
        Debug.Log("Heavy haptic feedback");
    }
    
    /// <summary>
    /// Generate success haptic feedback pattern for positive outcomes.
    /// </summary>
    public void SuccessFeedback()
    {
        if (!enableHaptics) return;
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Create a success pattern on Android
        long[] pattern = { 0, 100, 50, 100, 50, 100 };
        int[] amplitudes = { 0, 80, 0, 120, 0, 255 };
        if (AndroidVibrateEffect.HasAmplitudeControl())
            AndroidVibrateEffect.Vibrate(pattern, amplitudes, -1);
        else
            Handheld.Vibrate();
        #elif UNITY_IOS && !UNITY_EDITOR
        Taptic.Success();
        #endif
        
        if (successFeedbackSound != null && hapticAudioSource != null)
        {
            hapticAudioSource.PlayOneShot(successFeedbackSound, 1.0f);
        }
        
        Debug.Log("Success haptic feedback");
    }
    
    /// <summary>
    /// Enable or disable haptic feedback.
    /// </summary>
    public void SetHapticsEnabled(bool enabled)
    {
        enableHaptics = enabled;
    }
    
    /// <summary>
    /// Provides access to the singleton instance.
    /// </summary>
    public static HapticManager Instance
    {
        get { return instance; }
    }
}