using UnityEngine;
using System.Collections;

public class PlateformeChute : MonoBehaviour
{
    [SerializeField] private float timeBeforeFall = 1.5f;
    [SerializeField] private float resetDelay = 3f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb.isKinematic = true; // pour qu’elle ne tombe pas au début
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isFalling && collision.collider.CompareTag("Player"))
        {
            StartCoroutine(FallThenReset());
        }
    }

    IEnumerator FallThenReset()
    {
        isFalling = true;
        yield return new WaitForSeconds(timeBeforeFall);

        rb.isKinematic = false; // active la chute physique

        yield return new WaitForSeconds(resetDelay);

        rb.isKinematic = true; // désactive la physique
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isFalling = false;
    }
}