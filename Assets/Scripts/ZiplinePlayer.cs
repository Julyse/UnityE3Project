using UnityEngine;
using UnityEngine.UI;

public class ZiplinePlayer : MonoBehaviour
{
    [SerializeField] private float checkOffset = 1f;
    [SerializeField] private float checkRadius = 2f;
    [SerializeField] private KeyCode ziplineKey = KeyCode.E;
    
    [Header("UI Message")]
    [SerializeField] private GameObject messageUI;
    [SerializeField] private Text messageText;
    [SerializeField] private string messageContent = "Appuyez sur E pour utiliser la zipline";
    
    private bool canUseZipline = false;
    private Zipline currentZipline = null;

    private void Update()
    {
        CheckForZipline();
        
        if (Input.GetKeyDown(ziplineKey) && canUseZipline && currentZipline != null)
        {
            Debug.Log("Tentative d'utilisation de la zipline");
            currentZipline.StartZipline(gameObject);
        }
    }
    
    private void CheckForZipline()
    {
        // Utilisation d'OverlapSphere au lieu de SphereCastAll
        Vector3 checkPosition = transform.position + new Vector3(0, checkOffset, 0);
        Collider[] colliders = Physics.OverlapSphere(checkPosition, checkRadius);
        
        bool foundZipline = false;
        
        foreach (Collider collider in colliders)
        {
            // Utilisation de CompareTag au lieu de ==
            if (collider.CompareTag("Zipline"))
            {
                foundZipline = true;
                currentZipline = collider.GetComponent<Zipline>();
                
                // Vérification que le composant Zipline existe
                if (currentZipline != null)
                {
                    Debug.Log("Zipline détectée : " + collider.name);
                    break;
                }
                else
                {
                    Debug.LogWarning("Objet avec tag Zipline mais sans composant Zipline : " + collider.name);
                }
            }
        }
        
        if (foundZipline && !canUseZipline)
        {
            ShowMessage(true);
            canUseZipline = true;
            Debug.Log("Zipline disponible");
        }
        else if (!foundZipline && canUseZipline)
        {
            ShowMessage(false);
            canUseZipline = false;
            currentZipline = null;
            Debug.Log("Zipline non disponible");
        }
    }
    
    private void ShowMessage(bool show)
    {
        if (messageUI != null)
        {
            messageUI.SetActive(show);
        }
        
        if (messageText != null)
        {
            messageText.text = messageContent;
        }
        
        if (show)
        {
            Debug.Log(messageContent);
        }
    }
    
    // Méthode pour visualiser la zone de détection dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 checkPosition = transform.position + new Vector3(0, checkOffset, 0);
        Gizmos.DrawWireSphere(checkPosition, checkRadius);
    }
}