using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartMenuUI : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Intro UI")]
    [SerializeField] private GameObject introPanel;

    [SerializeField] private RectTransform storyTextRect;  // StoryText RectTransform
    [SerializeField] private GameObject finalLineObject;   // FinalLine GameObject

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup; // FULLSCREEN BLACK OVERLAY (CanvasGroup)
    [SerializeField] private float fadeInTime = 0.35f;
    [SerializeField] private float fadeOutTime = 0.35f;

    [Header("Timing")]
    [SerializeField] private float scrollDuration = 5f;
    [SerializeField] private float holdAfterFinal = 2f;

    [Header("Scroll Positions")]
    [SerializeField] private float startY = -800f; // start below screen
    [SerializeField] private float endY = 120f;    // end slightly above center (so final line has space)

    private bool starting = false;

    private void Start()
    {
        Time.timeScale = 1f;

        // start black -> fade in to menu
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;

            // IMPORTANT: while black, block clicks
            fadeGroup.blocksRaycasts = true;
            fadeGroup.interactable = false;

            StartCoroutine(FadeInMenuStart());
        }
    }

    private IEnumerator FadeInMenuStart()
    {
        // IMPORTANT: wait 1 frame so UI settles (removes jitter)
        yield return null;

        // fade from black to clear
        yield return Fade(1f, 0f, fadeInTime);

        // IMPORTANT: when fully clear, allow clicks
        fadeGroup.blocksRaycasts = false;
    }
    public void Play()
    {
        if (starting) return;
        starting = true;

        StartCoroutine(IntroThenStart());
    }

    private IEnumerator IntroThenStart()
    {
        if (introPanel != null)
            introPanel.SetActive(true);

        if (finalLineObject != null)
            finalLineObject.SetActive(false);

        // reset story text to bottom
        if (storyTextRect != null)
        {
            Vector2 p = storyTextRect.anchoredPosition;
            p.y = startY;
            storyTextRect.anchoredPosition = p;
        }

        // scroll story paragraph upward
        float t = 0f;
        while (t < scrollDuration)
        {
            t += Time.unscaledDeltaTime;

            float a = Mathf.Clamp01(t / scrollDuration);
            float y = Mathf.Lerp(startY, endY, a);

            if (storyTextRect != null)
            {
                Vector2 p = storyTextRect.anchoredPosition;
                p.y = y;
                storyTextRect.anchoredPosition = p;
            }

            yield return null;
        }

        // show final line in red (fixed center)
        if (finalLineObject != null)
            finalLineObject.SetActive(true);

        // hold for dramatic beat
        float h = 0f;
        while (h < holdAfterFinal)
        {
            h += Time.unscaledDeltaTime;
            yield return null;
        }

        // fade out to black BEFORE loading the game scene
        if (fadeGroup != null)
            yield return Fade(0f, 1f, fadeOutTime);

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator Fade(float from, float to, float time)
    {
        if (fadeGroup == null) yield break;

        float t = 0f;
        fadeGroup.alpha = from;

        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float a = (time <= 0f) ? 1f : Mathf.Clamp01(t / time);
            fadeGroup.alpha = Mathf.Lerp(from, to, a);
            yield return null;
        }

        fadeGroup.alpha = to;
    }

    public void Exit()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        Debug.Log("Exit pressed (Editor cannot quit via Application.Quit).");
#else
        Application.Quit();
#endif
    }
}