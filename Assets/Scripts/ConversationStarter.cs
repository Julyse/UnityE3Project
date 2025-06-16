using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DialogueEditor;

public class ConversationStarter : MonoBehaviour
{
    [SerializeField] private NPCConversation myConversation;
    [SerializeField] private bool unlockMouseDuringConversation = true;

    [Header("First Encounter Settings")]
    [SerializeField] private bool autoStartFirstConversation = true;
    [SerializeField] private float autoStartDelay = 0.5f; // Délai avant de lancer la conversation auto
    private bool hasHadFirstConversation = false;
    private bool isPlayerInRange = false;

    [Header("Animation")]
    [SerializeField] private Animator monkeyAnimator;
    [SerializeField] private string animationBoolName = "IsConfused";

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactionPrompt; // Optionnel : UI pour afficher "Appuyez sur E"
    [SerializeField] private Camera dialogueCamera;
    [SerializeField] private Camera mainCamera;
    private void Start()
    {
        // Cache le prompt d'interaction au début si il existe
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Si c'est la première fois et que l'auto-start est activé
            if (!hasHadFirstConversation && autoStartFirstConversation)
            {
                // Lance la conversation après un court délai
                StartCoroutine(AutoStartFirstConversation());
            }
            else if (hasHadFirstConversation)
            {
                // Affiche le prompt d'interaction pour les fois suivantes
                ShowInteractionPrompt();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HideInteractionPrompt();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Ne permet l'interaction manuelle qu'après la première conversation
            if (hasHadFirstConversation && Input.GetKeyDown(KeyCode.E))
            {
                StartConversation();
            }
        }
    }

    private IEnumerator AutoStartFirstConversation()
    {
        // Attend un court instant pour que le joueur réalise qu'il est entré dans la zone
        yield return new WaitForSeconds(autoStartDelay);

        // Vérifie que le joueur est toujours dans la zone
        if (isPlayerInRange && !hasHadFirstConversation)
        {
            hasHadFirstConversation = true;
            StartConversation();
        }
    }

    private void StartConversation()
    {
        // Cache le prompt d'interaction pendant la conversation
        HideInteractionPrompt();

        // Déclenche l'événement de début de conversation
        GameEvents.TriggerConversationStart();

        // Gère le curseur
        if (unlockMouseDuringConversation)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Démarre l'animation
        StartConversationAnimation();

        // Démarre le dialogue
        ConversationManager.Instance.StartConversation(myConversation);

        // S'abonne à la fin de conversation
        DialogueEditor.ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    private void OnConversationEnded()
    {
        // Déclenche l'événement de fin de conversation
        GameEvents.TriggerConversationEnd();
        SwitchToDialogueCamera(false);
        // Arrête l'animation
        StopConversationAnimation();

        // Remet le curseur comme avant
        if (unlockMouseDuringConversation)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Se désabonne de l'événement
        DialogueEditor.ConversationManager.OnConversationEnded -= OnConversationEnded;

        // Si le joueur est toujours dans la zone après la conversation, affiche le prompt
        if (isPlayerInRange && hasHadFirstConversation)
        {
            ShowInteractionPrompt();
        }
    }

    private void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    private void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void StartConversationAnimation()
    {
        SwitchToDialogueCamera(true);

        if (monkeyAnimator != null)
        {
            monkeyAnimator.SetBool(animationBoolName, true);
            monkeyAnimator.SetTrigger("StartConv");
        }
    }

    private void StopConversationAnimation()
    {
        if (monkeyAnimator != null)
        {
            monkeyAnimator.SetBool(animationBoolName, false);
        }
    }

    private void OnDestroy()
    {
        // S'assure de se désabonner en cas de destruction de l'objet
        DialogueEditor.ConversationManager.OnConversationEnded -= OnConversationEnded;
    }

    // Méthode pour réinitialiser l'état (utile pour les tests ou les checkpoints)
    public void ResetFirstConversation()
    {
        hasHadFirstConversation = false;
    }

    // Méthode pour sauvegarder/charger l'état
    public bool HasHadFirstConversation
    {
        get { return hasHadFirstConversation; }
        set { hasHadFirstConversation = value; }
    }
    private void SwitchToDialogueCamera(bool isDialogue)
{
    if (dialogueCamera != null && mainCamera != null)
    {
        dialogueCamera.enabled = isDialogue;
        mainCamera.enabled = !isDialogue;
    }
}
}