using UnityEngine;

public class Zipline : MonoBehaviour
{
    private GameObject playerOnZip;
    private float verticalOffset = 1.5f;
    [SerializeField] private Zipline targetZip;
    [SerializeField] private float zipSpeed = 5f;
    [SerializeField] private float zipScale = 0.2f;
    [SerializeField] private float arrivalThreshold = 0.1f;

    [Header("Corde Visuelle")]
    [SerializeField] private bool showRope = true;
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private float ropeWidth = 0.05f;
    [SerializeField] private int ropeSegments = 20;
    [SerializeField] private float ropeSag = 0.5f;

    public Transform ZipTransform;

    private bool zipping = false;
    private GameObject localZip;
    private LineRenderer ropeRenderer;
    private Vector3[] ropePoints;

    private void Awake()
    {
        if (targetZip == null)
            Debug.LogError("TargetZip n'est pas assigné sur " + gameObject.name);
        if (ZipTransform == null)
            Debug.LogError("ZipTransform n'est pas assigné sur " + gameObject.name);
    }

    private void Start()
    {
        if (showRope && targetZip != null && ZipTransform != null && targetZip.ZipTransform != null)
            CreateRope();
    }

private void Update()
{
    if (!zipping || localZip == null) return;

    // Vérifications de sécurité pour éviter les NullReference
    if (targetZip == null || targetZip.ZipTransform == null)
    {
        Debug.LogError("TargetZip ou targetZip.ZipTransform est null dans Update!");
        ResetZipline();
        return;
    }

    Vector3 direction = (targetZip.ZipTransform.position - localZip.transform.position).normalized;

    Rigidbody zipRb = localZip.GetComponent<Rigidbody>();
    if (zipRb != null)
        zipRb.AddForce(direction * zipSpeed * Time.deltaTime, ForceMode.Acceleration);

    float distance = Vector3.Distance(localZip.transform.position, targetZip.ZipTransform.position);
    if (distance <= arrivalThreshold)
    {
        ResetZipline();
        return;
    }

    // Vérification critique : s'assurer que playerOnZip existe toujours
    if (playerOnZip == null)
    {
        Debug.LogWarning("PlayerOnZip est devenu null pendant la zipline!");
        ResetZipline();
        return;
    }

    // Mettre à jour la position du joueur
    playerOnZip.transform.position = localZip.transform.position + Vector3.down * verticalOffset;
    
    // Orienter le joueur vers la cible pendant le trajet (en world space)
    Vector3 directionToTarget = (targetZip.ZipTransform.position - playerOnZip.transform.position).normalized;
    directionToTarget.y = 0; // Garder seulement la rotation horizontale
    if (directionToTarget != Vector3.zero)
    {
        // Utiliser world rotation au lieu de local rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        playerOnZip.transform.rotation = Quaternion.Slerp(
            playerOnZip.transform.rotation, 
            targetRotation, 
            Time.deltaTime * 5f // Rotation plus rapide pour être sûr
        );
    }
}    private void LateUpdate()
    {
        if (showRope && ropeRenderer != null)
            UpdateRopePoints();
    }

    private void CreateRope()
    {
        GameObject ropeObject = new GameObject("ZiplineRope");
        ropeObject.transform.SetParent(transform);

        ropeRenderer = ropeObject.GetComponent<LineRenderer>();
        if (ropeRenderer == null)
            ropeRenderer = ropeObject.AddComponent<LineRenderer>();

        ropeRenderer.material = ropeMaterial != null ? ropeMaterial : CreateDefaultRopeMaterial();
        ropeRenderer.startWidth = ropeWidth;
        ropeRenderer.endWidth = ropeWidth;
        ropeRenderer.positionCount = ropeSegments;
        ropeRenderer.useWorldSpace = true;
        ropeRenderer.sortingOrder = 1;
        ropeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ropeRenderer.receiveShadows = false;

        UpdateRopePoints();
    }

    private Material CreateDefaultRopeMaterial()
    {
        Shader shader = Shader.Find("Standard") ??
                        Shader.Find("Legacy Shaders/Diffuse") ??
                        Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = new Color(0.4f, 0.3f, 0.2f, 1f);

        if (shader.name == "Standard")
        {
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Glossiness", 0.2f);
        }

        return mat;
    }

    private void UpdateRopePoints()
    {
        if (ropeRenderer == null || targetZip == null || ZipTransform == null || targetZip.ZipTransform == null)
            return;

        ropePoints = new Vector3[ropeSegments];
        Vector3 startPos = ZipTransform.position;
        Vector3 endPos = targetZip.ZipTransform.position;

        for (int i = 0; i < ropeSegments; i++)
        {
            float t = (float)i / (ropeSegments - 1);
            Vector3 linearPos = Vector3.Lerp(startPos, endPos, t);
            ropePoints[i] = linearPos;
        }

        ropeRenderer.SetPositions(ropePoints);
    }

