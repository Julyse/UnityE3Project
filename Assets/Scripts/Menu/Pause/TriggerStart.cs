using UnityEngine;

public class TriggerStart : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            DonneesManager.Instance.DemarrerChrono();
        }
    }
}