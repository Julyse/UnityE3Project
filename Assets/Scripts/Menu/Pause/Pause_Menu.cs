using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Menu : MonoBehaviour
{
    public GameObject PausePanelMenu;
    public GameObject PausePanelOption;
    public static bool isPaused;

    public Transform fakePlayerTransform; // Référence au joueur
    public Rigidbody fakePlayerRb;        // Référence au Rigidbody du joueur
    public Vector3 pointDeDepart;        // Position initiale mémorisée

    void Start()
    {
        PausePanelMenu.SetActive(false);
        PausePanelOption.SetActive(false);

        // Sauvegarder la position de départ du joueur
        pointDeDepart = fakePlayerTransform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        PausePanelMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None; // Déverrouille le curseur
        Cursor.visible = true;  
    }

    public void ResumeGame()
    {
        PausePanelMenu.SetActive(false);
        PausePanelOption.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked; // Verrouille le curseur (ex: FPS)
        Cursor.visible = false;                   // Cache le curseur

    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OuvrirOption()
    {
        PausePanelMenu.SetActive(false);
        PausePanelOption.SetActive(true);
    }

    public void FermerOption()
    {
        PausePanelMenu.SetActive(true);
        PausePanelOption.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void RestartLevel()
    {
        BananaCheckpoint.DesactiverTriggers();
        
        fakePlayerTransform.position = pointDeDepart;
        fakePlayerRb.linearVelocity = Vector3.zero;
        
        if (DonneesManager.Instance != null)
        {
            DonneesManager.Instance.MettreAJourPositionPrecedente();
        }
        
        BananaCheckpoint.ReactiverTriggers();
        
        Time.timeScale = 1f;
        isPaused = false;
        PausePanelMenu.SetActive(false);
        PausePanelOption.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}