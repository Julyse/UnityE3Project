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
        if (Pause_Menu.isPaused || isCameraLocked)
            return;

        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        if (!isMovementLocked)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
                playerObject.forward = Vector3.Slerp(playerObject.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }
}