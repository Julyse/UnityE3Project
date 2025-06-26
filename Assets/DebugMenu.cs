using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DebugMenu : MonoBehaviour
{
    [Header("Configuration")]
    public KeyCode toucheDebug = KeyCode.F1;
    
    [Header("Checkpoints")]
    [Tooltip("Positions des 4 checkpoints")]
    public Transform[] checkpointPositions = new Transform[4];
    
    [Tooltip("Position de la fin du niveau")]
    public Transform positionFin;
    
    [Header("UI")]
    private bool menuOuvert = false;
    private GUIStyle styleMenu;
    private GUIStyle styleBouton;
    
    // Références
    private GameObject joueur;
    private Pause_Menu pauseMenu;
    private CharacterController characterController;
    
    private void Start()
    {
        // Trouver le joueur
        joueur = GameObject.FindGameObjectWithTag("Player");
        if (joueur == null)
        {
            Debug.LogError("DebugMenu: Aucun objet avec le tag 'Player' trouvé!");
        }
        
        // Trouver le Pause_Menu
        pauseMenu = FindObjectOfType<Pause_Menu>();
        
        // Trouver le CharacterController
        if (joueur != null)
        {
            characterController = joueur.GetComponent<CharacterController>();
        }
        
        // Vérifier les checkpoints assignés
        VerifierCheckpoints();
    }
    
    private void VerifierCheckpoints()
    {
        Debug.Log("=== Vérification des checkpoints ===");
        for (int i = 0; i < checkpointPositions.Length; i++)
        {
            if (checkpointPositions[i] != null)
            {
                Debug.Log($"Checkpoint {i + 1}: {checkpointPositions[i].name} à la position {checkpointPositions[i].position}");
            }
            else
            {
                Debug.LogWarning($"Checkpoint {i + 1} n'est pas assigné!");
            }
        }
        Debug.Log("===================================");
    }
    
    private void Update()
    {
        // Toggle du menu debug
        if (Input.GetKeyDown(toucheDebug))
        {
            menuOuvert = !menuOuvert;
            
            // Mettre en pause le jeu quand le menu est ouvert
            if (menuOuvert)
            {
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    private void OnGUI()
    {
        if (!menuOuvert) return;
        
        // Initialiser les styles s'ils ne le sont pas déjà
        if (styleMenu == null || styleBouton == null)
        {
            InitialiserStyles();
        }
        
        // Fenêtre du menu debug
        float largeur = 300f;
        float hauteur = 450f;
        float x = (Screen.width - largeur) / 2;
        float y = (Screen.height - hauteur) / 2;
        
        GUI.Box(new Rect(x, y, largeur, hauteur), "Menu Debug", styleMenu);
        
        float yOffset = 40f;
        
        // Bouton Recommencer
        if (GUI.Button(new Rect(x + 20, y + yOffset, largeur - 40, 40), "Recommencer le jeu", styleBouton))
        {
            RecommencerJeu();
        }
        yOffset += 50f;
        
        // Label pour les checkpoints
        GUI.Label(new Rect(x + 20, y + yOffset, largeur - 40, 30), "Téléportation aux Checkpoints:", styleMenu);
        yOffset += 35f;
        
        // Boutons pour tous les checkpoints
        for (int i = 0; i < checkpointPositions.Length; i++)
        {
            string nomBouton = $"Checkpoint {i + 1}";
            
            // Ajouter des infos de debug si le checkpoint est assigné
            if (checkpointPositions[i] != null)
            {
                nomBouton += " ✓";
            }
            else
            {
                nomBouton += " ✗";
            }
            
            if (GUI.Button(new Rect(x + 20, y + yOffset, largeur - 40, 35), nomBouton, styleBouton))
            {
                TeleporterAuCheckpoint(i);
            }
            yOffset += 40f;
        }
        
        yOffset += 10f;
        
        // Bouton pour aller à la fin
        if (GUI.Button(new Rect(x + 20, y + yOffset, largeur - 40, 40), "Aller à la fin", styleBouton))
        {
            TeleporterALaFin();
        }
        yOffset += 50f;
        
        // Bouton pour fermer le menu
        if (GUI.Button(new Rect(x + 20, y + yOffset, largeur - 40, 40), "Fermer (F1)", styleBouton))
        {
            menuOuvert = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void InitialiserStyles()
    {
        // Style pour le menu
        styleMenu = new GUIStyle(GUI.skin.box);
        styleMenu.fontSize = 16;
        styleMenu.fontStyle = FontStyle.Bold;
        styleMenu.alignment = TextAnchor.UpperCenter;
        styleMenu.normal.textColor = Color.white;
        
        // Style pour les boutons
        styleBouton = new GUIStyle(GUI.skin.button);
        styleBouton.fontSize = 14;
        styleBouton.fontStyle = FontStyle.Bold;
    }
    
    private void RecommencerJeu()
    {
        Time.timeScale = 1f;
        
        // Réinitialiser les checkpoints
        BananaCheckpoint[] tousLesCheckpoints = FindObjectsOfType<BananaCheckpoint>();
        foreach (var checkpoint in tousLesCheckpoints)
        {
            checkpoint.DesactiverMessageUI();
        }
        
        // Recharger la scène actuelle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void TeleporterAuCheckpoint(int index)
    {
        if (index < 0 || index >= checkpointPositions.Length)
        {
            Debug.LogError($"Index de checkpoint invalide: {index}");
            return;
        }
        
        if (checkpointPositions[index] == null)
        {
            Debug.LogError($"Checkpoint {index + 1} n'est pas assigné!");
            return;
        }
        
        // Log de debug pour vérifier la position
        Debug.Log($"Téléportation au checkpoint {index + 1} ({checkpointPositions[index].name}) à la position: {checkpointPositions[index].position}");
        
        TeleporterJoueur(checkpointPositions[index].position);
        
        // Mettre à jour le point de départ dans Pause_Menu
        if (pauseMenu != null)
        {
            pauseMenu.pointDeDepart = checkpointPositions[index].position;
        }
    }
    
    private void TeleporterALaFin()
    {
        if (positionFin == null)
        {
            Debug.LogError("Position de fin n'est pas assignée!");
            return;
        }
        
        TeleporterJoueur(positionFin.position);
        Debug.Log("Téléporté à la fin du niveau");
    }
    
    private void TeleporterJoueur(Vector3 position)
    {
        if (joueur == null)
        {
            Debug.LogError("Joueur non trouvé!");
            return;
        }
        
        // Log de debug
        Debug.Log($"Position actuelle du joueur: {joueur.transform.position}");
        Debug.Log($"Téléportation vers: {position}");
        
        // Désactiver temporairement le CharacterController pour la téléportation
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Téléporter le joueur
        joueur.transform.position = position;
        
        // Réactiver le CharacterController
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Vérifier que la téléportation a bien fonctionné
        Debug.Log($"Nouvelle position du joueur: {joueur.transform.position}");
        
        // Fermer le menu après téléportation
        menuOuvert = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

// Version améliorée de DebugMenuIntegration qui évite les conflits
[RequireComponent(typeof(DebugMenu))]
public class DebugMenuIntegration : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Utiliser la détection automatique des checkpoints?")]
    public bool detectionAutomatique = true;
    
    [Tooltip("Préfixe des noms de checkpoints à détecter")]
    public string prefixeCheckpoint = "BananaCheckpoint";
    
    private DebugMenu debugMenu;
    
    private void Awake()
    {
        debugMenu = GetComponent<DebugMenu>();
        
        if (detectionAutomatique)
        {
            // Attendre un frame pour s'assurer que tous les objets sont créés
            StartCoroutine(DetecterCheckpointsCoroutine());
        }
    }
    
    private System.Collections.IEnumerator DetecterCheckpointsCoroutine()
    {
        yield return null; // Attendre un frame
        
        DetecterCheckpoints();
    }
    
    private void DetecterCheckpoints()
    {
        // Ne pas écraser les checkpoints déjà assignés manuellement
        bool tousAssignes = true;
        for (int i = 0; i < debugMenu.checkpointPositions.Length; i++)
        {
            if (debugMenu.checkpointPositions[i] == null)
            {
                tousAssignes = false;
                break;
            }
        }
        
        if (tousAssignes)
        {
            Debug.Log("DebugMenuIntegration: Tous les checkpoints sont déjà assignés manuellement. Détection automatique ignorée.");
            return;
        }
        
        BananaCheckpoint[] tousLesCheckpoints = FindObjectsOfType<BananaCheckpoint>();
        
        if (tousLesCheckpoints.Length == 0)
        {
            Debug.LogWarning("DebugMenuIntegration: Aucun BananaCheckpoint trouvé dans la scène!");
            return;
        }
        
        // Trier les checkpoints par nom pour garantir un ordre cohérent
        System.Array.Sort(tousLesCheckpoints, (a, b) => 
        {
            // Essayer d'extraire les numéros des noms
            int numA = ExtraireNumero(a.name);
            int numB = ExtraireNumero(b.name);
            
            if (numA != -1 && numB != -1)
                return numA.CompareTo(numB);
            
            return a.name.CompareTo(b.name);
        });
        
        // Assigner les checkpoints détectés aux positions nulles uniquement
        int checkpointIndex = 0;
        for (int i = 0; i < debugMenu.checkpointPositions.Length && checkpointIndex < tousLesCheckpoints.Length; i++)
        {
            if (debugMenu.checkpointPositions[i] == null)
            {
                debugMenu.checkpointPositions[i] = tousLesCheckpoints[checkpointIndex].transform;
                Debug.Log($"DebugMenuIntegration: Checkpoint {i + 1} assigné automatiquement à {tousLesCheckpoints[checkpointIndex].name}");
                checkpointIndex++;
            }
        }
        
        Debug.Log($"DebugMenuIntegration: {checkpointIndex} checkpoints assignés automatiquement");
    }
    
    private int ExtraireNumero(string nom)
    {
        // Extraire le numéro du nom (ex: "BananaCheckpoint_3" -> 3)
        string[] parties = nom.Split('_', '-', ' ');
        foreach (string partie in parties)
        {
            if (int.TryParse(partie, out int numero))
            {
                return numero;
            }
        }
        
        // Chercher aussi des nombres dans le nom complet
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(nom, @"\d+");
        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        
        return -1;
    }
}