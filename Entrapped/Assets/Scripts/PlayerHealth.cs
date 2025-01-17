using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;       // Maximum health of the player
    public float health = 100f;          // Current health of the player
    public Image healthBar;              // Reference to the health bar UI

    public Animator playerAnimator;      // Reference to Animator for animations
    public GameObject gameOverScreen;    // Game Over UI screen
    public UIManager uiManager;          // Reference to the UIManager script (handles UI button actions)

    public Player playerScript;          // Reference to the Player script
    public Timer timerScript;            // Reference to the Timer script
    public float destroyDelay = 2f;      // Delay before destroying the player object (to allow animations to play)
    public AudioClip deathSound;         // Death sound effect

    private bool isDamagedRecently = false;  // Add this to track damage cooldown

    private void Start()
    {
        // Initialize health to maximum
        health = maxHealth;
        Debug.Log($"PlayerHealth: Health initialized to {health}");

        // Ensure the Player script is assigned if not already linked
        if (playerScript == null)
        {
            playerScript = GetComponent<Player>();
        }

        // Ensure the UIManager is assigned (you can optionally link this in the Inspector)
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>(); // Find the UIManager in the scene if not linked
        }
    }

    public void Update()
    {
        // Update health bar UI
        if (healthBar != null)
        {
            healthBar.fillAmount = Mathf.Clamp(health / maxHealth, 0f, 1f);
        }
    }

    // Detect collision with enemy (using OnTriggerEnter for trigger-based collisions)
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collider Entered: " + other.name);
        if (other.CompareTag("Enemy1"))
        {
            Debug.Log("Player hit by Enemy1");
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("isDamaged", true);
            }
        }
    }




    // Optionally use this method in the Hurt animation event or a time delay
    public void ResetIsDamaged()
    {
        playerAnimator.SetBool("isDamaged", false);
    }

    public void ResetCooldown()
    {
        isDamagedRecently = false;  // Allow damage again after the cooldown
    }

    // Take damage method
    public void TakeDamage(float damage)
    {
        Debug.Log("Taking damage: " + damage);
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (health <= 0)
        {
            Die();
        }
    }

    // Method to heal the player
    public void Heal(float healAmount)
    {
        health += healAmount;  // Increase the health
        health = Mathf.Clamp(health, 0, maxHealth);  // Ensure health doesn't exceed maxHealth or go below 0

        // Optional: You can update the health bar or trigger animations if needed
        if (healthBar != null)
        {
            healthBar.fillAmount = Mathf.Clamp(health / maxHealth, 0f, 1f);
        }
        Debug.Log($"Player healed by {healAmount}. Current health: {health}");
    }


    public void Die()
    {
        Debug.Log("Player is dead!");

        // Trigger death animation
        playerAnimator?.SetTrigger("isDead");

        // Show Game Over screen
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        UIManager.Instance.GameOver();
    }





}
