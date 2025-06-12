using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovementAdvanced : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Falling")]
    public float fallMultiplier = 2.5f;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Ledge Grab")]
    public bool ledgeGrabEnabled = true; // NEW: Toggle for ledge grabbing
    public float ledgeDetectionRadius = 1f;
    public float ledgeGrabSmoothing = 15f;
    public float playerHandsOffset = 1.8f;
    private bool isGrabbingLedge = false;
    private bool canGrabLedge = true;
    private Vector3 ledgePosition;
    private Vector3 ledgeNormal;
    private Vector3 grabTargetPosition;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode toggleLedgeGrabKey = KeyCode.L; // NEW: Key to toggle ledge grab

    [Header("Ground Check (Sphere)")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 50f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    Rigidbody rb;

    [Header("Animation")]
    public Animator animator;
    private bool isWalking;
    private bool isRunning;
    private bool isIdle;
    private bool isInAir;

    private Sound_Music audioManager;
    private float footstepTimer = 0f;
    private float footstepCooldown = 0.4f;

    public MovementState state;
    private MovementState previousState;

    public enum MovementState
    {
        idling,
        walking,
        sprinting,
        crouching,
        air,
        ledgeGrab
    }

    private void Awake()
    {
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManager = audioObject.GetComponent<Sound_Music>();
        }
        else
        {
            Debug.LogError("No GameObject with 'Audio' tag found in the scene!");
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;

        readyToJump = true;
        startYScale = transform.localScale.y;

        if (animator == null)
        {
            Debug.LogWarning("No Animator assigned on the player.");
        }

        previousState = state;
    }

    private void Update()
    {
        // NEW: Handle ledge grab toggle input
        if (Input.GetKeyDown(toggleLedgeGrabKey))
        {
            ToggleLedgeGrab();
        }

        // Only check for ledge grabbing if it's enabled
        if (!isGrabbingLedge && ledgeGrabEnabled)
        {
            LedgeGrab();
        }
        else if (isGrabbingLedge)
        {
            HandleLedgeGrabInput();
        }

        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

        if (!isGrabbingLedge)
        {
            MyInput();
            SpeedControl();
        }

        StateHandler();
        HandleFootsteps();

        rb.linearDamping = grounded ? groundDrag : 0f;
    }

    private void FixedUpdate()
    {
        if (!isGrabbingLedge)
        {
            MovePlayer();
            ApplyCustomGravity();
        }
        else
        {
            MaintainLedgePosition();
        }
    }

    // NEW: Method to toggle ledge grabbing
    private void ToggleLedgeGrab()
    {
        ledgeGrabEnabled = !ledgeGrabEnabled;
        
        // If we're currently grabbing a ledge and ledge grab is disabled, release the ledge
        if (!ledgeGrabEnabled && isGrabbingLedge)
        {
            ReleaseLedge();
        }
        
        Debug.Log("Ledge grabbing " + (ledgeGrabEnabled ? "enabled" : "disabled"));
    }

    // NEW: Method to release ledge (useful when disabling ledge grab while grabbing)
    private void ReleaseLedge()
    {
        if (isGrabbingLedge)
        {
            isGrabbingLedge = false;
            rb.useGravity = true;
            canGrabLedge = false;
            
            // Add a small delay before allowing ledge grab again
            Invoke(nameof(EnableLedgeGrab), 0.5f);
        }
    }

    private void ApplyCustomGravity()
    {
        if (!grounded && !OnSlope())
        {
            rb.AddForce(Physics.gravity * fallMultiplier, ForceMode.Acceleration);
        }
        else if (grounded)
        {
            rb.AddForce(Physics.gravity, ForceMode.Acceleration);
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        if (isGrabbingLedge)
        {
            state = MovementState.ledgeGrab;
        }
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (!grounded)
        {
            state = MovementState.air;
        }
        else
        {
            if (Input.GetKey(sprintKey))
            {
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }
            else
            {
                bool anyHorizontal = !Mathf.Approximately(horizontalInput, 0f);
                bool anyVertical = !Mathf.Approximately(verticalInput, 0f);

                if (anyHorizontal || anyVertical)
                {
                    state = MovementState.walking;
                    moveSpeed = walkSpeed;
                }
                else
                {
                    state = MovementState.idling;
                    moveSpeed = 0f;
                }
            }
        }

        isIdle = (state == MovementState.idling);
        isWalking = (state == MovementState.walking);
        isRunning = (state == MovementState.sprinting);
        isInAir = (state == MovementState.air);

        if (animator != null)
        {
            animator.SetBool("IsIdle", isIdle);
            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsInAir", isInAir);
            animator.SetBool("IsGrabbingLedge", isGrabbingLedge);
        }

        if (state != previousState)
        {
            switch (state)
            {
                case MovementState.idling:
                    break;

                case MovementState.walking:
                    // handled in HandleFootsteps
                    break;

                case MovementState.sprinting:
                    // handled in HandleFootsteps
                    break;

                case MovementState.air:
                    break;

                case MovementState.crouching:
                    if (audioManager != null)
                        audioManager.PlaySFX(audioManager.Crouch);
                    break;

                case MovementState.ledgeGrab:
                    // Optional ledge grab sound
                    break;
            }

            previousState = state;
        }
    }

    private void HandleFootsteps()
    {
        if (state == MovementState.walking || state == MovementState.sprinting)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                int rand = Random.Range(1, 6);

                if (state == MovementState.walking)
                {
                    footstepCooldown = 0.5f; // slower footsteps
                    if (audioManager != null)
                    {
                        switch (rand)
                        {
                            case 1: audioManager.PlaySFX(audioManager.WalkStepGrass1); break;
                            case 2: audioManager.PlaySFX(audioManager.WalkStepGrass2); break;
                            case 3: audioManager.PlaySFX(audioManager.WalkStepGrass3); break;
                            case 4: audioManager.PlaySFX(audioManager.WalkStepGrass4); break;
                            case 5: audioManager.PlaySFX(audioManager.WalkStepGrass5); break;
                        }
                    }
                }
                else if (state == MovementState.sprinting)
                {
                    footstepCooldown = 0.25f; // faster footsteps
                    if (audioManager != null)
                    {
                        switch (rand)
                        {
                            case 1: audioManager.PlaySFX(audioManager.RunStepGrass1); break;
                            case 2: audioManager.PlaySFX(audioManager.RunStepGrass2); break;
                            case 3: audioManager.PlaySFX(audioManager.RunStepGrass3); break;
                            case 4: audioManager.PlaySFX(audioManager.RunStepGrass4); break;
                            case 5: audioManager.PlaySFX(audioManager.RunStepGrass5); break;
                        }
                    }
                }

                footstepTimer = footstepCooldown;
            }
        }
        else
        {
            footstepTimer = 0f; // Reset timer when not walking/sprinting
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
            float uphillBoost = Mathf.Lerp(1f, 1.2f, 1f - slopeHit.normal.y);
            rb.AddForce(slopeMoveDirection * moveSpeed * 10f * uphillBoost, ForceMode.Force);

            if (rb.linearVelocity.y > 0.1f)
                rb.AddForce(Vector3.down * 5f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        if (animator != null)
        {
            animator.SetTrigger("IsJumping");
        }

        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        if (audioManager != null)
        {
            int rand = Random.Range(1, 21);

            switch (rand)
            {
                case 1: audioManager.PlaySFX(audioManager.CriSaut1); break;
                case 2: audioManager.PlaySFX(audioManager.CriSaut2); break;
            }
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out slopeHit, groundDistance + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private void LedgeGrab()
    {
        if (!grounded && canGrabLedge && rb.linearVelocity.y < 5f)
        {
            Vector3 forwardCheckOrigin = transform.position + Vector3.up * 1.5f;
            Vector3 forwardDir = orientation.forward;
            RaycastHit hit;

            if (Physics.Raycast(forwardCheckOrigin, forwardDir, out hit, ledgeDetectionRadius, whatIsGround) ||
                Physics.SphereCast(forwardCheckOrigin, 0.3f, forwardDir, out hit, ledgeDetectionRadius, whatIsGround))
            {
                TryGrabAtPoint(hit.point, hit.normal);
                return;
            }

            for (int i = -2; i <= 2; i++)
            {
                Vector3 checkDir = Quaternion.Euler(0, i * 15f, 0) * forwardDir;
                for (float h = 1.0f; h <= 2.0f; h += 0.3f)
                {
                    Vector3 origin = transform.position + Vector3.up * h;
                    if (Physics.Raycast(origin, checkDir, out hit, ledgeDetectionRadius, whatIsGround))
                    {
                        TryGrabAtPoint(hit.point, hit.normal);
                        return;
                    }
                }
            }
        }
    }

    private void TryGrabAtPoint(Vector3 hitPoint, Vector3 hitNormal)
    {
        float heightDifference = hitPoint.y - transform.position.y;
        if (heightDifference > 0.3f && heightDifference < 3f)
        {
            GrabLedge(hitPoint, hitNormal);
        }
    }

    private void GrabLedge(Vector3 ledgePos, Vector3 wallNormal)
    {
        isGrabbingLedge = true;
        ledgePosition = ledgePos;
        ledgeNormal = wallNormal;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;

        grabTargetPosition = ledgePosition - wallNormal * 0.3f - Vector3.up * playerHandsOffset;
        transform.position = grabTargetPosition;
        rb.position = grabTargetPosition;

        Vector3 lookDirection = -wallNormal;
        lookDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    private void MaintainLedgePosition()
    {
        rb.MovePosition(grabTargetPosition);

        Vector3 lookDirection = -ledgeNormal;
        lookDirection.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.fixedDeltaTime * 10f);

        rb.linearVelocity = Vector3.zero;
    }

    private void HandleLedgeGrabInput()
    {
        if (Input.GetKeyDown(jumpKey))
        {
            ClimbUpLedge();
        }
    }

    private void ClimbUpLedge()
    {
        isGrabbingLedge = false;
        canGrabLedge = false;
        rb.useGravity = true;

        Vector3 climbEndPosition = ledgePosition + Vector3.up * 0.2f - ledgeNormal * 0.8f;
        transform.position = climbEndPosition;
        rb.linearVelocity = (Vector3.up * 4f) + (-ledgeNormal * 2f);

        Invoke(nameof(EnableLedgeGrab), 0.8f);

        if (animator != null)
        {
            animator.SetTrigger("ClimbLedge");
        }
    }

    private void EnableLedgeGrab()
    {
        canGrabLedge = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);

        // Only show ledge grab gizmos if ledge grabbing is enabled
        if (!isGrabbingLedge && orientation != null && ledgeGrabEnabled)
        {
            Vector3 forwardCheckOrigin = transform.position + Vector3.up * 1.5f;
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(forwardCheckOrigin, ledgeDetectionRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(forwardCheckOrigin, orientation.forward * ledgeDetectionRadius);
        }
        else if (isGrabbingLedge)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ledgePosition, 0.2f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(grabTargetPosition, 0.15f);
        }
    }
}