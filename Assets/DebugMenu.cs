using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DebugMenu : MonoBehaviour
{
    [Header("Debug Menu Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F3;
    [SerializeField] private GameObject debugMenuPanel;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform endPosition;
    
    private Pause_Menu pauseMenu;
    private List<Vector3> checkpointPositions = new List<Vector3>();
    private bool isDebugMenuOpen = false;
    
    // UI References
    [Header("UI Elements")]
    [SerializeField] private Button teleportStartButton;
    [SerializeField] private Button teleportEndButton;
    [SerializeField] private Button teleportCheckpointButton;
    [SerializeField] private Button resetGameButton;
    [SerializeField] private Dropdown checkpointDropdown;
    
    private void Start()
    {
        // Get references
        pauseMenu = FindObjectOfType<Pause_Menu>();
        
        // Setup UI buttons
        if (teleportStartButton != null)
            teleportStartButton.onClick.AddListener(TeleportToStart);
            
        if (teleportEndButton != null)
            teleportEndButton.onClick.AddListener(TeleportToEnd);
            
        if (teleportCheckpointButton != null)
            teleportCheckpointButton.onClick.AddListener(TeleportToSelectedCheckpoint);
            
        if (resetGameButton != null)
            resetGameButton.onClick.AddListener(ResetGame);
        
        // Hide menu at start
        if (debugMenuPanel != null)
            debugMenuPanel.SetActive(false);
            
        // Find all checkpoints in the scene
        RefreshCheckpointList();
    }
    
    private void Update()
    {
        // Toggle debug menu
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleDebugMenu();
        }
    }
    
    private void ToggleDebugMenu()
    {
        isDebugMenuOpen = !isDebugMenuOpen;
        
        if (debugMenuPanel != null)
        {
            debugMenuPanel.SetActive(isDebugMenuOpen);
            
            // Pause/unpause game
            Time.timeScale = isDebugMenuOpen ? 0f : 1f;
            
            // Update checkpoint list when opening menu
            if (isDebugMenuOpen)
            {
                RefreshCheckpointList();
            }
        }
    }
    
    private void RefreshCheckpointList()
    {
        checkpointPositions.Clear();
        
        if (checkpointDropdown != null)
        {
            checkpointDropdown.ClearOptions();
            List<string> options = new List<string>();
            
            // Find all checkpoints
            Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
            
            for (int i = 0; i < checkpoints.Length; i++)
            {
                checkpointPositions.Add(checkpoints[i].transform.position);
                options.Add($"Checkpoint {i + 1}");
            }
            
            if (options.Count > 0)
            {
                checkpointDropdown.AddOptions(options);
            }
            else
            {
                options.Add("No checkpoints found");
                checkpointDropdown.AddOptions(options);
            }
        }
    }
    
    private void TeleportToStart()
    {
        if (playerTransform != null && startPosition != null)
        {
            playerTransform.position = startPosition.position;
            Debug.Log("Teleported to start position");
        }
        else if (pauseMenu != null && playerTransform != null)
        {
            // Use pause menu's starting position as fallback
            playerTransform.position = pauseMenu.pointDeDepart;
            Debug.Log("Teleported to pause menu start position");
        }
        
        CloseDebugMenu();
    }
    
    private void TeleportToEnd()
    {
        if (playerTransform != null && endPosition != null)
        {
            playerTransform.position = endPosition.position;
            Debug.Log("Teleported to end position");
        }
        else
        {
            Debug.LogWarning("End position not set!");
        }
        
        CloseDebugMenu();
    }
    
    private void TeleportToSelectedCheckpoint()
    {
        if (checkpointDropdown != null && playerTransform != null)
        {
            int selectedIndex = checkpointDropdown.value;
            
            if (selectedIndex < checkpointPositions.Count)
            {
                playerTransform.position = checkpointPositions[selectedIndex];
                Debug.Log($"Teleported to checkpoint {selectedIndex + 1}");
            }
        }
        
        CloseDebugMenu();
    }
    
    private void ResetGame()
    {
        // Reset player position
        if (pauseMenu != null && playerTransform != null)
        {
            playerTransform.position = pauseMenu.pointDeDepart;
        }
        
        // Reset other game states here
        // For example:
        // - Reset player health
        // - Reset collected items
        // - Reset enemies
        
        Debug.Log("Game reset!");
        CloseDebugMenu();
    }
    
    private void CloseDebugMenu()
    {
        isDebugMenuOpen = false;
        if (debugMenuPanel != null)
            debugMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }
    
    // Public method to add checkpoint dynamically
    public void RegisterCheckpoint(Vector3 position)
    {
        if (!checkpointPositions.Contains(position))
        {
            checkpointPositions.Add(position);
            RefreshCheckpointList();
        }
    }
}