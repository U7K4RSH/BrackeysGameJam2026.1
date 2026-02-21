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

    [Header("Timing")]
    [SerializeField] private float scrollDuration = 5f;
    [SerializeField] private float holdAfterFinal = 2f;

    [Header("Scroll Positions")]
    [SerializeField] private float startY = -800f; // start below screen
    [SerializeField] private float endY = 120f;    // end slightly above center (so final line has space)

    private bool starting = false;

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

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
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