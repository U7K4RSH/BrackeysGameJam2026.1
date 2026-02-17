using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SimpleHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCounterLabel;
    [SerializeField] private TMP_Text pauseButtonLabel;
    [SerializeField] private GameObject winPanel;
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

    public void SetRoomCounter(int roomId)
    {
        if (roomCounterLabel != null)
            roomCounterLabel.text = $"ROOM {roomId}";
        Debug.Log("Room counter set to: " + roomId);
    }
    public void ShowWin()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }
}
