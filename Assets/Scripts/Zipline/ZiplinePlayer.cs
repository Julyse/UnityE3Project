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
   
    [Header("Animation")]
    [SerializeField] private string animatorBoolName = "IsOnZipline";
   
    private bool canUseZipline = false;
    private Zipline currentZipline = null;
    public Animator playerAnimator;
    private bool isOnZipline = false;

    private void Start()
    {
        // Get the animator component at start
        if (playerAnimator == null)
        {
            Debug.LogWarning("Aucun Animator trouvé sur " + gameObject.name);
        }
        else
        {
            Debug.Log("Animator trouvé sur " + gameObject.name);
            // Check if the parameter exists
            foreach (AnimatorControllerParameter param in playerAnimator.parameters)
            {
                if (param.name == animatorBoolName)
                {
                    Debug.Log("Paramètre " + animatorBoolName + " trouvé dans l'animator");
                    return;
                }
            }
            Debug.LogWarning("Paramètre " + animatorBoolName + " NON trouvé dans l'animator!");
        }
    }

    private void Update()
    {
        CheckForZipline();
       
        if (Input.GetKeyDown(ziplineKey) && canUseZipline && currentZipline != null && !isOnZipline)
        {
            Debug.Log("Tentative d'utilisation de la zipline");
            StartZiplineAnimation();
            currentZipline.StartZipline(gameObject);
        }
    }
   
    private void CheckForZipline()
    {
        // Don't check for ziplines if already on one
        if (isOnZipline) return;

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

    private void StartZiplineAnimation()
    {
        Debug.Log("StartZiplineAnimation appelée");
        isOnZipline = true;
        if (playerAnimator != null)
        {
            Debug.Log("Tentative de définir " + animatorBoolName + " à true");
            playerAnimator.SetBool(animatorBoolName, true);
            Debug.Log("Animation zipline activée - Valeur actuelle: " + playerAnimator.GetBool(animatorBoolName));
        }
        else
        {
            Debug.LogError("playerAnimator est null dans StartZiplineAnimation!");
        }

        // Déclencher l'événement zipline (simple, sans Transform)
        GameEvents.TriggerZiplineStart();
    }

    // This method should be called by the Zipline script when the zipline ends
    public void EndZiplineAnimation()
    {
        isOnZipline = false;
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(animatorBoolName, false);
            Debug.Log("Animation zipline désactivée");
        }

        // Déclencher l'événement de fin de zipline
        GameEvents.TriggerZiplineEnd();
    }

    // Public getter for other scripts to check if player is on zipline
    public bool IsOnZipline()
    {
        return isOnZipline;
    }
   
    // Méthode pour visualiser la zone de détection dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 checkPosition = transform.position + new Vector3(0, checkOffset, 0);
        Gizmos.DrawWireSphere(checkPosition, checkRadius);
    }
}