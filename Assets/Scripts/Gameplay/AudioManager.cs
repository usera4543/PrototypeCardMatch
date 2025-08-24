using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I; // Singleton instance

    // Audio clips for different game events
    public AudioClip flipClip, matchClip, mismatchClip, gameOverClip;

    AudioSource src; // AudioSource component for playing sounds

    void Awake()
    {
        // Add AudioSource component if not already present
        src = gameObject.AddComponent<AudioSource>();
    }

    // Methods to play specific sounds
    public void PlayFlip() { src.PlayOneShot(flipClip); }
    public void PlayMatch() { src.PlayOneShot(matchClip); }
    public void PlayMismatch() { src.PlayOneShot(mismatchClip); }
    public void PlayGameOver() { src.PlayOneShot(gameOverClip); }
}
