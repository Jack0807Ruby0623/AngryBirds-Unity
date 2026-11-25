using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance; // 单例

    [SerializeField] private AudioClip bgmClip; // 在 Inspector 中拖入你的 BGM
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        // 单例模式，确保只有一个 BGMManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换时不销毁
        }
        else
        {
            Destroy(gameObject); // 防止重复
            return;
        }

        // 添加 AudioSource 组件
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.volume = volume;
        audioSource.loop = true; // 循环播放
        audioSource.playOnAwake = true; // 启动时自动播放
        audioSource.spatialBlend = 0f; // 2D 音效
    }

    // 可选：提供外部控制方法
    public void SetVolume(float vol)
    {
        audioSource.volume = Mathf.Clamp01(vol);
    }

    public void Play()
    {
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }
}