public void StartZipline(GameObject player)
{
    if (zipping) return;

    if (targetZip == null || ZipTransform == null)
    {
        Debug.LogError("Configuration zipline incomplète");
        return;
    }

    localZip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    localZip.name = "ZiplineTransport";
    localZip.transform.position = ZipTransform.position;
    localZip.transform.localScale = new Vector3(zipScale, zipScale, zipScale);

    Rigidbody zipRb = localZip.AddComponent<Rigidbody>();
    zipRb.useGravity = false;
    zipRb.linearDamping = 1f;

    Collider zipCollider = localZip.GetComponent<Collider>();
    zipCollider.isTrigger = true;

    Rigidbody playerRb = player.GetComponent<Rigidbody>();
    if (playerRb != null)
    {
        playerRb.linearVelocity = Vector3.zero; // Remettre à zéro AVANT de passer en kinematic
        playerRb.useGravity = false;
        playerRb.isKinematic = true;
    }

    playerOnZip = player;

    // Attacher le joueur SANS changer sa rotation pour l'instant
    player.transform.SetParent(localZip.transform);
    player.transform.localPosition = new Vector3(0, -1.5f, 0);

    // Orienter le joueur vers la zipline cible APRÈS l'avoir attaché
    Vector3 directionToTarget = (targetZip.ZipTransform.position - player.transform.position).normalized;
    directionToTarget.y = 0; // Garder seulement la rotation horizontale
    if (directionToTarget != Vector3.zero)
    {
        // Rotation immédiate au début
        player.transform.rotation = Quaternion.LookRotation(directionToTarget);
        Debug.Log("Joueur orienté vers la cible : " + targetZip.name);
    }

    zipping = true;
}    private void DisablePlayerControls(GameObject player)
    {
        foreach (MonoBehaviour component in player.GetComponents<MonoBehaviour>())
        {
            string name = component.GetType().Name;
            if (name.Contains("Input") || name.Contains("Controller") || name.Contains("Movement"))
                component.enabled = false;
        }
    }

    private void EnablePlayerControls(GameObject player)
    {
        foreach (MonoBehaviour component in player.GetComponents<MonoBehaviour>())
        {
            string name = component.GetType().Name;
            if (name.Contains("Input") || name.Contains("Controller") || name.Contains("Movement"))
                component.enabled = true;
        }
    }
private void ResetZipline()
{
    if (!zipping || localZip == null) return;

    if (localZip.transform.childCount > 0)
    {
        GameObject player = localZip.transform.GetChild(0).gameObject;

        player.transform.SetParent(null);
        Vector3 arrivalPos = targetZip.ZipTransform.position;
        arrivalPos.y -= 1.5f;
        player.transform.position = arrivalPos;

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.isKinematic = false; // Remettre en mode physique AVANT de modifier la vélocité
            playerRb.useGravity = true;
            playerRb.linearVelocity = Vector3.zero; // Maintenant c'est safe

            playerRb.detectCollisions = false;
            playerOnZip = player;
            Invoke(nameof(EnablePlayerCollision), 0.05f);
        }

        // Call the ZiplinePlayer's EndZiplineAnimation method
        ZiplinePlayer ziplinePlayer = player.GetComponent<ZiplinePlayer>();
        if (ziplinePlayer != null)
        {
            ziplinePlayer.EndZiplineAnimation();
        }

        // Plus de EnablePlayerControls - on utilise les événements
    }

    Destroy(localZip);
    localZip = null;
    zipping = false;
}
    private void EnablePlayerCollision()
    {
        if (playerOnZip != null)
        {
            Rigidbody rb = playerOnZip.GetComponent<Rigidbody>();
            if (rb != null) rb.detectCollisions = true;
            playerOnZip = null;
        }
    }

    private void OnDestroy()
    {
        if (ropeRenderer != null)
            DestroyImmediate(ropeRenderer.gameObject);
    }

    private void OnDrawGizmos()
    {
        if (ZipTransform != null && targetZip != null && targetZip.ZipTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ZipTransform.position, targetZip.ZipTransform.position);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ZipTransform.position, 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetZip.ZipTransform.position, 0.5f);
        }
    }
}
