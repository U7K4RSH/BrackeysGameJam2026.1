using UnityEngine;

public class KeyUI : MonoBehaviour
{
    [SerializeField] private GameObject keyIcon; 

    private void Start()
    {
        if (keyIcon != null)
            keyIcon.SetActive(false);
    }

    public void ShowKeyIcon()
    {
        if (keyIcon != null)
            keyIcon.SetActive(true);
    }
}
