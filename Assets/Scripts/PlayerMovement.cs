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

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check (Sphere)")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
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

    public MovementState state;
    public enum MovementState
    {
        idling,
        walking,
        sprinting,
        crouching,
        air
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
            Debug.LogWarning("No Animator assigned on the player. Please drag your Animator into the Inspector, and add Bool parameters 'IsIdle', 'IsWalking', 'IsRunning', 'IsInAir', and Trigger 'IsJumping'.");
        }
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 2f, Color.red);

        grounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            whatIsGround
        );

        Debug.DrawLine(
            groundCheck.position,
            groundCheck.position + Vector3.down * groundDistance,
            grounded ? Color.green : Color.red
        );

        MyInput();
        SpeedControl();
        StateHandler();

        rb.linearDamping = grounded ? groundDrag : 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        ApplyCustomGravity();
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
        verticalInput   = Input.GetAxisRaw("Vertical");

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
        if (Input.GetKey(crouchKey))
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
                bool anyVertical   = !Mathf.Approximately(verticalInput, 0f);

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

        isIdle    = (state == MovementState.idling);
        isWalking = (state == MovementState.walking);
        isRunning = (state == MovementState.sprinting);
        isInAir   = (state == MovementState.air);

        if (animator != null)
        {
            animator.SetBool("IsIdle",    isIdle);
            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsInAir",   isInAir);
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
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
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(
            groundCheck.position,
            Vector3.down,
            out slopeHit,
            groundDistance + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
