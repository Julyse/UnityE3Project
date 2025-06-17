using UnityEngine;

public class BananaCheckpoint : MonoBehaviour
{
    [Header("Références")]
    public GameObject bananeNormale;    
    public GameObject bananeDore;       
    
    [Header("Interaction")]
    public KeyCode toucheInteraction = KeyCode.E;
    public float distanceInteraction = 2f;
    
    [Header("UI Message")]
    public GameObject messageUI;
    
    private Pause_Menu pauseMenu;
    private bool estCheckpointActif = false;
    private bool joueurDansZone = false;
    private GameObject joueur;
    private Sound_Music audioManager;
    private static bool triggersDesactives = false;
    
    private static BananaCheckpoint checkpointActuel = null;
    
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<Sound_Music>();
    }
    
    private void Start()
    {
        pauseMenu = FindFirstObjectByType<Pause_Menu>();
        
        bananeNormale.SetActive(true);
        bananeDore.SetActive(false);
        if (messageUI != null)
        {
            messageUI.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (joueurDansZone && Input.GetKeyDown(toucheInteraction))
        {
            ActiverCheckpoint();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggersDesactives)
        {
            joueurDansZone = true;
            joueur = other.gameObject;
            
            if (messageUI != null && !estCheckpointActif)
            {
                messageUI.SetActive(true);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            joueurDansZone = false;
            joueur = null;
            
            if (messageUI != null)
            {
                messageUI.SetActive(false);
            }
        }
    }
    
    private void ActiverCheckpoint()
    {
        if (checkpointActuel != null && checkpointActuel != this)
        {
            checkpointActuel.DesactiverCheckpoint();
        }
        
        estCheckpointActif = true;
        checkpointActuel = this;
        
        bananeNormale.SetActive(false);
        bananeDore.SetActive(true);
        
        pauseMenu.pointDeDepart = pauseMenu.fakePlayerTransform.position;
        
        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.Checkpoint);
        }
        
        if (messageUI != null)
        {
            messageUI.SetActive(false);
        }
    }
    
    private void DesactiverCheckpoint()
    {
        estCheckpointActif = false;
        
        bananeNormale.SetActive(true);
        bananeDore.SetActive(false);
    }
    
    // Méthode publique pour forcer la désactivation du message UI
    public void DesactiverMessageUI()
    {
        if (messageUI != null)
        {
            messageUI.SetActive(false);
        }
    }
    
    // Méthode statique pour désactiver tous les messages UI des checkpoints
    public static void DesactiverTousLesMessagesUI()
    {
        BananaCheckpoint[] tousCheckpoints = FindObjectsOfType<BananaCheckpoint>();
        foreach (BananaCheckpoint checkpoint in tousCheckpoints)
        {
            checkpoint.DesactiverMessageUI();
        }
    }
}