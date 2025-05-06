using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 60f;                      // Increased for faster movement
    public float airControlMultiplier = 0.5f;
    public float maxSpeed = 12f;

    [Header("Jumping")]
    public float jumpForce = 10f;
    public int maxJumps = 2;
    private int jumpCount;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;

    [Header("Wall Jump")]
    public float wallJumpForce = 12f;
    public Vector3 wallJumpDirection = new Vector3(1f, 1f, 0f);
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.6f;

    [Header("Ground & Wall Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    private bool isGrounded;
    private bool isTouchingWall;
    private Rigidbody rb;
    private float inputX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
            jumpCount = maxJumps;

        // Wall check (left/right raycast)
        isTouchingWall = Physics.Raycast(transform.position, Vector3.right * Mathf.Sign(inputX), wallCheckDistance, wallLayer);

        // Jump
        if (Input.GetButtonDown("Jump"))
        {
            if (isTouchingWall && !isGrounded)
                WallJump();
            else if (jumpCount > 0)
                Jump();
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.RightShift) && canDash)
            StartCoroutine(Dash());
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        float forceMultiplier = isGrounded ? 1f : airControlMultiplier;

        // Apply horizontal velocity directly
        Vector3 velocityChange = new Vector3(inputX * moveForce * forceMultiplier * Time.fixedDeltaTime, 0f, 0f);
        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Clamp horizontal speed
        Vector3 clampedVel = rb.velocity;
        clampedVel.x = Mathf.Clamp(clampedVel.x, -maxSpeed, maxSpeed);
        rb.velocity = new Vector3(clampedVel.x, rb.velocity.y, rb.velocity.z);

        // Apply custom drag when idle on ground
        if (Mathf.Abs(inputX) < 0.01f && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x * 0.9f, rb.velocity.y, rb.velocity.z);
        }
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, 0f); // Reset vertical speed
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpCount--;
    }

    void WallJump()
    {
        Vector3 dir = new Vector3(-Mathf.Sign(inputX) * wallJumpDirection.x, wallJumpDirection.y, 0f).normalized;
        rb.velocity = Vector3.zero;
        rb.AddForce(dir * wallJumpForce, ForceMode.Impulse);
    }

    System.Collections.IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        rb.velocity = Vector3.zero;

        Vector3 dashDir = new Vector3(Mathf.Sign(inputX) != 0 ? Mathf.Sign(inputX) : transform.localScale.x, 0f, 0f);
        rb.AddForce(dashDir * dashForce, ForceMode.VelocityChange);

        yield return new WaitForSeconds(0.15f);
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * Mathf.Sign(inputX) * wallCheckDistance);
    }
}
