using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("音频源引用")]
    public AudioSource soundEffectSource;
    public AudioSource backgroundMusicSource;

    void Awake()
    {
        // 确保在Start前创建音频源
        if (soundEffectSource == null) soundEffectSource = gameObject.AddComponent<AudioSource>();
        if (backgroundMusicSource == null) backgroundMusicSource = gameObject.AddComponent<AudioSource>();
    }

    public void Initialize()
    {
        // 配置音频源
        backgroundMusicSource.loop = true;
        soundEffectSource.loop = false;

        // 设置合理的默认音量
        backgroundMusicSource.volume = 0.5f;
        soundEffectSource.volume = 0.7f;
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        if (backgroundMusicSource == null) return;
        if (music == null) return;

        // 简化：如果已经在播放同一首音乐，就不重新播放
        if (backgroundMusicSource.isPlaying && backgroundMusicSource.clip == music)
        {
            return;
        }

        // 停止当前音乐并播放新音乐
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

    // 新增：重置BGM系统（用于重新开始时）
    public void ResetBackgroundMusicSystem()
    {
        if (backgroundMusicSource == null) return;
        // 只停止音乐，不清除clip引用
        if (backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }
    }
}