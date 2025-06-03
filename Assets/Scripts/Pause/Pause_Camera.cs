using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Pause_Camera : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Pause script démarré");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PostProcessVolume ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
            ppVolume.enabled = !ppVolume.enabled;
        }
    }
}



