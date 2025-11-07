using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad = "FPSControllerScene"; // Editable in Inspector

    // Called when Play button is pressed
    public void PlayGame()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name is empty! Please assign a scene in the inspector.");
        }
    }

    // Called when Quit button is pressed
    public void QuitGame()
    { 
        Application.Quit();
    }
}
