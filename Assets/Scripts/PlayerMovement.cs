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
    public bool ledgeGrabEnabled = true;
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
    public KeyCode toggleLedgeGrabKey = KeyCode.L;

    [Header("Ground Check (Sphere)")]
    public Transform groundCheck;
    public float groundDistance = 0.5f;
    public LayerMask whatIsGround;
    bool grounded;
    bool wasGrounded; // NEW: Track previous ground state

    [Header("Slope Handling")]
    public float maxSlopeAngle = 50f;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    private bool wasOnSlope; // NEW: Track previous slope state
    private float slopeExitTimer = 0f; // NEW: Timer for slope exit

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

    [Header("Movement Lock")]
    private bool isMovementLocked = false;
    public bool IsMovementLocked => isMovementLocked;
    
    public enum MovementState
    {
        idling,
        walking,
        sprinting,
        crouching,
        air,
        ledgeGrab
    }
    
    private void OnEnable()
    {
        GameEvents.OnPlayerMovementLockChanged += HandleMovementLockChanged;
        GameEvents.OnPlayerControlsLockChanged += HandleMovementLockChanged;
    }
    
    private void OnDisable()
    {
        GameEvents.OnPlayerMovementLockChanged -= HandleMovementLockChanged;
        GameEvents.OnPlayerControlsLockChanged -= HandleMovementLockChanged;
    }
    
    private void HandleMovementLockChanged(bool isLocked)
    {
        LockMovement(isLocked);
    }

    private void Awake()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule) capsule.enabled = false;
        
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

        StartCoroutine(EnableColliderNextFrame());

        readyToJump = true;
        startYScale = transform.localScale.y;

        if (animator == null)
        {
            Debug.LogWarning("No Animator assigned on the player.");
        }

        previousState = state;
        wasGrounded = true;
    }

    private IEnumerator EnableColliderNextFrame()
    {
        yield return new WaitForFixedUpdate();
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule) capsule.enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleLedgeGrabKey))
        {
            ToggleLedgeGrab();
        }

        if (isMovementLocked)
        {
            grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);
            return;
        }

        // Store previous ground state
        wasGrounded = grounded;
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

        // Handle slope exit timer
        if (slopeExitTimer > 0)
        {
            slopeExitTimer -= Time.deltaTime;
            if (slopeExitTimer <= 0)
            {
                exitingSlope = false;
            }
        }

        // Check if we just left the ground
        if (wasGrounded && !grounded)
        {
            // If we were on a slope, handle the exit properly
            if (wasOnSlope)
            {
                exitingSlope = true;
                slopeExitTimer = 0.3f; // Give some time to clear slope physics
                
                // Clamp upward velocity to prevent floating
                if (rb.linearVelocity.y > 0)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 
                                                    Mathf.Min(rb.linearVelocity.y, 2f), 
                                                    rb.linearVelocity.z);
                }
            }
        }

        if (!isGrabbingLedge && ledgeGrabEnabled)
        {
            LedgeGrab();
        }
        else if (isGrabbingLedge)
        {
            HandleLedgeGrabInput();
        }

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
        if (isMovementLocked)
        {
            ApplyCustomGravity();
            return;
        }
        
        if (!isGrabbingLedge)
        {
            MovePlayer();
            ApplyCustomGravity();
        }
        else
        {
            MaintainLedgePosition();
        }

        // Update slope tracking
        wasOnSlope = OnSlope();
    }

    private void ToggleLedgeGrab()
    {
        ledgeGrabEnabled = !ledgeGrabEnabled;

        if (!ledgeGrabEnabled && isGrabbingLedge)
        {
            ReleaseLedge();
        }

        Debug.Log("Ledge grabbing " + (ledgeGrabEnabled ? "enabled" : "disabled"));
    }

    private void ReleaseLedge()
    {
        if (isGrabbingLedge)
        {
            isGrabbingLedge = false;
            rb.useGravity = true;
            canGrabLedge = false;

            Invoke(nameof(EnableLedgeGrab), 0.5f);
        }
    }

    private void ApplyCustomGravity()
    {
        if (!grounded && !OnSlope())
        {
            // Apply stronger gravity when falling
            Vector3 downForce = Physics.gravity * fallMultiplier;
            rb.AddForce(downForce, ForceMode.Acceleration);
        }
        else if (grounded)
        {
            // Light downward force to keep grounded
            rb.AddForce(Physics.gravity, ForceMode.Acceleration);
        }
    }

    private void MyInput()
    {
        if (isMovementLocked) return;
        
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
        }

        if (state != previousState)
        {
            switch (state)
            {
                case MovementState.idling:
                    break;
                case MovementState.walking:
                    break;
                case MovementState.sprinting:
                    break;
                case MovementState.air:
                    break;
                case MovementState.crouching:
                    if (audioManager != null)
                        audioManager.PlaySFX(audioManager.Crouch);
                    break;
                case MovementState.ledgeGrab:
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
                    footstepCooldown = 0.5f;
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
                    footstepCooldown = 0.25f;
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
            footstepTimer = 0f;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Check if we're on a slope and not trying to exit
        if (OnSlope() && !exitingSlope)
        {
            // Get slope-aligned movement direction
            Vector3 slopeMoveDirection = GetSlopeMoveDirection();
            
            // Apply movement force
            rb.AddForce(slopeMoveDirection * moveSpeed * 10f, ForceMode.Force);

            // Only apply downward force if moving up the slope
            if (rb.linearVelocity.y > 0.1f)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Force);
            }
        }
        else if (grounded)
        {
            // Normal ground movement
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            // Air movement
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void SpeedControl()
    {
        // Special handling when leaving a slope
        if (exitingSlope && !grounded)
        {
            // Limit velocity more aggressively when exiting slope
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed * 0.8f)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed * 0.8f;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
            return;
        }

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
        slopeExitTimer = 0.5f; // Set timer for slope exit
        
        // Clear velocity before jumping
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.5f, 0f, rb.linearVelocity.z * 0.5f);
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
        // Don't reset exitingSlope here - let the timer handle it
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

    public void LockMovement(bool lockState)
    {
        isMovementLocked = lockState;
        
        if (lockState)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.angularVelocity = Vector3.zero;
            
            state = MovementState.idling;
            moveSpeed = 0f;
            
            if (animator != null)
            {
                animator.SetBool("IsIdle", true);
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
            }
        }
    }
}