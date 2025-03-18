using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Provides access to iOS haptic feedback system.
/// </summary>
public static class Taptic
{
    #if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _TapticLight();
    
    [DllImport("__Internal")]
    private static extern void _TapticMedium();
    
    [DllImport("__Internal")]
    private static extern void _TapticHeavy();
    
    [DllImport("__Internal")]
    private static extern void _TapticSuccess();
    
    [DllImport("__Internal")]
    private static extern void _TapticWarning();
    
    [DllImport("__Internal")]
    private static extern void _TapticFailure();
    #endif
    
    /// <summary>
    /// Triggers a light haptic impact.
    /// </summary>
    public static void Light()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _TapticLight();
        #endif
    }
    
    /// <summary>
    /// Triggers a medium haptic impact.
    /// </summary>
    public static void Medium()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _TapticMedium();
        #endif
    }
    
    /// <summary>
    /// Triggers a heavy haptic impact.
    /// </summary>
    public static void Heavy()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _TapticHeavy();
        #endif
    }
    
    /// <summary>
    /// Triggers a success haptic notification.
    /// </summary>
    public static void Success()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _TapticSuccess();
        #endif
    }
    
    /// <summary>
    /// Triggers a warning haptic notification.
    /// </summary>
    public static void Warning()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _TapticWarning();
        #endif
    }
    
    /// <summary>
    /// Triggers a failure haptic notification.
    /// </summary>
    public static void Failure()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _TapticFailure();
        #endif
    }
}