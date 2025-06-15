using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DialogueEditor;
public class ConversationStarter : MonoBehaviour
{
    [SerializeField] private NPCConversation myConversation; // Référence au scriptable object Dialogue
    [SerializeField] private bool unlockMouseDuringConversation = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerStay(Collider other)
    {
        // Vérifie si le joueur est entré dans le trigger
        if (other.CompareTag("Player"))
        {
            // Vérifie si la touche "E" est pressée
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Démarre le dialogue
                if (unlockMouseDuringConversation)
{
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}
                ConversationManager.Instance.StartConversation(myConversation);
            }

        }
    }
}
