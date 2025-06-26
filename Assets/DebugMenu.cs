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
        
        // Fenêtre du menu debug - augmentée pour accommoder le 4ème checkpoint
        float largeur = 300f;
        float hauteur = 450f; // Augmenté de 50px pour le 4ème checkpoint
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
        
        // Boutons pour tous les checkpoints (maintenant 4)
        for (int i = 0; i < checkpointPositions.Length; i++)
        {
            if (GUI.Button(new Rect(x + 20, y + yOffset, largeur - 40, 35), $"Checkpoint {i + 1}", styleBouton))
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
        
        TeleporterJoueur(checkpointPositions[index].position);
        
        // Mettre à jour le point de départ dans Pause_Menu
        if (pauseMenu != null)
        {
            pauseMenu.pointDeDepart = checkpointPositions[index].position;
        }
        
        Debug.Log($"Téléporté au checkpoint {index + 1}");
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
        
        // Fermer le menu après téléportation
        menuOuvert = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // Méthode utilitaire pour définir les positions des checkpoints par code
    public void DefinirCheckpoint(int index, Vector3 position)
    {
        if (index >= 0 && index < checkpointPositions.Length)
        {
            GameObject temp = new GameObject($"Checkpoint_{index + 1}_Position");
            temp.transform.position = position;
            checkpointPositions[index] = temp.transform;
        }
    }
}

// Extension optionnelle pour intégration automatique avec vos checkpoints existants
public class DebugMenuIntegration : MonoBehaviour
{
    private DebugMenu debugMenu;
    private List<BananaCheckpoint> checkpointsDetectes = new List<BananaCheckpoint>();
    
    private void Start()
    {
        debugMenu = GetComponent<DebugMenu>();
        if (debugMenu == null)
        {
            debugMenu = gameObject.AddComponent<DebugMenu>();
        }
        
        // Détecter automatiquement les BananaCheckpoints dans la scène
        DetecterCheckpoints();
    }
    
    private void DetecterCheckpoints()
    {
        BananaCheckpoint[] tousLesCheckpoints = FindObjectsOfType<BananaCheckpoint>();
        
        // Trier les checkpoints par nom ou position
        System.Array.Sort(tousLesCheckpoints, (a, b) => a.name.CompareTo(b.name));
        
        // Assigner les 4 premiers checkpoints trouvés (au lieu de 3)
        for (int i = 0; i < Mathf.Min(4, tousLesCheckpoints.Length); i++)
        {
            debugMenu.DefinirCheckpoint(i, tousLesCheckpoints[i].transform.position);
            checkpointsDetectes.Add(tousLesCheckpoints[i]);
        }
        
        Debug.Log($"DebugMenu: {checkpointsDetectes.Count} checkpoints détectés et assignés");
    }
}