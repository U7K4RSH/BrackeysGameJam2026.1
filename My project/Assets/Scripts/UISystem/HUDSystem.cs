using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SimpleHUD : MonoBehaviour
{
    
    [SerializeField] private TMP_Text pauseButtonLabel;

    private bool isPaused = false;

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseButtonLabel != null)
            pauseButtonLabel.text = isPaused ? "RESUME" : "PAUSE";
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        Debug.Log("Quit pressed (Editor cannot quit via Application.Quit).");
#else
        Application.Quit();
#endif
    }
}
