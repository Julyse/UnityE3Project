using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody Rb;
    
    [Header("Camera Settings")]
    public float rotationSpeed = 7f;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;
    public float minDistance = 1f;
    public float collisionOffset = 0.3f;
    
    [Header("Collision Settings")]
    public LayerMask collisionLayers = -1; // What layers the camera should collide with
    public float sphereCastRadius = 0.2f; // Radius of the sphere cast for smoother collision
    
    private Vector3 cameraOffset;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Calculate initial camera offset
        cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
    }
    
    void Update()
    {
        if (PauseMenu.isPaused)
            return;
            
        // Handle player rotation
        HandlePlayerRotation();
    }
    
    void LateUpdate()
    {
        if (PauseMenu.isPaused)
            return;
            
        // Handle camera position and collision
        HandleCameraCollision();
    }
    
    void HandlePlayerRotation()
    {
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
    
    void HandleCameraCollision()
    {
        // Calculate desired camera position
        Vector3 desiredCameraPosition = player.position + player.rotation * cameraOffset;
        
        // Perform sphere cast from player to desired camera position
        RaycastHit hit;
        Vector3 direction = desiredCameraPosition - player.position;
        float distance = direction.magnitude;
        
        // Check if there's an obstacle between player and desired camera position
        if (Physics.SphereCast(player.position, sphereCastRadius, direction.normalized, out hit, distance, collisionLayers))
        {
            // Adjust camera position to avoid collision
            float newDistance = Mathf.Clamp(hit.distance - collisionOffset, minDistance, distance);
            transform.position = player.position + direction.normalized * newDistance;
        }
        else
        {
            // No collision, place camera at desired position
            transform.position = desiredCameraPosition;
        }
        
        // Make camera look at player
        transform.LookAt(player.position + Vector3.up * cameraHeight * 0.5f);
    }
    
    // Optional: Add smoothing for camera movement
    public void SetCameraDistance(float newDistance)
    {
        cameraDistance = Mathf.Clamp(newDistance, minDistance, 20f);
        cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
    }
    
    // Optional: Debug visualization
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, sphereCastRadius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(player.position, transform.position);
        }
    }
}