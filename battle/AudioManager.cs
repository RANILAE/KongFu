using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("��ƵԴ����")]
    public AudioSource soundEffectSource;
    public AudioSource backgroundMusicSource;

    private BattleConfig config;

    public void Initialize(BattleConfig config)
    {
        this.config = config;

        // ���û��������ƵԴ������Ĭ�����
        if (soundEffectSource == null) soundEffectSource = gameObject.AddComponent<AudioSource>();
        if (backgroundMusicSource == null) backgroundMusicSource = gameObject.AddComponent<AudioSource>();

        // ������ƵԴ
        backgroundMusicSource.loop = true;
        soundEffectSource.loop = false;
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        if (backgroundMusicSource == null) return;
        if (music == null) return; // û����������ʱ������

        backgroundMusicSource.clip = music;
        backgroundMusicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource == null) return;
        backgroundMusicSource.Stop();
    }

    public void PlaySoundEffect(AudioClip sound)
    {
        if (soundEffectSource == null) return;
        if (sound == null) return; // û��������Чʱ������

        soundEffectSource.PlayOneShot(sound);
    }
}