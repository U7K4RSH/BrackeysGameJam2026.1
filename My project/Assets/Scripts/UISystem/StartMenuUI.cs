using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartMenuUI : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Intro Panel")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private RectTransform introTextRect; // RectTransform of TMP text
    [SerializeField] private float scrollDuration = 6f;
    [SerializeField] private float startY = -700f; // below screen
    [SerializeField] private float endY = 700f;    // above screen

    private bool starting = false;

    public void Play()
    {
        if (starting) return;
        starting = true;

        StartCoroutine(PlayIntroThenStart());
    }

    private IEnumerator PlayIntroThenStart()
    {
        // show panel
        if (introPanel != null)
            introPanel.SetActive(true);

        // reset text position to bottom
        if (introTextRect != null)
        {
            Vector2 p = introTextRect.anchoredPosition;
            p.y = startY;
            introTextRect.anchoredPosition = p;
        }

        // scroll up over time
        float t = 0f;
        while (t < scrollDuration)
        {
            t += Time.unscaledDeltaTime;

            float a = Mathf.Clamp01(t / scrollDuration);
            float y = Mathf.Lerp(startY, endY, a);

            if (introTextRect != null)
            {
                Vector2 p = introTextRect.anchoredPosition;
                p.y = y;
                introTextRect.anchoredPosition = p;
            }

            yield return null;
        }

        // load game
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