using UnityEngine;
using UnityEngine.SceneManagement;
// scenes load/reload karne ke kaam aata

public class GameManager : MonoBehaviour
{
    private void Update()
    {
        // Update har frame chalega (matlab continuously check karega keys)

        // R dabaya toh current scene reset / reload
        // game jam mein testing ke liye mast shortcut
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // GetActiveScene().buildIndex = jo scene abhi chal raha hai uska number
        // LoadScene = same scene dubara load => full reset

        // Escape dabaya toh game band
        // Editor mein quit nahi hota properly, build mein hota hai
        if (Input.GetKeyDown(KeyCode.Escape))
            UnityEngine.Application.Quit();
        // UnityEngine.Application likhna pada kyuki 'Application' naam ka conflict aa raha tha
    }
}
