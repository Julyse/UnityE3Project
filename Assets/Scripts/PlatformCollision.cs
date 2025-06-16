using UnityEngine;

public class PlatformCollision : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform platform;
    [SerializeField] Ascenseur movingPlatform;
    
    [Header("Méthode non-invasive")]
    [SerializeField] bool useGroundCheck = true;
    [SerializeField] LayerMask platformLayer = 1;
    
    private bool playerOnPlatform = false;
    private Transform playerTransform;
    private Vector3 lastPlatformPosition;
    private Vector3 platformOffset;

    void Start()
    {
        if (platform == null)
            platform = transform;
        
        if (movingPlatform == null)
            movingPlatform = FindObjectOfType<Ascenseur>();
            
        if (platform != null)
            lastPlatformPosition = platform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerTransform = other.transform;
            playerOnPlatform = true;
            
            // NE FAIT RIEN au joueur - juste démarre la plateforme
            if (movingPlatform != null)
                movingPlatform.StartMoving();
                
            lastPlatformPosition = platform.position;
            
            // Calcule l'offset relatif initial
            platformOffset = playerTransform.position - platform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerOnPlatform = false;
            playerTransform = null;
        }
    }
    
    void LateUpdate()
    {
        // Cette méthode n'affecte PAS directement le joueur
        // Elle peut être utilisée pour des effets visuels ou des calculs
        if (playerOnPlatform && playerTransform != null && platform != null)
        {
            Vector3 platformMovement = platform.position - lastPlatformPosition;
            lastPlatformPosition = platform.position;
            
            // Optionnel: Log pour debug
            if (platformMovement.magnitude > 0.001f)
            {
                // Debug.Log("Platform moved: " + platformMovement);
            }
        }
    }
    
    // Méthodes publiques pour que d'autres scripts puissent savoir si le joueur est sur la plateforme
    public bool IsPlayerOnPlatform()
    {
        return playerOnPlatform;
    }
    
    public Vector3 GetPlatformVelocity()
    {
        if (!playerOnPlatform || platform == null) return Vector3.zero;
        
        Vector3 platformMovement = platform.position - lastPlatformPosition;
        return platformMovement / Time.deltaTime;
    }
    
    public Transform GetPlatform()
    {
        return platform;
    }
}