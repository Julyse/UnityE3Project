using UnityEngine;

public class Zipline : MonoBehaviour
{
    [SerializeField] private Zipline targetZip;
    [SerializeField] private float zipSpeed = 5f;
    [SerializeField] private float zipScale = 0.2f;
    [SerializeField] private float arrivalThreshold = 0.1f;

    public Transform ZipTransform;

    private bool zipping = false;
    private GameObject localZip;

    private void Awake()
    {
        // Vérifications de configuration
        if (targetZip == null)
        {
            Debug.LogError("TargetZip n'est pas assigné sur " + gameObject.name);
        }
        
        if (ZipTransform == null)
        {
            Debug.LogError("ZipTransform n'est pas assigné sur " + gameObject.name);
        }
    }

    private void Update()
    {
        if (!zipping || localZip == null) return;
        
        // Calcul de la direction vers la cible
        Vector3 direction = (targetZip.ZipTransform.position - localZip.transform.position).normalized;
        
        // Application de la force
        Rigidbody zipRb = localZip.GetComponent<Rigidbody>();
        if (zipRb != null)
        {
            zipRb.AddForce(direction * zipSpeed * Time.deltaTime, ForceMode.Acceleration);
        }

        // Vérification de l'arrivée
        float distance = Vector3.Distance(localZip.transform.position, targetZip.ZipTransform.position);
        if (distance <= arrivalThreshold)
        {
            Debug.Log("Zipline terminée - Distance: " + distance);
            ResetZipline();
        }
    }

    public void StartZipline(GameObject player)
    {
        if (zipping)
        {
            Debug.Log("Zipline déjà en cours");
            return;
        }
        
        if (targetZip == null || ZipTransform == null)
        {
            Debug.LogError("Configuration zipline incomplète");
            return;
        }
        
        Debug.Log("Démarrage de la zipline de " + gameObject.name + " vers " + targetZip.gameObject.name);
        
        // Création de la sphère de transport
        localZip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        localZip.name = "ZiplineTransport";
        localZip.transform.position = ZipTransform.position;
        localZip.transform.localScale = new Vector3(zipScale, zipScale, zipScale);
        
        // Configuration de la physique
        Rigidbody zipRb = localZip.AddComponent<Rigidbody>();
        zipRb.useGravity = false;
        zipRb.linearDamping = 1f; // Ajout d'un peu de friction pour contrôler la vitesse
        
        Collider zipCollider = localZip.GetComponent<Collider>();
        zipCollider.isTrigger = true;

        // Configuration du joueur
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.useGravity = false;
            playerRb.isKinematic = true;
            playerRb.linearVelocity = Vector3.zero;
        }
        
        // Désactivation des contrôles du joueur
        DisablePlayerControls(player);
        
        // Attachement du joueur à la sphère de transport
        player.transform.SetParent(localZip.transform);
        player.transform.localPosition = Vector3.zero;
        
        zipping = true;
        Debug.Log("Zipline démarrée avec succès");
    }

    private void DisablePlayerControls(GameObject player)
    {
        // Recherche et désactivation des composants de contrôle
        MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();
        
        foreach (MonoBehaviour component in components)
        {
            string componentName = component.GetType().Name;
            if (componentName.Contains("Input") || componentName.Contains("Controller") || componentName.Contains("Movement"))
            {
                component.enabled = false;
                Debug.Log("Composant désactivé: " + componentName);
            }
        }
    }
    
    private void EnablePlayerControls(GameObject player)
    {
        // Recherche et réactivation des composants de contrôle
        MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();
        
        foreach (MonoBehaviour component in components)
        {
            string componentName = component.GetType().Name;
            if (componentName.Contains("Input") || componentName.Contains("Controller") || componentName.Contains("Movement"))
            {
                component.enabled = true;
                Debug.Log("Composant réactivé: " + componentName);
            }
        }
    }

    private void ResetZipline()
    {
        if (!zipping || localZip == null) return;

        Debug.Log("Réinitialisation de la zipline");

        // Récupération et libération du joueur
        if (localZip.transform.childCount > 0)
        {
            GameObject player = localZip.transform.GetChild(0).gameObject;
            
            // Positionnement du joueur à la destination
            player.transform.SetParent(null);
            player.transform.position = targetZip.ZipTransform.position;
            
            // Configuration de la physique du joueur
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.useGravity = true;
                playerRb.isKinematic = false;
                playerRb.linearVelocity = Vector3.zero;
            }
            
            // Réactivation des contrôles
            EnablePlayerControls(player);
        }
        
        // Nettoyage
        if (localZip != null)
        {
            Destroy(localZip);
        }
        
        localZip = null;
        zipping = false;
        Debug.Log("Zipline réinitialisée avec succès");
    }
    
    // Visualisation dans l'éditeur
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