using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Battle Sounds")]
    public AudioClip attackSound;
    public AudioClip damageSound;
    public AudioClip chargeSound;
    public AudioClip defenseSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    void OnEnable()
    {
        BattleEventSystem.OnPlaySound.AddListener(PlaySound);
    }

    void OnDisable()
    {
        BattleEventSystem.OnPlaySound.RemoveListener(PlaySound);
    }
}