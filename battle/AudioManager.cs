using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("音频源引用")]
    public AudioSource soundEffectSource;
    public AudioSource backgroundMusicSource;

    private BattleConfig config;

    public void Initialize(BattleConfig config)
    {
        this.config = config;

        // 如果没有设置音频源，创建默认组件
        if (soundEffectSource == null) soundEffectSource = gameObject.AddComponent<AudioSource>();
        if (backgroundMusicSource == null) backgroundMusicSource = gameObject.AddComponent<AudioSource>();

        // 配置音频源
        backgroundMusicSource.loop = true;
        soundEffectSource.loop = false;
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        if (backgroundMusicSource == null) return;
        if (music == null) return; // 没有配置音乐时不播放

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
        if (sound == null) return; // 没有配置音效时不播放

        soundEffectSource.PlayOneShot(sound);
    }
}