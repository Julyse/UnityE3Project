using UnityEngine;

public class PlatformCollision : MonoBehaviour
{

    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform platform;
    [SerializeField] Ascenseur movingPlatform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(playerTag))
        {
            other.gameObject.transform.parent = platform;
            movingPlatform.StartMoving();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals(playerTag))
        {
            other.gameObject.transform.parent = null;
        }
        
    }
   
   
}


