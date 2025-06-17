using UnityEngine;

public class TriggerFinish : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            DonneesManager.Instance.ArreterChrono();
        }
    }
}