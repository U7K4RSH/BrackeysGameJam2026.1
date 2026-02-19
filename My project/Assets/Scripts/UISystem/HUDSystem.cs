using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SimpleHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCounterLabel; 
    [SerializeField] private TMP_Text pauseButtonLabel;
    [SerializeField] private GameObject winPanel;

    
    [SerializeField] private GameObject keyIcon;
    [SerializeField] private GameObject keyHalfAIcon;
    [SerializeField] private GameObject keyHalfBIcon;

    private bool isPaused = false;



    private void Start()
    {
        if (keyIcon != null) keyIcon.SetActive(false);
        if (keyHalfAIcon != null) keyHalfAIcon.SetActive(false);
        if (keyHalfBIcon != null) keyHalfBIcon.SetActive(false);
    }

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


    public void ShowFullKeyIcon()
    {
        if (keyIcon != null)
            keyIcon.SetActive(true);

        // Optional: hide halves once complete
        if (keyHalfAIcon != null) keyHalfAIcon.SetActive(false);
        if (keyHalfBIcon != null) keyHalfBIcon.SetActive(false);
    }

    public void ShowHalfAIcon()
    {
        if (keyHalfAIcon != null)
            keyHalfAIcon.SetActive(true);
    }

    public void ShowHalfBIcon()
    {
        if (keyHalfBIcon != null)
            keyHalfBIcon.SetActive(true);
    }

    public void HideKeyIcon()
    {
        if (keyIcon != null)
            keyIcon.SetActive(false);
    }

    public void ShowWin()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }
}
