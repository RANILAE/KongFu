using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("��ƵԴ����")]
    public AudioSource soundEffectSource;
    public AudioSource backgroundMusicSource;

    void Awake()
    {
        // ȷ����Startǰ������ƵԴ
        if (soundEffectSource == null) soundEffectSource = gameObject.AddComponent<AudioSource>();
        if (backgroundMusicSource == null) backgroundMusicSource = gameObject.AddComponent<AudioSource>();
    }

    public void Initialize()
    {
        // ������ƵԴ
        backgroundMusicSource.loop = true;
        soundEffectSource.loop = false;

        // ���ú����Ĭ������
        backgroundMusicSource.volume = 0.5f;
        soundEffectSource.volume = 0.7f;
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        if (backgroundMusicSource == null) return;
        if (music == null) return;

        // �򻯣�����Ѿ��ڲ���ͬһ�����֣��Ͳ����²���
        if (backgroundMusicSource.isPlaying && backgroundMusicSource.clip == music)
        {
            return;
        }

        // ֹͣ��ǰ���ֲ�����������
        backgroundMusicSource.Stop();
        backgroundMusicSource.clip = music;
        backgroundMusicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource == null) return;
        if (backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }
    }

    public void PlaySoundEffect(AudioClip sound)
    {
        if (soundEffectSource == null) return;
        if (sound == null) return;

        soundEffectSource.PlayOneShot(sound);
    }

    // ����������BGMϵͳ���������¿�ʼʱ��
    public void ResetBackgroundMusicSystem()
    {
        if (backgroundMusicSource == null) return;
        // ֹֻͣ���֣������clip����
        if (backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }
    }
}