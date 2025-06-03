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
            currentZipline.StartZipline(gameObject);
        }
    }
    
    private void CheckForZipline()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + new Vector3(0, checkOffset, 0), checkRadius, Vector3.up);
        bool foundZipline = false;
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.tag == "Zipline")
            {
                foundZipline = true;
                currentZipline = hit.collider.GetComponent<Zipline>();
                break;
            }
        }
        
        if (foundZipline && !canUseZipline)
        {
            // Montrer le message
            ShowMessage(true);
            canUseZipline = true;
        }
        else if (!foundZipline && canUseZipline)
        {
            // Cacher le message
            ShowMessage(false);
            canUseZipline = false;
            currentZipline = null;
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
        
        // Alternative : utiliser Debug.Log si vous n'avez pas d'UI
        if (show)
        {
            Debug.Log(messageContent);
        }
    }
}