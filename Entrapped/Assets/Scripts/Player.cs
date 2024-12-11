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

    // Lives system
    public int lives = 3; // Total lives

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

    // Dash settings
    public bool canDash = true;
    private bool isDashing;
    public float dashingPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

    // Movement variables
    private Vector2 movementInput;

    // Maximum velocity
    public float maxVelocity = 10f; // Maximum speed in any direction

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        moveAction = playerActions.FindAction("Move");
        jumpAction = playerActions.FindAction("Jump");
        dashAction = playerActions.FindAction("Dash");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        dashAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        dashAction.Disable();
    }

    private void Update()
    {
        movementInput = moveAction.ReadValue<Vector2>();
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

        // Check for out-of-bounds
        if (transform.position.y < -10f) // Example for out-of-bounds check
        {
            LoseLife();
        }
    }

    private void LoseLife()
    {
        lives--;

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            RespawnPlayer();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
    }

    private void RespawnPlayer()
    {
        Debug.Log($"Lives left: {lives}");
        transform.position = new Vector3(0, 1, 0); // Example respawn position
        rb.velocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!isDashing && !isWallClimbing)
        {
            rb.velocity = new Vector2(movementInput.x * moveSpeed, rb.velocity.y);
            ClampVelocity();
        }

        if (isGrounded && rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
    }

    private void HandleJumpInput()
    {
        if (jumpAction.triggered && jumpCount < maxJumps)
        {
            StartJump();
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

    private void StartJump()
    {
        isJumping = true;
        jumpTimer = 0f;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount++;
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

        ClampVelocity();
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void HandleWallClimbInput()
    {
        if ((isTouchingLeftWall && movementInput.x < 0) || (isTouchingRightWall && movementInput.x > 0))
        {
            if (movementInput.y > 0)
            {
                isWallClimbing = true;
                rb.velocity = new Vector2(rb.velocity.x, wallClimbSpeed);
            }
            else
            {
                isWallClimbing = false;
            }
        }
        else
        {
            isWallClimbing = false;
        }
    }

    private void ClampVelocity()
    {
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("coinDash"))
        {
            canDash = true;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("coinJump"))
        {
            maxJumps++;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("coinWallClimb"))
        {
            wallCheckDistance += 0.008f;
            Destroy(other.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(leftWallCheck.position, Vector2.left * wallCheckDistance);
        Gizmos.DrawRay(rightWallCheck.position, Vector2.right * wallCheckDistance);
    }
}
