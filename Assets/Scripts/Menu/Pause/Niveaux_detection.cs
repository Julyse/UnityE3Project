using UnityEngine;

public class ZoneNiveau : MonoBehaviour
{
    public string nomZone = "Zone inconnue";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DonneesManager.Instance.MettreAJourZone(nomZone);
        }
    }
}
