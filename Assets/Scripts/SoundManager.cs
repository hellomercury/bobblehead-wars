using UnityEngine;

public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// Single instance of the sound manager to be referenced.
    /// </summary>
    public static SoundManager Instance { get; private set; }

    /// <summary>
    /// Audio clip when firing gun.
    /// </summary>
    public AudioClip GunFire;

    /// <summary>
    /// Audio clip when firing upgraded gun.
    /// </summary>
    public AudioClip UpgradedGunFire;

    /// <summary>
    /// Audio clip when being hurt by alien.
    /// </summary>
    public AudioClip Hurt;

    /// <summary>
    /// Audio clip when alien dies.
    /// </summary>
    public AudioClip AlienDeath;

    /// <summary>
    /// Audio clip when marine dies.
    /// </summary>
    public AudioClip MarineDeath;

    /// <summary>
    /// Audio clip when game is won.
    /// </summary>
    public AudioClip Victory;

    /// <summary>
    /// Audio clip when elevator arrived.
    /// </summary>
    public AudioClip ElevatorArrived;

    /// <summary>
    /// Audio clip when picking up power up.
    /// </summary>
    public AudioClip PowerUpPickup;

    /// <summary>
    /// Audio clip when power up appeared.
    /// </summary>
    public AudioClip PowerUpAppear;

    /// <summary>
    /// Audio source for playing effect sounds that is attached to this game object.
    /// </summary>
    private AudioSource _soundEffectAudio;

    private void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    private void Start()
    {
        var audioSources = GetComponents<AudioSource>();
        foreach (var source in audioSources)
        {
            if (source.clip == null)
            {
                _soundEffectAudio = source;
            }
        }
    }

    /// <summary>
    /// Play an audio clip.
    /// </summary>
    /// <param name="audioClip">The audio clip to be played</param>
    public void PlayOneShot(AudioClip audioClip)
    {
        _soundEffectAudio.PlayOneShot(audioClip);
    }
}