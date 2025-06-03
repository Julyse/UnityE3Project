using UnityEngine;

public class Zipline : MonoBehaviour
{
    [SerializeField] private Zipline targetZip;
    [SerializeField] private float zipSpeed = 5f;
    [SerializeField] private float zipScale = 0.2f;
    [SerializeField] private float arrivalThreshold = 0.1f;

    public Transform ZipTransform;

    private bool zipping = false;
    private GameObject localZip;

    private void Awake()
    {
    }

    private void Update()
    {
        if (!zipping || localZip == null) return;
        
        localZip.GetComponent<Rigidbody>().AddForce((targetZip.ZipTransform.position - ZipTransform.position).normalized * zipSpeed * Time.deltaTime, ForceMode.Acceleration);

        if (Vector3.Distance(localZip.transform.position, targetZip.ZipTransform.position) <= arrivalThreshold)
        {
            ResetZipline();
        }
    }

    public void StartZipline(GameObject player)
    {
        if (zipping) return;
        
        localZip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        localZip.transform.position = ZipTransform.position;
        localZip.transform.localScale = new Vector3(zipScale, zipScale, zipScale);
        localZip.AddComponent<Rigidbody>().useGravity = false;
        localZip.GetComponent<Collider>().isTrigger = true;

        player.GetComponent<Rigidbody>().useGravity = false;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        
        // Désactiver les composants de contrôle du joueur si ils existent
        var thirdPersonInput = player.GetComponent<MonoBehaviour>();
        if (thirdPersonInput != null && thirdPersonInput.GetType().Name == "vThirdPersonInput")
        {
            thirdPersonInput.enabled = false;
        }
        
        var thirdPersonController = player.GetComponent<MonoBehaviour>();
        if (thirdPersonController != null && thirdPersonController.GetType().Name == "vThirdPersonController")
        {
            thirdPersonController.enabled = false;
        }
        
        player.transform.parent = localZip.transform;
        zipping = true;
    }

    private void ResetZipline()
    {
        if (!zipping || localZip == null) return;

        if (localZip.transform.childCount > 0)
        {
            GameObject player = localZip.transform.GetChild(0).gameObject;
            player.GetComponent<Rigidbody>().useGravity = true;
            player.GetComponent<Rigidbody>().isKinematic = false;
            player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            
            // Réactiver les composants de contrôle du joueur si ils existent
            var thirdPersonInput = player.GetComponent<MonoBehaviour>();
            if (thirdPersonInput != null && thirdPersonInput.GetType().Name == "vThirdPersonInput")
            {
                thirdPersonInput.enabled = true;
            }
            
            var thirdPersonController = player.GetComponent<MonoBehaviour>();
            if (thirdPersonController != null && thirdPersonController.GetType().Name == "vThirdPersonController")
            {
                thirdPersonController.enabled = true;
            }
            
            player.transform.parent = null;
        }
        
        Destroy(localZip);
        localZip = null;
        zipping = false;
        Debug.Log("Zipline reset");
    }
}