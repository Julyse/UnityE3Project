using UnityEngine;
public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody Rb;
    public float rotationSpeed;
   
    // Système de verrouillage
    private bool isCameraLocked = false;
    private bool isMovementLocked = false; // Nouveau: verrouillage du mouvement
    public bool IsCameraLocked => isCameraLocked;
    public bool IsMovementLocked => isMovementLocked; // Getter public
    public Quaternion upZiplineRotation = Quaternion.Euler(0.028f, -473.974f, 1.038f); // Rotation pour la zipline en haut
    public Quaternion downZiplineRotation = Quaternion.Euler(0.432f, -294.058f, 0.053f);
   
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
   
    // Gestion des événements
    private void OnEnable()
    {
        GameEvents.OnCameraLockChanged += HandleCameraLockChanged;
        GameEvents.OnPlayerControlsLockChanged += HandleCameraLockChanged;
        GameEvents.OnPlayerMovementLockChanged += HandleMovementLockChanged; // Pour les ziplines et autres
    }
   
    private void OnDisable()
    {
        GameEvents.OnCameraLockChanged -= HandleCameraLockChanged;
        GameEvents.OnPlayerControlsLockChanged -= HandleCameraLockChanged;
        GameEvents.OnPlayerMovementLockChanged -= HandleMovementLockChanged; // Pour les ziplines et autres
    }
   
    private void HandleCameraLockChanged(bool isLocked)
    {
        LockCamera(isLocked);
    }

    private void HandleMovementLockChanged(bool isLocked) // Nouvelle méthode
    {
        LockMovement(isLocked);
    }
   
    // Méthode publique pour verrouiller/déverrouiller la caméra
    public void LockCamera(bool lockState)
    {
        isCameraLocked = lockState;
    }

    // Nouvelle méthode pour verrouiller/déverrouiller le mouvement
    public void LockMovement(bool lockState)
    {
        isMovementLocked = lockState;
    }
    public void DirectionFaceZip(bool direction)
    {
        // Cette méthode est appelée pour orienter le joueur vers la direction de la zipline
        if (direction)
        {
            // Si la zipline est en haut, oriente le joueur vers le haut
            playerObject.rotation = upZiplineRotation;
        }
        else
        {
            // Si la zipline est en bas, oriente le joueur vers le bas
            playerObject.rotation = downZiplineRotation;
        }
    }
   
    void LateUpdate()
    {
        if (Pause_Menu.isPaused || isCameraLocked) return;
        
        // Directly use the camera's forward vector for orientation
        Vector3 camForward = transform.forward;
        Vector3 flattenedForward = new Vector3(camForward.x, 0, camForward.z).normalized;
        
        if (flattenedForward.sqrMagnitude > 0.01f)
        {
            // Low-pass filter: Use RotateTowards to filter out micro-jitter from the camera's rotation
            // 360 degrees per second is snappy enough for input but blocks sub-frame 'vibrations'
            orientation.forward = Vector3.RotateTowards(orientation.forward, flattenedForward, 360f * Mathf.Deg2Rad * Time.deltaTime, 0f);
        }
    }

}