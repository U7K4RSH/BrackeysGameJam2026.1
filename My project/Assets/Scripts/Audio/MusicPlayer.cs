using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    private AudioSource src;

    [SerializeField] private AudioClip gameLoop;
    [SerializeField] private AudioClip winLoop;

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

        src.clip = gameLoop;
        src.loop = true;
        src.Play();
    }

    public void PlayWinLoop()
    {
        Debug.Log("PlayWinLoop called. src=" + (src != null) + " winLoop=" + (winLoop != null));
        if (src == null || winLoop == null) return;

        src.clip = winLoop;
        src.loop = true;
        src.Play();
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

