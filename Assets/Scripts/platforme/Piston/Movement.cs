/*  using UnityEngine;
[RequireComponent(typeof(Rigidbody),typeof(Animator))]
public class Movement : MonoBehaviour
{
    [Header("Mouvements")]
    // Mouvement
public float maxSpeed = 5f;
public float groundAccel = 20f;
public float airAccel = 8f;

// Saut
public float jumpForce = 8f;
public float jumpCutMultiplier = 2.5f;
public int maxJumps = 2;

// Dash
    public float dashForce = 10f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool hasDashed = false;

// Internes
    private Rigidbody rb;
    private Animator anim;
       // these track your inputs/states
    private bool isWalking = false;
    private bool isRunning = false;
    private bool isJumping = false;
    private bool isDashing = false;
    private bool isMovementPressed = false;


    private int jumpcount;
    void Start(){
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        jumpCount = maxJumps;
        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.LogError($"[{name}] No Animator component found!");
        canDash = true;
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        //isJumpingHash = Animator.StringToHash("isJumping");
        //isDashingHash = Animator.StringToHash("isDashing");
    }
     void Update()
     {
        HandleMovement();
        HandleJump();
        HandleDash();
        HandleSprint();

        // AFTER all state changes, push them into the Animator:
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isDashing", isDashing);
    }

    void HandleMovement()
    {
        

    }

    void handleAnimation()
    {
        bool isWalking = animator.getBool("isWalking");
        bool isRunning = animator.getBool("isRunning");
        //bool isJumping = animator.getBool("isJumping");
        //bool isDashing = animator.getBool("isDashing");
        // walking
        if (isMovementPressed && !isWalking)
        {
            animtor.setBool(isWalkingHash, true);
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.setBool(isWalkingHash, false);
        }

        // running
        if((isMovementPressed&& isRunPressed) && !isRunning)
        {
            animator.setBool(isRunningHash, true);
        }
        else if ((!isMovementPressed || isRunning)&&!isRunning)
        {
            animator.setBool(isRunningHash, false);
        }

    }


    void HandleJump()
    {
        if (IsGrounded())
            jumpcount = maxJumps;

    }
    void HandleDash()
    {
    }

    void ResetDash() => canDash = true;

    bool IsGrounded()
    {
        float dist = GetComponent<Collider>().bounds.extents.y + 0.1f;
        return Physics.Raycast(transform.position, Vector3.down, dist);
    }
} */
  