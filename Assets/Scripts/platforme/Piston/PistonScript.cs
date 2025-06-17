using UnityEngine;

public class PistonTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private PistonScript pistonScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            pistonScript.ActivatePiston();
        }
    }
}