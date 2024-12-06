using UnityEngine;
using UnityEngine.SceneManagement; // To handle scene management
using UnityEngine.UI; // To interact with UI buttons

public class MenuManager : MonoBehaviour
{
    [Header("Menu UI Elements")]
    public GameObject menuPanel; // The menu panel (containing the buttons)

    private bool isPaused = false; // To check if the game is paused

    void Start()
    {
        // Initially hide the menu panel
        menuPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle the menu when pressing the Escape key (or any key of your choice)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame(); // If paused, resume the game
            }
            else
            {
                ShowMenu(); // If not paused, show the menu
            }
        }
    }

    public void ShowMenu()
    {
        menuPanel.SetActive(true); // Show the menu
        Time.timeScale = 0f; // Pause the game by stopping time
        isPaused = true;
    }

    public void ResumeGame()
    {
        menuPanel.SetActive(false); // Hide the menu
        Time.timeScale = 1f; // Resume the game by restoring time
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Ensure the game is running at normal speed
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
    }

    public void ExitGame()
    {
        // Exit the game (useful for builds, not in the editor)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
        #else
        Application.Quit(); // Quit the game if it's a build
        #endif
    }
}
