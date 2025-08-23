using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I;
    public AudioClip flipClip, matchClip, mismatchClip, gameOverClip;
    AudioSource src;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
        src = gameObject.AddComponent<AudioSource>();
    }

    public void PlayFlip() { src.PlayOneShot(flipClip); }
    public void PlayMatch() { src.PlayOneShot(matchClip); }
    public void PlayMismatch() { src.PlayOneShot(mismatchClip); }
    public void PlayGameOver() { src.PlayOneShot(gameOverClip); }
}