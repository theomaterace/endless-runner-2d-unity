using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip hit;

    [Header("Audio")]
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.6f;
    [SerializeField] private AudioSource oneShotSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Jeœli nie podpinasz w Inspectorze, utwórz AudioSource automatycznie.
        if (oneShotSource == null)
        {
            oneShotSource = gameObject.AddComponent<AudioSource>();
        }

        // Kluczowe: ¿adnego grania na starcie.
        oneShotSource.playOnAwake = false;
        oneShotSource.loop = false;
    }

    public void PlayClick()
    {
        PlayOneShot(click);
    }

    public void PlayJump()
    {
        if (jump == null || oneShotSource == null) return;
        oneShotSource.PlayOneShot(jump, jumpVolume);
    }

    public void PlayHit()
    {
        PlayOneShot(hit);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || oneShotSource == null) return;
        oneShotSource.PlayOneShot(clip);
    }
}