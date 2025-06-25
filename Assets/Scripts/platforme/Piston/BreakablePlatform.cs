using UnityEngine;

public class PlateformeCasse : MonoBehaviour
{
    [SerializeField] private float delayBeforeFall = 0.5f;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            Invoke(nameof(Fall), delayBeforeFall);
        }
    }

    private void Fall()
    {
        rb.isKinematic = false;
    }
}