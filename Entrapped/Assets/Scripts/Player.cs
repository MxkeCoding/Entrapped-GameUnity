using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance;

    // Player components
    public Rigidbody2D rb;

    // Movement settings
    public float moveSpeed = 5f;
    public InputActionAsset playerActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction attackAction;

    public PlayerInput playerInput;

    // Jump settings
    public float jumpForce = 15f;
    public float holdJumpForce = 5f;
    public float maxJumpDuration = 0.5f;

    // Ground check
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // Jumping state
    private bool isGrounded;
    private bool isJumping;
    private float jumpTimer;
    public int maxJumps = 2;
    private int jumpCount;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    // Dash settings
    public bool canDash = true;
    private bool isDashing;
    public float dashingPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

    // Movement variables
    private Vector2 movementInput;

    // Maximum velocity
    public float maxVelocity = 10f;

    // Wall climbing settings
    public Transform leftWallCheck;
    public Transform rightWallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    public float wallClimbSpeed = 5f;

    // Wall climbing state
    private bool isTouchingLeftWall;
    private bool isTouchingRightWall;
    private bool isWallClimbing;

    // Animator
    public Animator animator;

    // UI Screens
    public GameObject playScreen;
    



    // Map boundaries
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    // Audio
    public AudioSource audioSource;
    // Death sound
    public AudioClip deathSound; // Define this if you're playing a death sound


    // Attack settings
    public float attackRadius = 5f;
    public LayerMask enemyLayer;
    public float xPosition = 0f;
    public float yPosition = 0f;
    public GameObject explosionEffectPrefab;
    public bool canAttack = false;

    // Health
    public int health = 100; // Add the health variable here
    public PlayerHealth playerHealthScript;

    private float startTime;
    private float finalTime;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Movement
        moveAction = playerActions.FindAction("Move");
        jumpAction = playerActions.FindAction("Jump");
        dashAction = playerActions.FindAction("Dash");
        attackAction = playerActions.FindAction("Attack");

        // Get the Animator component
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Initialize the AudioSource
        audioSource = GetComponent<AudioSource>();

        // Show play screen initially
        UIManager.Instance.ShowPlayScreen();

        startTime = Time.time; // Record the start time when the player starts the level


        if (playerHealthScript == null)
        {
            playerHealthScript = GetComponent<PlayerHealth>();  // Ensure it's linked
        }
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
        dashAction?.Enable();

        if (attackAction != null)
        {
            attackAction.performed += OnAttack;
            if (!canAttack)
            {
                attackAction.Disable(); // Disable attack action if the player cannot attack
            }
        }
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        dashAction?.Disable();
        attackAction?.Disable();

        if (attackAction != null)
        {
            attackAction.performed -= OnAttack;
        }
    }


    private void Update()
    {
        movementInput = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            jumpCount = 0; // Reset jump count when grounded
        }

        isTouchingLeftWall = Physics2D.Raycast(leftWallCheck.position, Vector2.left, wallCheckDistance, wallLayer);
        isTouchingRightWall = Physics2D.Raycast(rightWallCheck.position, Vector2.right, wallCheckDistance, wallLayer);

        HandleWallClimbInput();
        HandleJumpInput();
        if (!isDashing) HandleDashInput();

        FlipSprite(); // Call sprite flipping logic here
        UpdateAnimator();

        CheckOutOfBounds(); // Check if the player is out of bounds

        if (canAttack)
        {
            OnAttack(new InputAction.CallbackContext());
        }

    }




    private void CheckOutOfBounds()
    {
        Vector3 position = transform.position;

        // Check if the player's position is outside of the defined map boundaries
        if (position.x < minX || position.x > maxX || position.y < minY || position.y > maxY)
        {
            // Trigger the GameOver method in UIManager
            UIManager.Instance.GameOver();
        }
    }





    private void ResetPlayer()
    {
        rb.velocity = Vector2.zero;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!canAttack)
        {
            Debug.LogWarning("Attack is disabled! Collect a coinAttack to enable it.");
            return; // Exit if the player cannot attack
        }

        if (Time.timeScale == 0) return; // Prevent attack if the game is paused

        Debug.Log("Attack performed!");
        PerformExplosionEffect(); // Trigger the explosion particle effect
        PerformAreaAttack();      // Perform the area-of-effect attack

        // Disable attack after performing it
        canAttack = false;
        Debug.Log("Attack disabled. Collect a coinAttack to re-enable it.");
    }



    // Show the attack circle
    private void PerformExplosionEffect()
    {
        if (explosionEffectPrefab != null)
        {
            // Instantiate the particle effect at the player's position
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            // Optional: Destroy the particle effect after its duration
            ParticleSystem particleSystem = explosionEffect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                Destroy(explosionEffect, particleSystem.main.duration);
            }
            else
            {
                Debug.LogWarning("Explosion effect prefab does not have a ParticleSystem component!");
            }
        }
        else
        {
            Debug.LogWarning("Explosion effect prefab is not assigned in the inspector!");
        }
    }

    private void PerformAttack()
    {
        // Perform the attack (your attack logic goes here)
        Debug.Log("Player attacked!");

        // After the player uses the attack, disable the attack ability
        canAttack = false;
    }

    // Perform the area attack and deal damage to enemies within the attack radius
    private void PerformAreaAttack()
    {
        // Use the player's current position and attackRadius to detect enemies
        Vector2 attackPosition = new Vector2(transform.position.x + xPosition, transform.position.y + yPosition);

        // Detect enemies in the attack radius
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPosition, attackRadius, enemyLayer);

        // Debug: Check how many enemies are detected
        Debug.Log($"Enemies detected: {enemiesHit.Length}");

        foreach (Collider2D enemy in enemiesHit)
        {
            if (enemy != null)
            {
                Debug.Log($"Enemy {enemy.name} hit! Applying damage.");

                // Assuming the enemy has an EnemyHealth script attached
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(50f); // Apply 20 damage (or modify as needed)
                    Debug.Log($"Damage applied to {enemy.name}. Remaining health: {enemyHealth.maxHealth - 50f}");
                }
                else
                {
                    Debug.LogError($"Enemy {enemy.name} does not have an EnemyHealth component attached.");
                }
            }
        }
    }


    private void OnDrawGizmos()
    {
        // Set Gizmos color to red for boundaries
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(minX, minY, 0), new Vector3(maxX, minY, 0));
        Gizmos.DrawLine(new Vector3(maxX, minY, 0), new Vector3(maxX, maxY, 0));
        Gizmos.DrawLine(new Vector3(maxX, maxY, 0), new Vector3(minX, maxY, 0));
        Gizmos.DrawLine(new Vector3(minX, maxY, 0), new Vector3(minX, minY, 0));

        // Draw the attack radius
        Gizmos.color = Color.blue;
        Vector2 attackPosition = new Vector2(transform.position.x + xPosition, transform.position.y + yPosition);
        Gizmos.DrawWireSphere(attackPosition, attackRadius);
    }



    private void FixedUpdate()
    {
        if (!isDashing && !isWallClimbing)
        {
            rb.velocity = new Vector2(movementInput.x * moveSpeed, rb.velocity.y);
            ClampVelocity();
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !jumpAction.IsPressed())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }



        // Check and apply the boundaries to the player's position
        Vector3 clampedPosition = transform.position;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);

        transform.position = clampedPosition;
    }



    private void ClampVelocity()
    {
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
    }

    private void HandleJumpInput()
    {
        if (jumpAction.triggered && jumpCount < maxJumps)
        {
            StartJump();
            isJumping = true;
        }

        if (jumpAction.IsPressed() && isJumping)
        {
            HoldJump();
        }

        if (jumpAction.WasReleasedThisFrame())
        {
            isJumping = false;
        }
    }

    


    private void HandleWallClimbInput()
    {
        // Check if the player is touching a wall and allow climbing if the jump button is pressed
        if ((isTouchingLeftWall || isTouchingRightWall) && !isGrounded)
        {
            // If the jump action is triggered and the player is touching a wall, initiate wall climbing
            if (jumpAction.triggered)
            {
                isWallClimbing = true;
                rb.velocity = new Vector2(rb.velocity.x, wallClimbSpeed);  // Set the climbing speed
            }
            else if (!jumpAction.IsPressed()) // Stop climbing if jump button is released
            {
                isWallClimbing = false;
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);  // Maintain current vertical velocity
            }
        }
        else
        {
            isWallClimbing = false;  // Stop climbing when not touching a wall
        }
    }

    private void StartJump()
    {
        isJumping = true;
        jumpTimer = 0f;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount++;
        UpdateAnimator();
    }

    private void HoldJump()
    {
        if (jumpTimer < maxJumpDuration)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + holdJumpForce * Time.deltaTime);
            jumpTimer += Time.deltaTime;
        }
    }

    private void HandleDashInput()
    {
        if (dashAction.triggered && Mathf.Abs(movementInput.x) > 0.1f && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        Vector2 dashDirection = new Vector2(movementInput.x, 0).normalized;
        rb.velocity = dashDirection * dashingPower;

        yield return new WaitForSeconds(dashingTime);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);

        canDash = true;
    }

    private void FlipSprite()
    {
        if (movementInput.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (movementInput.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            // Update animator parameters
            animator.SetBool("isMoving", Mathf.Abs(movementInput.x) > 0.1f); // Set isMoving to true if the player is moving horizontally
            animator.SetBool("isGrounded", isGrounded); // Whether the player is on the ground
            animator.SetBool("isJumping", isJumping); // Whether the player is jumping
            animator.SetBool("isDashing", isDashing); // Whether the player is dashing
        }
    }


    public void PlayDeathSound()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);  // Play the death sound
        }
        else
        {
            Debug.LogWarning("AudioSource or Death Sound is missing!");
        }
    }

    public void DisablePlayerInput()
    {
        playerInput.enabled = false;
    }

    public void EnablePlayerInput()
    {
        playerInput.enabled = true;
    }


    public void GrantDashAbility()
    {
        // Re-enable dashing if the player picks up a Dash coin
        canDash = true;
        Debug.Log("Dash power-up granted!");
    }

    public void GrantExtraJump()
    {
        // Increase max jumps by one
        maxJumps++;
        Debug.Log("Extra jump power-up granted!");
    }

    public void GrantWallClimbAbility()
    {
        // Increase wall check distance for better wall climbing
        wallCheckDistance += 0.008f;
        Debug.Log("Wall climb power-up granted!");
    }

    private void GrantAttackAbility()
    {
        canAttack = true; // Enable the attack ability
        Debug.Log("Attack power-up granted!");

        // Automatically trigger OnAttack if canAttack is true
        if (canAttack)
        {
            OnAttack(new InputAction.CallbackContext());
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player collects a coin (existing logic remains unchanged)
        if (other.CompareTag("coinDash"))
        {
            GrantDashAbility(); // Grant Dash ability
            Destroy(other.gameObject); // Destroy the coin
        }
        else if (other.CompareTag("coinJump"))
        {
            GrantExtraJump(); // Grant Extra Jump ability
            Destroy(other.gameObject); // Destroy the coin
        }
        else if (other.CompareTag("coinWallClimb"))
        {
            GrantWallClimbAbility(); // Grant Wall Climb ability
            Destroy(other.gameObject); // Destroy the coin
        }
        else if (other.CompareTag("coinAttack"))
        {
            GrantAttackAbility(); // Enable the attack ability
            Destroy(other.gameObject); // Destroy the coin
        }
        else if (other.CompareTag("coinTime"))
        {
            UIManager.Instance.ReduceTimeBy5Seconds(); // Call method to reduce time
            Destroy(other.gameObject); // Destroy the coin
        }
        else if (other.CompareTag("coinHealth"))
        {
            Debug.Log("Player collected a health coin!");
            playerHealthScript.Heal(25f);  // Heal the player by 25 health (adjust as necessary)
            Destroy(other.gameObject);  // Destroy the coin object after collection
        }

        // Check if the player reaches the end (e.g., finish line or special zone)
        if (other.CompareTag("coinEnd"))
        {
            finalTime = Time.time - startTime; // Calculate the final time when the player reaches the end zone
            UIManager.Instance.ShowEndScreen(finalTime); // Pass the final time to the ShowEndScreen method
            Debug.Log("Player reached the end zone!");
        }
    }






}
