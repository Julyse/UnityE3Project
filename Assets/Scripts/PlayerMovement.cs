using UnityEngine;
using KinematicCharacterController;

public class PlayerMovementAdvanced : MonoBehaviour, ICharacterController
{
    [Header("References")]
    public KinematicCharacterMotor Motor;
    public Transform orientation;
    public Animator animator;

    [Header("Ground Movement")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 12f;
    public float crouchSpeed = 3.5f;
    public float StableMovementSharpness = 15f;
    public float OrientationSharpness = 10f;

    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 10f;
    public float AirAccelerationSpeed = 10f;
    public float Drag = 0.1f;

    [Header("Jumping")]
    public float JumpUpSpeed = 10f;
    public float JumpPostGroundingGraceTime = 0.15f;

    [Header("Gravity")]
    public Vector3 Gravity = new Vector3(0, -25f, 0);
    public float FallMultiplier = 2f;

    [Header("Crouching")]
    public float CrouchedCapsuleHeight = 1f;

    [Header("Ledge Grab")]
    public bool ledgeGrabEnabled = true;
    public float ledgeDetectionRadius = 1f;
    public float ledgeGrabSmoothing = 15f;
    public float playerHandsOffset = 1.8f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode toggleLedgeGrabKey = KeyCode.L;

    public MovementState state;
    public bool IsMovementLocked => _isMovementLocked;

    public enum MovementState { idling, walking, sprinting, crouching, air, ledgeGrab }

    // Input state (gathered in Update, consumed in KCC callbacks)
    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    private bool _isSprinting;
    private bool _jumpRequested;
    private bool _jumpConsumed;
    private bool _jumpedThisFrame;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump;

    // Ledge grab
    private bool _isGrabbingLedge;
    private bool _canGrabLedge = true;
    private Vector3 _ledgePosition;
    private Vector3 _ledgeNormal;
    private Vector3 _grabTargetPosition;
    private bool _applyClimbVelocity;
    private Vector3 _climbVelocity;

    // Crouch
    private bool _shouldBeCrouching;
    private bool _isCrouching;

    // Lock
    private bool _isMovementLocked;

    // Audio
    private Sound_Music _audioManager;
    private float _footstepTimer;
    private const float FootstepCooldownWalk = 0.5f;
    private const float FootstepCooldownRun = 0.25f;

    private MovementState _previousState;
    private Collider[] _probedColliders = new Collider[8];

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

    private void HandleMovementLockChanged(bool locked) => LockMovement(locked);

    private void Awake()
    {
        Motor.CharacterController = this;
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
            _audioManager = audioObj.GetComponent<Sound_Music>();
    }

    private void Start() { }

    private void Update()
    {
        if (Input.GetKeyDown(toggleLedgeGrabKey))
        {
            ledgeGrabEnabled = !ledgeGrabEnabled;
            if (!ledgeGrabEnabled && _isGrabbingLedge) ReleaseLedge();
        }

        if (_isMovementLocked) return;

        if (!_isGrabbingLedge && ledgeGrabEnabled)
            LedgeGrab();
        else if (_isGrabbingLedge)
            HandleLedgeGrabInput();

        GatherInput();
        UpdateAnimatorState();
        HandleFootsteps();
    }

    private void GatherInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _isSprinting = Input.GetKey(sprintKey);

        Vector3 raw = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);

        if (orientation != null)
        {
            Vector3 fwd = Vector3.ProjectOnPlane(orientation.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(orientation.right, Vector3.up).normalized;
            _moveInputVector = fwd * raw.z + right * raw.x;
        }
        else
        {
            _moveInputVector = raw;
        }

        if (_moveInputVector.sqrMagnitude > 0.01f)
            _lookInputVector = _moveInputVector.normalized;

        if (Input.GetKeyDown(jumpKey) && !_isGrabbingLedge)
        {
            _timeSinceJumpRequested = 0f;
            _jumpRequested = true;
        }

        if (Input.GetKeyDown(crouchKey) && !_isCrouching)
        {
            _isCrouching = true;
            _shouldBeCrouching = true;
            Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            _shouldBeCrouching = false;
        }
    }

    // ---- ICharacterController ----

