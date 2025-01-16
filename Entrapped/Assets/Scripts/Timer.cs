using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timeText; // Reference to the TextMeshProUGUI element
    private float elapsedTime = 0f;  // Timer variable
    private bool isRunning = false;  // To track if the timer is running

    // Update the timer every frame
    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;  // Increment elapsed time
            UpdateTimeDisplay(elapsedTime); // Update the UI display
        }
    }

    // Start the timer
    public void StartTimer()
    {
        isRunning = true;
    }

    // Stop the timer
    public void StopTimer()
    {
        isRunning = false;
    }

    // Reset the timer
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimeDisplay(elapsedTime);
    }

    // Update the timer display in the format MM:SS:SSS
    private void UpdateTimeDisplay(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);  // Get the minutes
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);  // Get the seconds
        int milliseconds = Mathf.FloorToInt((timeToDisplay * 1000) % 1000);  // Get the milliseconds

        timeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);  // Update the UI text
    }

    // Get the current elapsed time (to pass to UIManager or for saving)
    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}
