using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    private AudioSource src;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();
        src.playOnAwake = true;
        src.loop = true;
        src.spatialBlend = 0f; // 2D
    }

    public void SetVolume(float v)
    {
        if (src != null) src.volume = Mathf.Clamp01(v);
    }

    public void Play()
    {
        if (src != null && !src.isPlaying) src.Play();
    }

    public void Stop()
    {
        if (src != null && src.isPlaying) src.Stop();
    }

    public void Pause()
    {
        if (src != null && src.isPlaying) src.Pause();
    }

    public void UnPause()
    {
        if (src != null) src.UnPause();
    }
}

