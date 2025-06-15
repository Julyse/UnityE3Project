using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ManegeRotation : MonoBehaviour
{
    [Header("Vitesse de rotation (en degrés par seconde)")]
    [SerializeField] private float rotationSpeed = 30f;

    void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }
}