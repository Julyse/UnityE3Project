using UnityEngine;
using TMPro;

public class PlayerDebugUI : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementTutorial playerMovement;
    public TextMeshProUGUI speedText;
    
    [Header("Display Settings")]
    public bool showDebugInfo = true;
    public int decimalPlaces = 1;
    
    private Rigidbody playerRb;
    
    private void Start()
    {
        // Get the player's rigidbody
        if (playerMovement != null)
        {
            playerRb = playerMovement.GetComponent<Rigidbody>();
        }
        
        // Hide cursor for better gameplay experience
        if (showDebugInfo)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void Update()
    {
        if (showDebugInfo && speedText != null && playerRb != null)
        {
            UpdateSpeedDisplay();
        }
        
        // Toggle debug info with F3 (common debug key)
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ToggleDebugInfo();
        }
    }
    
    private void UpdateSpeedDisplay()
    {
        // Calculate different speed components
        Vector3 horizontalVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        float horizontalSpeed = horizontalVelocity.magnitude;
        float verticalSpeed = playerRb.linearVelocity.y;
        float totalSpeed = playerRb.linearVelocity.magnitude;
        
        // Get grounded state (accessing the private grounded variable through reflection would be complex,
        // so we'll use a simple ground check here)
        bool isGrounded = Physics.CheckSphere(
            playerMovement.groundCheck.position, 
            playerMovement.groundDistance, 
            playerMovement.whatIsGround
        );
        
        // Format the display text with nice colors and formatting
        speedText.text = 
            $"<color=#00ff00><b>PLAYER DEBUG</b></color>\n" +
            $"<color=#ffffff>Horizontal: </color><color=#ffff00>{horizontalSpeed.ToString($"F{decimalPlaces}")}</color>\n" +
            $"<color=#ffffff>Vertical: </color><color={GetVerticalSpeedColor(verticalSpeed)}>{verticalSpeed.ToString($"F{decimalPlaces}")}</color>\n" +
            $"<color=#ffffff>Total Speed: </color><color=#cyan>{totalSpeed.ToString($"F{decimalPlaces}")}</color>\n" +
            $"<color=#ffffff>Grounded: </color><color={GetGroundedColor(isGrounded)}>{isGrounded}</color>\n" +
            $"<color=#gray><size=10>Press F3 to toggle</size></color>";
    }
    
    private string GetVerticalSpeedColor(float verticalSpeed)
    {
        if (verticalSpeed > 0.1f) return "#00ff00"; // Green for rising
        if (verticalSpeed < -0.1f) return "#ff4444"; // Red for falling
        return "#ffffff"; // White for stable
    }
    
    private string GetGroundedColor(bool grounded)
    {
        return grounded ? "#00ff00" : "#ff4444"; // Green if grounded, red if airborne
    }
    
    public void ToggleDebugInfo()
    {
        showDebugInfo = !showDebugInfo;
        
        if (speedText != null)
        {
            speedText.gameObject.SetActive(showDebugInfo);
        }
        
        // Toggle cursor lock when debug is on/off
        if (showDebugInfo)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}