    public void BeforeCharacterUpdate(float deltaTime)
    {
        if (_isGrabbingLedge)
            Motor.SetTransientPosition(_grabTargetPosition);
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_isGrabbingLedge)
        {
            Vector3 look = -_ledgeNormal;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f)
                currentRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(look, Vector3.up), 1f - Mathf.Exp(-ledgeGrabSmoothing * deltaTime));
        }
        // ThirdPersonCam rotates playerObject directly — don't rotate the KCC root
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_isMovementLocked || _isGrabbingLedge)
        {
            currentVelocity = Vector3.zero;
            return;
        }

        if (_applyClimbVelocity)
        {
            currentVelocity = _climbVelocity;
            _applyClimbVelocity = false;
            return;
        }

        float moveSpeed = _isCrouching ? crouchSpeed : (_isSprinting ? sprintSpeed : walkSpeed);

        if (Motor.GroundingStatus.IsStableOnGround)
        {
            float mag = currentVelocity.magnitude;
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * mag;

            Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
            Vector3 reoriented = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
            currentVelocity = Vector3.Lerp(currentVelocity, reoriented * moveSpeed, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
        }
        else
        {
            if (_moveInputVector.sqrMagnitude > 0f)
            {
                Vector3 added = _moveInputVector * AirAccelerationSpeed * deltaTime;
                Vector3 flatVel = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                if (flatVel.magnitude < MaxAirMoveSpeed)
                    added = Vector3.ClampMagnitude(flatVel + added, MaxAirMoveSpeed) - flatVel;
                else if (Vector3.Dot(flatVel, added) > 0f)
                    added = Vector3.ProjectOnPlane(added, flatVel.normalized);

                currentVelocity += added;
            }

            float gravScale = currentVelocity.y < 0f ? FallMultiplier : 1f;
            currentVelocity += Gravity * gravScale * deltaTime;
            currentVelocity *= 1f / (1f + Drag * deltaTime);
        }

        // Jump
        _jumpedThisFrame = false;
        _timeSinceJumpRequested += deltaTime;

        if (_jumpRequested && !_jumpConsumed)
        {
            bool canJump = Motor.GroundingStatus.IsStableOnGround || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime;
            if (canJump)
            {
                Motor.ForceUnground();
                currentVelocity += (Motor.CharacterUp * JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                _jumpRequested = false;
                _jumpConsumed = true;
                _jumpedThisFrame = true;
                FireJumpEffects();
            }
        }

        if (_jumpRequested && _timeSinceJumpRequested > 0.2f)
            _jumpRequested = false;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            if (!_jumpedThisFrame) _jumpConsumed = false;
            _timeSinceLastAbleToJump = 0f;
        }
        else
        {
            _timeSinceLastAbleToJump += deltaTime;
        }

        if (_isCrouching && !_shouldBeCrouching)
        {
            Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
            if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders, Motor.CollidableLayers, QueryTriggerInteraction.Ignore) > 0)
                Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
            else
                _isCrouching = false;
        }
    }

    public void PostGroundingUpdate(float deltaTime) { }
    public bool IsColliderValidForCollisions(Collider coll) => true;
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }

    // ---- Ledge grab ----

    private void LedgeGrab()
    {
        if (Motor.GroundingStatus.IsStableOnGround || !_canGrabLedge) return;

        Vector3 fwd = orientation != null ? orientation.forward : transform.forward;
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        RaycastHit hit;

        if (Physics.Raycast(origin, fwd, out hit, ledgeDetectionRadius) ||
            Physics.SphereCast(origin, 0.3f, fwd, out hit, ledgeDetectionRadius))
        {
            TryGrabAtPoint(hit.point, hit.normal);
            return;
        }

        for (int i = -2; i <= 2; i++)
        {
            Vector3 dir = Quaternion.Euler(0, i * 15f, 0) * fwd;
            for (float h = 1.0f; h <= 2.0f; h += 0.3f)
            {
                if (Physics.Raycast(transform.position + Vector3.up * h, dir, out hit, ledgeDetectionRadius))
                {
                    TryGrabAtPoint(hit.point, hit.normal);
                    return;
                }
            }
        }
    }

    private void TryGrabAtPoint(Vector3 hitPoint, Vector3 hitNormal)
    {
        float diff = hitPoint.y - transform.position.y;
        if (diff > 0.3f && diff < 3f)
        {
            _isGrabbingLedge = true;
            _ledgePosition = hitPoint;
            _ledgeNormal = hitNormal;
            _grabTargetPosition = hitPoint - hitNormal * 0.3f - Vector3.up * playerHandsOffset;
            Motor.ForceUnground();
        }
    }

    private void HandleLedgeGrabInput()
    {
        if (Input.GetKeyDown(jumpKey))
            ClimbUpLedge();
    }

    private void ClimbUpLedge()
    {
        _isGrabbingLedge = false;
        _canGrabLedge = false;
        Motor.SetTransientPosition(_ledgePosition + Vector3.up * 0.2f - _ledgeNormal * 0.8f);
        _climbVelocity = Vector3.up * 4f - _ledgeNormal * 2f;
        _applyClimbVelocity = true;
        Invoke(nameof(EnableLedgeGrab), 0.8f);
        animator?.SetTrigger("ClimbLedge");
    }

    private void ReleaseLedge()
    {
        _isGrabbingLedge = false;
        _canGrabLedge = false;
        Invoke(nameof(EnableLedgeGrab), 0.5f);
    }

    private void EnableLedgeGrab() => _canGrabLedge = true;

    // ---- Animation / Audio ----

    private void UpdateAnimatorState()
    {
        bool grounded = Motor.GroundingStatus.IsStableOnGround;
        MovementState newState;

        if (_isGrabbingLedge)
            newState = MovementState.ledgeGrab;
        else if (Input.GetKey(crouchKey))
            newState = MovementState.crouching;
        else if (!grounded)
            newState = MovementState.air;
        else if (_isSprinting && _moveInputVector.sqrMagnitude > 0.01f)
            newState = MovementState.sprinting;
        else if (_moveInputVector.sqrMagnitude > 0.01f)
            newState = MovementState.walking;
        else
            newState = MovementState.idling;

        state = newState;

        if (animator != null)
        {
            animator.SetBool("IsIdle", state == MovementState.idling);
            animator.SetBool("IsWalking", state == MovementState.walking);
            animator.SetBool("IsRunning", state == MovementState.sprinting);
            animator.SetBool("IsInAir", state == MovementState.air);
        }

        if (state != _previousState)
        {
            if (state == MovementState.crouching && _audioManager != null)
                _audioManager.PlaySFX(_audioManager.Crouch);
            _previousState = state;
        }
    }

    private void HandleFootsteps()
    {
        if (state != MovementState.walking && state != MovementState.sprinting)
        {
            _footstepTimer = 0f;
            return;
        }

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer > 0f) return;

        int rand = Random.Range(1, 6);
        if (state == MovementState.walking)
        {
            _footstepTimer = FootstepCooldownWalk;
            if (_audioManager != null)
                switch (rand)
                {
                    case 1: _audioManager.PlaySFX(_audioManager.WalkStepGrass1); break;
                    case 2: _audioManager.PlaySFX(_audioManager.WalkStepGrass2); break;
                    case 3: _audioManager.PlaySFX(_audioManager.WalkStepGrass3); break;
                    case 4: _audioManager.PlaySFX(_audioManager.WalkStepGrass4); break;
                    case 5: _audioManager.PlaySFX(_audioManager.WalkStepGrass5); break;
                }
        }
        else
        {
            _footstepTimer = FootstepCooldownRun;
            if (_audioManager != null)
                switch (rand)
                {
                    case 1: _audioManager.PlaySFX(_audioManager.RunStepGrass1); break;
                    case 2: _audioManager.PlaySFX(_audioManager.RunStepGrass2); break;
                    case 3: _audioManager.PlaySFX(_audioManager.RunStepGrass3); break;
                    case 4: _audioManager.PlaySFX(_audioManager.RunStepGrass4); break;
                    case 5: _audioManager.PlaySFX(_audioManager.RunStepGrass5); break;
                }
        }
    }

    private void FireJumpEffects()
    {
        animator?.SetTrigger("IsJumping");
        if (_audioManager != null)
        {
            int rand = Random.Range(1, 21);
            if (rand == 1) _audioManager.PlaySFX(_audioManager.CriSaut1);
            else if (rand == 2) _audioManager.PlaySFX(_audioManager.CriSaut2);
        }
    }

    // ---- Public API ----

    public void LockMovement(bool locked)
    {
        _isMovementLocked = locked;
        if (!locked) return;

        state = MovementState.idling;
        if (animator != null)
        {
            animator.SetBool("IsIdle", true);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsInAir", false);
        }
    }

    public void SetMotorActive(bool active)
    {
        Motor.enabled = active;
    }

    // ---- Gizmos ----

    private void OnDrawGizmosSelected()
    {
        if (!_isGrabbingLedge && orientation != null && ledgeGrabEnabled)
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(origin, ledgeDetectionRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(origin, orientation.forward * ledgeDetectionRadius);
        }
        else if (_isGrabbingLedge)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_ledgePosition, 0.2f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_grabTargetPosition, 0.15f);
        }
    }
}
