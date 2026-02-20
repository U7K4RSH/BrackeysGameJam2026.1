using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";

    public void Play()
    {
        Time.timeScale = 1f; // safety in case you paused in last run
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