using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement_builtin : MonoBehaviour
{
    public float maxSpeed = 1.0f;
    public float jumpForce = 8f;
    float rotationFactorperFrame = 7f;
    float runMultiplier = 3f;
    float movementMultiplier = 1.5f;
    float groundedGravity = -0.5f;
    float gravity = -9.81f;

    InputSystem_Actions playerInput;
    CharacterController characterController;
    Animator animator;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 currentRotation;

    bool isMovementPressed;
    bool isRunPressed;
    bool hasDoubleJump = true;
    bool wasJumping = false;
    bool isJumpPressed;
    int isWalkingHash;
    int isRunningHash;
    float initialJumpVelocity;
    float maxJumpHeight= 1.0f;
    float maxJumpTime= 0.5f;
    bool isJumping= false;


    void Awake()
    {
        playerInput = new InputSystem_Actions();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        playerInput.Player.Move.started += onMovementInput;
        playerInput.Player.Move.performed += onMovementInput;
        playerInput.Player.Move.canceled += onMovementInput;

        playerInput.Player.Run.started += onRun;
        playerInput.Player.Run.canceled += onRun;

        playerInput.Player.Jump.performed += onJump;
        playerInput.Player.Jump.canceled += onJump;

        setupJump();
    }
void HandleJump()
{
    if(!isJumping && characterController.isGrounded && isJumpPressed){
        isJumping=true;
        currentMovement.y = initialJumpVelocity;
        currentRunMovement.y = initialJumpVelocity;
    }
}
void setupJump(){
    float timeToApex= timeToApex = maxJumpTime/2;
    gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
    initialJumpVelocity=(2 * maxJumpHeight) / timeToApex;
}
    
void onJump(InputAction.CallbackContext context)
{
    isJumpPressed= context.ReadValueAsButton();

}

    

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x * movementMultiplier;
        currentMovement.z = currentMovementInput.y * movementMultiplier;

        currentRunMovement.x = currentMovementInput.x * movementMultiplier * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * movementMultiplier * runMultiplier;

        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }
void handleGravity()
{
    if (characterController.isGrounded)
    {
        // Réinitialise le flag de saut
        isJumping = false;
        hasDoubleJump = true;

        currentMovement.y = groundedGravity;
        currentRunMovement.y = groundedGravity;
    }
    else
    {
        currentMovement.y += gravity * Time.deltaTime;
        currentRunMovement.y += gravity * Time.deltaTime;
    }
}


    void handleRotation()
    {
        Vector3 direction;
        direction.x = currentMovementInput.x;
        direction.z = currentMovementInput.y;
        direction.y = 0f;

        Quaternion currentRot = transform.rotation;
        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(currentRot, targetRotation, rotationFactorperFrame * Time.deltaTime);
        }
    }

    void handleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }
    }

    void OnEnable()
    {
        playerInput.Player.Enable();
    }

    void OnDisable()
    {
        playerInput.Player.Disable();
    }

    void ResetDoubleJumpIfGrounded()
    {
        if (characterController.isGrounded)
        {
            hasDoubleJump = true;
        }
    }

void Update()
{
    handleRotation();
    handleAnimation();
    ResetDoubleJumpIfGrounded();

    HandleJump();           // ← make sure to invoke it each frame

    if (isRunPressed)
    {
        characterController.Move(currentRunMovement * Time.deltaTime);
        animator.SetBool(isRunningHash, true);
    }
    else
    {
        characterController.Move(currentMovement * Time.deltaTime);
        animator.SetBool(isRunningHash, false);
    }

    handleGravity();
}
}
