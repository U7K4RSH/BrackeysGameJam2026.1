using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeTransitionManager : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;
    private CanvasGroup canvasGroup;
    private Canvas fadeCanvas;
    private static FadeTransitionManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Create Canvas if it doesn't exist
        if (fadeCanvas == null)
        {
            CreateFadeCanvas();
        }
    }

    private void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.zero;

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // High sort order to appear on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Create fade image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        imageObj.transform.localPosition = Vector3.zero;

        Image fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = Color.black;

        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    public static FadeTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("FadeTransitionManager");
                instance = obj.AddComponent<FadeTransitionManager>();
            }
            return instance;
        }
    }

    // Expose configured fade duration for callers that need to wait for transitions
    public float FadeDuration => fadeDuration;

    public Coroutine FadeOut(System.Action onFadeOutComplete = null)
    {
        return StartCoroutine(FadeOutCoroutine(onFadeOutComplete));
    }

    public Coroutine FadeIn(System.Action onFadeInComplete = null)
    {
        return StartCoroutine(FadeInCoroutine(onFadeInComplete));
    }

    private IEnumerator FadeOutCoroutine(System.Action onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        onComplete?.Invoke();
    }

    private IEnumerator FadeInCoroutine(System.Action onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        onComplete?.Invoke();
    }

    public void SetFadeDuration(float duration)
    {
        fadeDuration = duration;
    }
}
