using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel; // Assign your pause panel in the Inspector

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu"; // Set your main menu scene name here

    private bool isPaused = false;

    void Start()
    {
        // Ensure the game starts unpaused
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    void Update()
    {
        // Toggle pause on ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None; // Unlock cursor for UI
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Re-lock cursor if you use first-person controls
        Cursor.visible = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reset time before changing scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
