using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // UI Elements
    public GameObject playScreen;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI timerText;  // New TextMeshPro for timer display

    public PlayerInput playerInput;

    public MonoBehaviour scriptToDisable; // Reference to the specific script you want to disable

    public GameObject gameOverScreen; // Reference to the Game Over UI

    public GameObject HealthBar;  // Reference to the HealthBar GameObject

    // Music
    public AudioClip playMusicClip;  // Background music when the game is playing
    public AudioClip gameOverMusicClip;  // Music when the game is over
    public AudioClip startGameMusicClip; // New music for when the game starts
    public AudioSource playerAudioSource; // Reference to the Player's AudioSource
    public AudioClip endScreenMusicClip; // Reference to the end screen music


    // Light2D GameObject
    public GameObject light2D; // Reference to the light2D GameObject

    // Timer Variables
    private float elapsedTime = 0f; // Timer variable
    private bool stopwatchIsRunning = false; // Controls whether the stopwatch is active

    public GameObject Light;

    public GameObject endScreen;  // Reference to the end screen UI
    public TextMeshProUGUI finalTimeText; // Reference to display the final time
    public Button startButton;

    private void Awake()
    {
        // Ensure only one instance of UIManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy the duplicate instance
        }
    }

    // Method to update health display
    public void UpdateHealthDisplay(int health)
    {
        healthText.text = $"Health: {health}";
    }

    // Show the play screen and manage related UI and game states
    public void ShowPlayScreen()
    {
        playScreen.SetActive(true);

        Light.SetActive(false);

        // Disable the Player script when the Play Screen is shown
        DisablePlayerScript();

        // Stop all enemies
        StopAllEnemies();

        // Play play screen music
        PlayBackgroundMusic(playMusicClip);
    }

    // Show the end screen and display the final time
    public void ShowEndScreen(float finalTime)
    {
        // Stop the stopwatch
        StopStopwatch();

        // Show the end screen
        endScreen.SetActive(true);

        // Format and display the final time
        UpdateTimerDisplay(finalTime);

        // Save the time
        SaveTime(finalTime);

        // Disable the player GameObject
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Player object not found to disable.");
        }

        // Play the end screen music
        Debug.Log("Playing end screen music...");
        if (endScreenMusicClip != null)
        {
            Debug.Log($"End screen music clip: {endScreenMusicClip.name}");
        }
        else
        {
            Debug.LogWarning("End screen music clip is null.");
        }
        PlayBackgroundMusic(endScreenMusicClip);

        // Stop all enemies
        StopAllEnemies();

        PlayEndScreenMusic();

        Debug.Log("Final Time: " + finalTime);
    }


    public void PlayEndScreenMusic()
    {
        if (playerAudioSource != null && endScreenMusicClip != null)
        {
            playerAudioSource.clip = endScreenMusicClip;
            playerAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource or End Screen Music Clip is null.");
        }
    }





    // Save the final time (you can use PlayerPrefs for simplicity)
    public void SaveTime(float time)
    {
        PlayerPrefs.SetFloat("LastTime", time);
        PlayerPrefs.Save(); // Make sure it's saved
    }


    // Start the game and set up the relevant game states
    public void StartGame()
    {
        // Hide play screen
        playScreen.SetActive(false);

        // Enable player input and script
        Player.Instance.EnablePlayerInput();
        EnablePlayerScript();

        // Allow enemies to move
        ResumeAllEnemies();

        // Reset Time.timeScale
        Time.timeScale = 1f;

        // Start the stopwatch
        StartStopwatch();

        // Play the start game music
        PlayBackgroundMusic(startGameMusicClip);

        // Enable the light2D GameObject
        if (light2D != null)
        {
            light2D.SetActive(true);  // Enable the light2D GameObject
        }
        else
        {
            Debug.LogWarning("light2D GameObject is not assigned.");
        }
    }

    // Disable the player script when game is paused
    public void DisablePlayerScript()
    {
        if (Player.Instance != null)
        {
            Player.Instance.enabled = false; // Disable the Player script
        }
    }

    // Enable the player script after starting the game
    public void EnablePlayerScript()
    {
        if (Player.Instance != null)
        {
            Player.Instance.enabled = true; // Enable the Player script
        }
    }

    // Stop all enemies in the scene
    public void StopAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy1");
        foreach (GameObject enemy in enemies)
        {
            // Disable the specific child GameObject of the enemy
            Transform playerFindChild = enemy.transform.Find("PlayerFindChild");  // Replace "PlayerFindChild" with the actual name of the child object
            if (playerFindChild != null)
            {
                playerFindChild.gameObject.SetActive(false);  // Disable the specific child GameObject
            }

            // Optionally, you can disable other components on the enemy here if needed
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.enabled = false;  // Optionally disable the EnemyAI script
            }
        }
    }

    // Resume all enemies
    public void ResumeAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy1");
        foreach (GameObject enemy in enemies)
        {
            // Enable a specific GameObject inside the enemy
            Transform childObject = enemy.transform.Find("Hitbox");  // Replace with the name of your desired GameObject inside the enemy
            if (childObject != null)
            {
                childObject.gameObject.SetActive(true);  // Enable the specific GameObject inside the enemy
            }
            else
            {
                Debug.LogWarning("YourChildObjectName not found inside enemy.");
            }
        }
    }

    // Handle Game Over state and UI
    public void GameOver()
    {
        // Show the Game Over UI
        gameOverScreen.SetActive(true);

        // Stop the player
        DisablePlayerScript();

        // Stop all enemies
        StopAllEnemies();

        // Disable the HealthBar GameObject
        if (HealthBar != null)
        {
            HealthBar.SetActive(false);  // Disable the HealthBar GameObject
        }

        // Optional: Play a death sound
        if (Player.Instance.audioSource != null)
        {
            Player.Instance.audioSource.Play();
        }

        // Play Game Over music
        PlayBackgroundMusic(gameOverMusicClip);

        // Stop the stopwatch
        StopStopwatch();  // Call the StopStopwatch method directly here
    }

    // Retry the game (reload the current scene)
    public void RetryGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Reset Time.timeScale
        Time.timeScale = 1f;

        // Play play music again after retry
        PlayBackgroundMusic(playMusicClip);
    }

    // Go back to the main menu
    public void GoToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");

        // Reset Time.timeScale
        Time.timeScale = 1f;
    }

    // Play background music
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (playerAudioSource != null && clip != null)
        {
            // Check if the clip is different from the current one
            if (playerAudioSource.clip != clip)
            {
                playerAudioSource.clip = clip;
                playerAudioSource.Play();
            }
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip is null.");
        }
    }


    // Start the stopwatch
    public void StartStopwatch()
    {
        stopwatchIsRunning = true; // Start the timer
    }

    // Stop the stopwatch
    public void StopStopwatch()
    {
        stopwatchIsRunning = false; // Stop the timer
    }

    // Update the stopwatch display on the UI
    void Update()
    {
        if (stopwatchIsRunning)
        {
            // Increment the elapsed time by the time since the last frame
            elapsedTime += Time.deltaTime;

            // Update the timer display
            UpdateTimerDisplay(elapsedTime);
        }
    }

    // Update the timer display on the UI
    void UpdateTimerDisplay(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60); // Convert to minutes
        int seconds = Mathf.FloorToInt(timeToDisplay % 60); // Get the remainder in seconds
        int milliseconds = Mathf.FloorToInt((timeToDisplay * 1000) % 1000); // Milliseconds

        // Format and display the time as MM:SS:MS
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
    }

    public void ReduceTimeBy5Seconds()
    {
        elapsedTime -= 5f; // Subtract 5 seconds
        if (elapsedTime < 0f)
        {
            elapsedTime = 0f; // Ensure time doesn't go below zero
        }

        // Update the timer display
        UpdateTimerDisplay(elapsedTime);
        Debug.Log($"Time reduced by 5 seconds. New time: {elapsedTime}");
    }

}
