using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFind : MonoBehaviour
{
    public MonoBehaviour scriptToDisable; // Reference to the specific script you want to disable

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger has the "Player" tag
        if (other.CompareTag("Player"))
        {
            if (scriptToDisable != null)
            {
                // Disable the specified script
                scriptToDisable.enabled = true;
                Debug.Log($"{scriptToDisable.GetType().Name} has been disabled.");
            }
            else
            {
                Debug.LogWarning("No script assigned to disable.");
            }
        }
    }

}
