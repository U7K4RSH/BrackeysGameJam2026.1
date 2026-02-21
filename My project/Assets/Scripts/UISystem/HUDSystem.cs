using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;

public class SimpleHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCounterLabel; 
    [SerializeField] private TMP_Text pauseButtonLabel;
    [SerializeField] private GameObject winPanel;

    
    [SerializeField] private GameObject keyIcon;
    [SerializeField] private GameObject keyHalfAIcon;
    [SerializeField] private GameObject keyHalfBIcon;

    private bool isPaused = false;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float dialogueAutoHideSeconds = 2.5f;

    private Coroutine dialogueRoutine;
    public static SimpleHUD Instance { get; private set; }

    private void Awake()
    {
        
        Instance = this;
    }
    private void Start()
    {
        if (keyIcon != null) keyIcon.SetActive(false);
        if (keyHalfAIcon != null) keyHalfAIcon.SetActive(false);
        if (keyHalfBIcon != null) keyHalfBIcon.SetActive(false);
        if (dialoguePanel != null && !dialoguePanel.activeSelf)
            dialoguePanel.SetActive(false);
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

        if(MusicPlayer.Instance != null)
            MusicPlayer.Instance.PlayWinLoop();

        Time.timeScale = 0f;
    }

    public void ShowDialogue(string message, float? seconds = null)
    {
        if (dialoguePanel == null || dialogueText == null) return;

        dialoguePanel.SetActive(true);
        dialogueText.text = message;

        if (dialogueRoutine != null) StopCoroutine(dialogueRoutine);

        float t = seconds ?? dialogueAutoHideSeconds;
        dialogueRoutine = StartCoroutine(HideDialogueAfter(t));
    }

    private IEnumerator HideDialogueAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}
