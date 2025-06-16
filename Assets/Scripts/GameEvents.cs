// GameEvents.cs - Système centralisé d'événements
using System;
using UnityEngine;

public static class GameEvents
{
    // Événements séparés pour un contrôle granulaire
    public static event Action<bool> OnPlayerMovementLockChanged;
    public static event Action<bool> OnCameraLockChanged;
    public static event Action<bool> OnPlayerControlsLockChanged;
    
    // Événements pour les états de conversation
    public static event Action OnConversationStarted;
    public static event Action OnConversationEnded;
    
    // Méthodes pour déclencher les événements
    public static void TriggerPlayerMovementLock(bool isLocked)
    {
        OnPlayerMovementLockChanged?.Invoke(isLocked);
    }
    
    public static void TriggerCameraLock(bool isLocked)
    {
        OnCameraLockChanged?.Invoke(isLocked);
    }
    
    // Verrouille tous les contrôles du joueur
    public static void TriggerPlayerControlsLock(bool isLocked)
    {
        OnPlayerControlsLockChanged?.Invoke(isLocked);
        OnPlayerMovementLockChanged?.Invoke(isLocked);
        OnCameraLockChanged?.Invoke(isLocked);
    }
    
    // Événements de conversation
    public static void TriggerConversationStart()
    {
        OnConversationStarted?.Invoke();
        TriggerPlayerControlsLock(true);
    }
    
    public static void TriggerConversationEnd()
    {
        OnConversationEnded?.Invoke();
        TriggerPlayerControlsLock(false);
    }
}