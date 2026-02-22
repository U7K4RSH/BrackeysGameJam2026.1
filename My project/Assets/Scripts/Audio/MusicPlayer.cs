using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    private AudioSource src;

    [SerializeField] private AudioClip darkRoomLoop;
    [SerializeField] private AudioClip gameLoop;
    [SerializeField] private AudioClip winLoop;

    [Header("Music Volumes (0-1)")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float gameLoopVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float darkLoopVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float winLoopVolume = 0.8f;

    private void Start()
    {
        PlayGameLoop();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // IMPORTANT: grab AudioSource every time
        src = GetComponent<AudioSource>();

        // force it to behave like music
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f; // 2D
    }

    public void SetVolume(float v)
    {
        if (src != null) src.volume = Mathf.Clamp01(v);
    }

    public void PlayGameLoop()
    {
        if (src == null || gameLoop == null) return;

        if (src.isPlaying && src.clip == gameLoop) return;

        src.Stop();
        src.clip = gameLoop;
        src.loop = true;
        ApplyVolume(gameLoopVolume);
        src.Play();
    }

    public void PlayWinLoop()
    {
        Debug.Log("PlayWinLoop called. src=" + (src != null) + " winLoop=" + (winLoop != null));
        if (src == null || winLoop == null) return;

        if (src.isPlaying && src.clip == winLoop) return;

        src.Stop();
        src.clip = winLoop;
        src.loop = true;
        ApplyVolume(winLoopVolume);
        src.Play();
    }

    private void ApplyVolume(float trackVolume)
    {
        if (src == null) return;
        src.volume = Mathf.Clamp01(masterVolume * trackVolume);
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

    public void PlayDarkRoomLoop()
    {
        if (src == null || darkRoomLoop == null) return;

        if (src.isPlaying && src.clip == darkRoomLoop) return;

        src.Stop();
        src.clip = darkRoomLoop;
        src.loop = true;
        ApplyVolume(darkLoopVolume);
        src.Play();
    }
}

