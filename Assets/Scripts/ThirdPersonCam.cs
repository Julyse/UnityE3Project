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
    public bool IsCameraLocked => isCameraLocked;
    
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
    }
    
    private void OnDisable()
    {
        GameEvents.OnCameraLockChanged -= HandleCameraLockChanged;
        GameEvents.OnPlayerControlsLockChanged -= HandleCameraLockChanged;
    }
    
    private void HandleCameraLockChanged(bool isLocked)
    {
        LockCamera(isLocked);
    }
    
    // Méthode publique pour verrouiller/déverrouiller la caméra
    public void LockCamera(bool lockState)
    {
        isCameraLocked = lockState;
    }
    
    void Update()
    {
        // Vérifie si le jeu est en pause ou si la caméra est verrouillée
        if (Pause_Menu.isPaused || isCameraLocked)
            return;
            
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
        
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if(inputDir != Vector3.zero)
        {
            playerObject.forward = Vector3.Slerp(playerObject.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }
}