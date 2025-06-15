using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Pause_Menu pauseMenu;

    private void Start()
    {
       
        pauseMenu = FindObjectOfType<Pause_Menu>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pauseMenu.pointDeDepart = pauseMenu.fakePlayerTransform.position;
        }
    }
}
