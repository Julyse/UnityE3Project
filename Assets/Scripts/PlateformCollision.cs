using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform platform;
    [SerializeField] MovingPlateform movingPlateform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(playerTag))
        {
            other.gameObject.transform.parent = platform;
            movingPlateform.StratMoving();
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
